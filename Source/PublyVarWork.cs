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

                decimal fprice = 0;

                h.FORM_(oninput: $"pay.value = {fprice} * parseInt(unitx.value) * parseInt(qty.value);");

                h.MAINGRID(arr, o =>
                {
                    h.SECTION_("uk-card-body uk-flex");
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

                    h.HEADER_().H5(o.name);
                    if (o.unitx != 1)
                    {
                        h.T('（').T(o.unitx).T(o.unit).T("件").T('）');
                    }
                    // top right corner span
                    h.SPAN_(css: "uk-badge");
                    // ran mark
                    short rank = 0;
                    if (o.itemid > 0)
                    {
                        var item = GrabObject<int, Item>(o.itemid);
                        if (item.state > 0)
                        {
                            rank = item.state;
                        }
                    }
                    h.MARK(Item.States[rank], "state", rank);
                    h._SPAN();
                    h._HEADER();

                    h.Q(o.tip, "uk-width-expand");

                    // FOOTER: price and qty select & detail
                    h.T($"<footer cookie= \"vip\" onfix=\"fixPrice(this,event,{o.price},{o.off});\">"); // pricing portion
                    h.SPAN_("uk-width-1-3").T("<output class=\"rmb fprice\"></output>&nbsp;<sub>").T(o.unit).T("</sub>")._SPAN();
                    h.SELECT_(o.id, onfix: $"qtyFill(this,{o.min},{o.max},{o.step});", onchange: $"sumQtyDetails(this,{o.unitx});", css: "uk-width-1-6 qtyselect ").OPTION((short) 0, "不选")._SELECT().SPAN("件&nbsp;", css: "uk-background-muted uk-height-1-1");
                    h.SPAN_("qtydetail uk-invisible").T("&nbsp;<output class=\"qtyx\"></output>&nbsp;").T(o.unit).T("<output class=\"rmb subtotal uk-width-expand uk-text-end\"></output>")._SPAN();
                    h._FOOTER();

                    h._ASIDE();

                    h._SECTION();
                });

                var topay = 0.00M;

                h.BOTTOMBAR_(large: true);

                h.DIV_("uk-col");
                h.T("<output class=\"nametel\" name=\"nametel\" cookie=\"nametel\" onfix=\"this.value = event.detail;\"></output>");
                h.T("<input type=\"text\" name=\"addr\" class=\"uk-input\" placeholder=\"请填收货地址（限离市场２公里内）\" cookie=\"addr\" onfix=\"this.value = event.detail;\" required>");
                h._DIV();

                h.BUTTON_(nameof(buy), css: "uk-button-danger uk-width-medium uk-height-1-1", onclick: "return call_buy(this);").CNYOUTPUT(nameof(topay), topay)._BUTTON();

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

            // detail list
            var details = new List<BuyDetail>();
            for (int i = 0; i < f.Count; i++)
            {
                var ety = f.EntryAt(i);
                int id = ety.Key.ToInt();
                short qty = ety.Value;

                if (id <= 0 || qty <= 0) // filter out the non-selected (but submitted)
                {
                    continue;
                }

                details.Add(new BuyDetail
                {
                    wareid = id,
                    qty = qty
                });
            }

            using var dc = NewDbContext(IsolationLevel.ReadCommitted);
            try
            {
                dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares WHERE shpid = @1 AND id ")._IN_(details);
                var map = await dc.QueryAsync<int, Ware>(p => p.Set(shpid).SetForIn(details));

                for (int i = 0; i < details.Count; i++)
                {
                    var dtl = details[i];
                    var ware = map[dtl.wareid];
                    if (ware != null)
                    {
                        dtl.SetupWithWare(ware, offed: prin.vip?.Contains(shpid) ?? false);
                    }
                }

                var m = new Buy
                {
                    typ = Buy.TYP_ONLINE,
                    name = shp.ShopName,
                    created = DateTime.Now,
                    creator = prin.name,
                    shpid = shp.id,
                    mktid = shp.MarketId,
                    details = details.ToArray(),
                    uid = prin.id,
                    uname = prin.name,
                    utel = prin.tel,
                    uim = prin.im,
                    uaddr = addr,
                };
                m.SetToPay();

                // NOTE single unsubmitted record
                const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
                dc.Sql("INSERT INTO buys ").colset(Buy.Empty, msk)._VALUES_(Buy.Empty, msk).T(" ON CONFLICT (shpid, status) WHERE status = 0 DO UPDATE ")._SET_(Buy.Empty, msk).T(" RETURNING id, topay");
                await dc.QueryTopAsync(p => m.Write(p, msk));
                dc.Let(out int buyid);
                dc.Let(out decimal topay);

                // // call WeChatPay to prepare order there
                string trade_no = (buyid + "-" + topay).Replace('.', '-');
                var (prepay_id, err_code) = await WeixinUtility.PostUnifiedOrderAsync(sc: false,
                    trade_no,
                    topay,
                    prin.im, // the payer
                    wc.RemoteIpAddress.ToString(),
                    MainApp.WwwUrl + "/" + nameof(WwwService.onpay),
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
                Application.Err(e.Message);
                wc.Give(500);
            }
        }
    }
}