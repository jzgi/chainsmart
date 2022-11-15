using System.Threading.Tasks;
using ChainFx.Web;
using static ChainMart.User;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public abstract class BuyWork<V> : WebWork where V : BuyVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    [Ui("零售订单", "个人")]
    public class MyBuyWork : BuyWork<MyBuyVarWork>
    {
        [Ui("零售订单"), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE uid = @1 AND status > 0 ORDER BY id DESC LIMIT 10 OFFSET 10 * @2");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(prin.id).Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("尚无零售订单");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.HEADER_("uk-card-header").T(o.name)._HEADER();
                    h.UL_("uk-card-body uk-list uk-list-divider");
                    for (int i = 0; i < o?.lines.Length; i++)
                    {
                        var ln = o.lines[i];
                        h.LI_();
                        if (ln.itemid > 0)
                        {
                            h.PIC("/item/", ln.itemid, "/icon", css: "uk-width-1-6");
                        }
                        else
                        {
                            h.PIC("/ware/", ln.itemid, "/icon", css: "uk-width-1-6");
                        }
                        h.SPAN(ln.name, "uk-width-1-3");
                        h.SPAN(ln.qty, "uk-width-1-6");
                        h._LI();
                    }
                    h._UL();
                    h.NAV_("uk-card-footer")._NAV();
                });

                h.PAGINATION(arr?.Length > 10);
            });
        }
    }


    [UserAuthorize(orgly: ORGLY_OPN)]
    [Ui("零售外卖", "商户")]
    public class ShplyBuyWork : BuyWork<ShplyBuyVarWork>
    {
        [Ui("零售外卖", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var shp = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE shpid = @1 AND status > 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(shp.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, css: "uk-card-body uk-flex");
                    h.PIC("/void.webp", css: "uk-width-1-5");
                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name);
                    h.P(o.tip);
                    h._DIV();
                    h._A();
                });
            });
        }

        [Ui(tip: "历史外卖订单", icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var shp = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE shpid = @1 AND status > 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(shp.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    // h.TD(o.mrtname, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }
    }

    [UserAuthorize(Org.TYP_MKT, 1)]
#if ZHNT
    [Ui("零售外卖统一送货", "市场")]
#else
    [Ui("零售外卖统一送货", "驿站")]
#endif
    public class MktlyBuyWork : BuyWork<MktlyBuyVarWork>
    {
        [Ui("外卖订单", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var mkt = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE mktid = @1 AND status > 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(mkt.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, css: "uk-card-body uk-flex");
                    h.PIC("/void.webp", css: "uk-width-1-5");
                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name);
                    h.P(o.tip);
                    h._DIV();
                    h._A();
                });
            });
        }

        [Ui(tip: "历史订单", icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }
}