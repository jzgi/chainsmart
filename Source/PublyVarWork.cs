using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainMart
{
    /// 
    /// The home for a certain market
    ///
    [UserAuthenticate]
    public class PublyVarWork : WebWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyVarVarWork>(); // home for shop
        }

        public async Task @default(WebContext wc, int sec)
        {
            int mktid = wc[0];
            var mkt = GrabObject<int, Org>(mktid);
            var regs = Grab<short, Reg>();

            Org[] arr;
            if (sec == 0) // when default section
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 AND regid IS NULL AND status = 4 ORDER BY addr");
                arr = await dc.QueryAsync<Org>(p => p.Set(mktid));
                arr = arr.AddOf(mkt, first: true);
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 AND regid = @2 AND status = 4 ORDER BY addr");
                arr = await dc.QueryAsync<Org>(p => p.Set(mktid).Set(sec));
            }

            wc.GivePage(200, h =>
            {
                h.NAVBAR(string.Empty, sec, regs, (k, v) => v.IsSection, "star");

                if (arr == null)
                {
                    h.ALERT("尚无商户");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    if (o.EqBrand)
                    {
                        h.A_(o.addr, css: "uk-card-body uk-flex");
                    }
                    else
                    {
                        h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.ShopName, css: "uk-card-body uk-flex");
                    }

                    if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/org/").T(o.id).T("/icon")._PIC();
                    }
                    else
                        h.PIC("/void.webp", css: "uk-width-1-5");

                    h.ASIDE_();
                    h.HEADER_().H5(o.ShopName).SPAN("")._HEADER();
                    h.Q(o.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            }, shared: sec > 0, 900, mkt.name); // shared cache when no personal data
        }
    }

    /// 
    /// The home for shop
    ///
    public class PublyVarVarWork : WebWork
    {
        public async Task @default(WebContext wc)
        {
            int shpid = wc[0];
            var shp = GrabObject<int, Org>(shpid);

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares_vw WHERE shpid = @1 AND status = 4 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Ware>(p => p.Set(shp.id));

            wc.GivePage(200, h =>
            {
                if (arr == null)
                {
                    h.ALERT("暂无商品");
                    return;
                }

                decimal realprice = 0;

                h.FORM_(oninput: $"pay.value = {realprice} * parseInt(unitx.value) * parseInt(qty.value);");

                h.MAINGRID(arr, o =>
                {
                    h.DIV_("uk-card-body uk-flex");
                    if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/ware/").T(o.id).T("/icon")._PIC();
                    }
                    else if (o.itemid > 0)
                    {
                        h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();
                    }
                    else
                    {
                        h.PIC("/void.webp", css: "uk-width-1-5");
                    }

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN(Item.Statuses[o.status], "uk-badge")._HEADER();
                    h.Q(o.tip, "uk-width-expand");
                    
                    h.FOOTER_();

                    decimal qtyx = 1 * o.unitx;
                    h.SPAN_("uk-width-1-4").CNYOUTPUT(nameof(o.price), o.price, cookie: "vip", onfix: $"fixPrice(this,event,{o.RealPrice},{o.price});")._SPAN();
                    
                    h.SELECT_(o.id, onfix: $"qtyFill(this,{o.min},{o.max},{o.step});", css: "uk-width-1-6 uk-height-auto uk-border-rounded")._SELECT();
                    h.SPAN_("uk-width-expand").T("件共").SP().OUTPUT(nameof(qtyx), qtyx).SP().T(o.unit)._SPAN();

                    h.SPAN_("uk-margin-auto-left")._SPAN();
                    h._FOOTER();
                    
                    h._ASIDE();

                    h._DIV();
                });

                decimal pay = 0;
                string nametel = null;
                h.BOTTOMBAR_();

                h.DIV_("uk-width-1-1");

                h.OUTPUT(nameof(nametel), nametel, cookie: "nametel", onfix: "this.value = event.detail;", css: "uk-width-1-1 uk-background-default").SP();

                h.T("<input type=\"text\" name=\"addr\" class=\"uk-input\" placeholder=\"同城收货地址（街道／小区／室号）\" cookie=\"addr\" fix=\"this.value = event.detail;\" required>");
                h._DIV();

                h.BUTTON_(nameof(buy), css: "uk-button-danger uk-width-medium uk-height-1-1", onclick: "return call_buy(this);\">").CNYOUTPUT(nameof(pay), pay)._BUTTON();
                h._BOTTOMBAR();

                h._FORM();
            }, true, 300, title: shp.name, onload: "fixAll();");
        }

        public async Task buy(WebContext wc, int cmd)
        {
            int shpid = wc[-1];
            var shp = GrabObject<int, Org>(shpid);
            var prin = (User) wc.Principal;

            var f = await wc.ReadAsync<Form>();
            string addr = f[nameof(addr)];

            // line item list
            var linelst = new List<BuyDetail>();
            for (int i = 0; i < f.Count; i++)
            {
                var ety = f.EntryAt(i);
                int id = ety.Key.ToInt();
                short qty = ety.Value;
                linelst.Add(new BuyDetail
                {
                    wareid = id,
                    qty = qty
                });
            }

            using var dc = NewDbContext(IsolationLevel.ReadCommitted);
            try
            {
                dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares WHERE shpid = @1 AND id ")._IN_(linelst);
                var wares = await dc.QueryAsync<int, Ware>(p => p.Set(shpid).SetForIn(linelst));

                for (int i = 0; i < linelst.Count; i++)
                {
                    var line = linelst[i];
                    var ware = wares[line.wareid];
                    if (ware != null)
                    {
                        line.InitWithWare(ware, discount: prin.vip == shpid);
                    }
                }

                var m = new Buy
                {
                    typ = Buy.TYP_ONLINE,
                    name = shp.name,
                    created = DateTime.Now,
                    creator = prin.name,
                    shpid = shp.id,
                    mktid = shp.MarketId,
                    details = linelst.ToArray(),
                    uid = prin.id,
                    utel = prin.tel,
                    uaddr = addr
                };

                // make use of any existing abandoned record
                const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
                dc.Sql("INSERT INTO buys ").colset(Buy.Empty, msk)._VALUES_(Buy.Empty, msk).T(" ON CONFLICT (shpid, status) WHERE status = 0 DO UPDATE ")._SET_(Buy.Empty, msk).T(" RETURNING id, pay");
                await dc.QueryTopAsync(p => m.Write(p, msk));
                dc.Let(out int buyid);
                dc.Let(out decimal topay);

                // // call WeChatPay to prepare order there
                string trade_no = (buyid + "-" + topay).Replace('.', '-');
                var (prepay_id, _) = await WeixinUtility.PostUnifiedOrderAsync(SC: false,
                    trade_no,
                    topay,
                    prin.im, // the payer
                    wc.RemoteIpAddress.ToString(),
                    MainApp.MgtUrl + "/" + nameof(WwwService.onpay),
                    MainApp.MgtUrl
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
                Application.Err(e.Message);
                wc.Give(500);
            }
        }
    }
}