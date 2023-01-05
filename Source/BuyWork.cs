using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
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

    [Ui("我的消费", "账号")]
    public class MyBuyWork : BuyWork<MyBuyVarWork>
    {
        public async Task @default(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE uid = @1 AND status > 0 ORDER BY id DESC LIMIT 10 OFFSET 10 * @2");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(prin.id).Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: prin.name);

                if (arr == null)
                {
                    h.ALERT("尚无消费订单");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.HEADER_("uk-card-header").H4(o.name)._HEADER();
                    h.UL_("uk-card-body uk-list uk-list-divider");
                    h.LI_().T(o.created)._LI();
                    for (int i = 0; i < o?.details.Length; i++)
                    {
                        var ln = o.details[i];
                        h.LI_();
                        if (ln.itemid > 0)
                        {
                            h.PIC("/item/", ln.itemid, "/icon", css: "uk-width-micro");
                        }
                        else
                        {
                            h.PIC("/ware/", ln.itemid, "/icon", css: "uk-width-micro");
                        }
                        h.SPAN(ln.name, "uk-width-expand");
                        h.SPAN(ln.qty, "uk-width-1-6");
                        h._LI();
                    }
                    h._UL();
                    h.FOOTER_("uk-card-footer").T("合计：").T(o.topay)._FOOTER();
                });

                h.PAGINATION(arr?.Length > 10);
            });
        }
    }


    [OrglyAuthorize(Org.TYP_SHP, 1)]
    [Ui("零售外卖", "商户")]
    public class ShplyBuyWork : BuyWork<ShplyBuyVarWork>
    {
        [Ui("零售外卖", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var shp = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE shpid = @1 AND status BETWEEN 1 AND 2 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(shp.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, css: "uk-card-body uk-flex");

                    h.PIC("/void.webp", css: "uk-width-1-5");

                    h.ASIDE_();
                    h.HEADER_().H4(o.uname).SPAN(Buy.Statuses[o.status], "uk-badge")._HEADER();
                    h.Q(o.uaddr, "uk-width-expand");
                    h.FOOTER_().CNY(o.pay, true).SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                    h._ASIDE();

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

    [OrglyAuthorize(Org.TYP_MKT, 1)]
#if ZHNT
    [Ui("零售外卖送货", "市场")]
#else
    [Ui("零售外卖送货", "驿站")]
#endif
    public class MktlyBuyWork : BuyWork<MktlyBuyVarWork>
    {
        [Ui("零售外卖", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var mkt = wc[-1].As<Org>();

            using var dc = NewDbContext();
            const short msk = Entity.MSK_EXTRA;
            dc.Sql("SELECT ").collst(Buy.Empty, msk).T(" FROM buys WHERE mktid = @1 AND state >= 0 ORDER BY uid DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(mkt.id), msk);

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAIN_(grid: true);

                int last = 0; // uid

                foreach (var o in arr)
                {
                    if (o.uid != last)
                    {
                        h.FORM_("uk-card uk-card-default");
                        h.HEADER_("uk-card-header").T(o.uname).SP().T(o.utel).SP().T(o.uaddr)._HEADER();
                        h.UL_("uk-card-body");
                        // h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                    }
                    h.LI_().T(o.name)._LI();

                    last = o.uid;
                }
                h._UL();
                h._FORM();

                h._MAIN();
            });
        }

        [Ui(tip: "历史", icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var mkt = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE mktid = @1 AND state >= 0 ORDER BY uid DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(mkt.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAIN_(grid: true);

                int last = 0; // uid

                foreach (var o in arr)
                {
                    if (o.uid != last)
                    {
                        h.FORM_("uk-card uk-card-default");
                        h.HEADER_("uk-card-header").T(o.uname).SP().T(o.utel).SP().T(o.uaddr)._HEADER();
                        h.UL_("uk-card-body");
                        // h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                    }
                    h.LI_().T(o.name)._LI();

                    last = o.uid;
                }
                h._UL();
                h._FORM();

                h._MAIN();
            });
        }
    }
}