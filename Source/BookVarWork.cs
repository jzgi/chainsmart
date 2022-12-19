using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Application;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public abstract class BookVarWork : WebWork
    {
    }

    public class ShplyBookVarWork : BookVarWork
    {
        public async Task @default(WebContext wc)
        {
            int lotid = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1");
            var o = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

            wc.GivePane(200, h =>
            {
                // picture
                h.PIC_().T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/pic")._PIC();

                h.DIV_("uk-card uk-card-default");
                h._DIV();

                // bottom bar
                //
                decimal realprice = o.RealPrice;
                short qty = o.min;
                decimal unitx = o.unitx;
                decimal qtyx = qty * unitx;
                decimal topay = qtyx * o.RealPrice;

                h.BOTTOMBAR_();
                h.FORM_("uk-flex uk-width-1-1", oninput: $"qtyx.value = qty.value * {unitx}; topay.value = {realprice} * parseInt(qtyx.value);");

                h.HIDDEN(nameof(realprice), realprice);

                h.SELECT_(null, nameof(qty), css: "uk-width-small");
                for (int i = o.min; i < o.max; i += o.step)
                {
                    h.OPTION_(i).T(i)._OPTION();
                }
                h._SELECT().SP().SPAN_("uk-width-expand").T("件，共").SP();
                h.OUTPUT(nameof(qtyx), qtyx).SP().T(o.unit)._SPAN();

                // pay button
                h.BUTTON_(nameof(book), css: "uk-button-danger uk-width-medium").OUTPUTCNY(nameof(topay), topay)._BUTTON();

                h._FORM();
                h._BOTTOMBAR();
            });
        }

        public async Task book(WebContext wc, int cmd)
        {
            var shp = wc[-2].As<Org>();
            int lotid = wc[0];

            var prin = (User) wc.Principal;

            // submitted values
            var f = await wc.ReadAsync<Form>();
            short qty = f[nameof(qty)];

            using var dc = NewDbContext(IsolationLevel.ReadCommitted);
            try
            {
                dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1");
                var lot = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

                dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE id = @1");
                var item = await dc.QueryTopAsync<Item>(p => p.Set(lot.itemid));

                var m = new Book
                {
                    typ = lot.typ,
                    name = lot.name,
                    created = DateTime.Now,
                    creator = prin.name,
                    shpid = shp.id,
                    shpname = shp.name,
                    mktid = shp.MarketId,
                    srcid = lot.srcid,
                    srcname = lot.srcname,
                    zonid = lot.zonid,
                    ctrid = lot.ctrid,
                    itemid = lot.itemid,
                    lotid = lot.id,
                    unit = lot.unit,
                    unitx = lot.unitx,
                    price = lot.price,
                    off = lot.off,
                    qty = qty,
                    topay = lot.RealPrice * qty * lot.unitx
                };

                // make use of any existing abandoned record
                const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
                dc.Sql("INSERT INTO books ").colset(Book.Empty, msk)._VALUES_(Book.Empty, msk).T(" ON CONFLICT (shpid, status) WHERE status = 0 DO UPDATE ")._SET_(Book.Empty, msk).T(" RETURNING id, topay");
                await dc.QueryTopAsync(p => m.Write(p));
                dc.Let(out int bookid);
                dc.Let(out decimal topay);


                // call WeChatPay to prepare order there
                string trade_no = (bookid + "-" + topay).Replace('.', '-');
                var (prepay_id, _) = await WeixinUtility.PostUnifiedOrderAsync(SC: true,
                    trade_no,
                    topay,
                    prin.im, // the payer
                    wc.RemoteIpAddress.ToString(),
                    MainApp.MgtUrl + "/" + nameof(MgtService.onpay),
                    m.ToString()
                );
                if (prepay_id != null)
                {
                    wc.Give(200, WeixinUtility.BuildPrepayContent(prepay_id));
                }
                else
                {
                    dc.Rollback();
                    wc.Give(500);
                }
            }
            catch (Exception e)
            {
                dc.Rollback();
                Err(e.Message);
                wc.Give(500);
            }
        }
    }

    public class SrclyBookVarWork : BookVarWork
    {
        [Ui, Tool(ButtonOpen)]
        public async Task act(WebContext wc, int cmd)
        {
            int lotid = wc[0];
            short orgid = wc[-2];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Book.Empty).T(" FROM lots_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Book>(p => p.Set(lotid));
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                int[] key = f[nameof(key)];
                if (cmd == 1)
                {
                    using var dc = NewDbContext(IsolationLevel.ReadCommitted);
                    try
                    {
                    }
                    catch (Exception e)
                    {
                        Err(e.Message);
                        dc.Rollback();
                    }
                }
                else
                {
                }

                wc.GiveRedirect(nameof(act));
            }
        }

        [Ui("修改", group: 1), Tool(ButtonOpen)]
        public async Task upd(WebContext wc)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-2];
            int id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Book.Empty).T(" FROM lots_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Book>(p => p.Set(id));
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                short typ = f[nameof(typ)];
                var m = new Book
                {
                };
                m.Read(f);
                using var dc = NewDbContext();
                dc.Sql("UPDATE lots ")._SET_(m, 0).T(" WHERE id = @1");
                await dc.ExecuteAsync(p =>
                {
                    m.Write(p, 0);
                    p.Set(id);
                });

                wc.GivePane(201);
            }
        }
    }

    public class CtrlyBookVarWork : BookVarWork
    {
    }


    public class MktlyBookVarWork : BookVarWork
    {
    }
}