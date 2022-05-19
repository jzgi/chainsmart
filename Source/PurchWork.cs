using System.Threading.Tasks;
using Chainly.Web;
using static Chainly.Web.Modal;
using static Chainly.Nodal.Store;

namespace Revital
{
    public class PurchWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_MRT, 1)]
    [Ui("市场供应链业务", icon: "sign-in")]
    public class MrtlyPurchWork : PurchWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
        }
    }

    [UserAuthorize(Org.TYP_BIZ, 1)]
#if ZHNT
    [Ui("商户采购订货", icon: "chevron-up")]
#else
    [Ui("驿站采购订货", icon: "chevron-up")]
#endif
    public class BizlyPurchWork : PurchWork
    {
        [Ui("当前订货", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE bizid = @1 AND status = 0 ORDER BY id");
            var arr = await dc.QueryAsync<Purch>(p => p.Set(org.id));

            var items = Grab<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: "当前订货");
                h.TABLE(arr, o =>
                {
                    // h.TD(items[o.itemid].name);
                    h.TD(o.ctrid);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("以往", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE bizid = @1 AND status >= 1 ORDER BY id");
            var arr = await dc.QueryAsync<Purch>(p => p.Set(org.id));

            var items = Grab<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: "历史订货");
                h.TABLE(arr, o =>
                {
                    // h.TD(items[o.itemid].name);
                    h.TD(o.ctrid);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("✚", "新增订货", group: 1), Tool(ButtonOpen)]
        public void @new(WebContext wc)
        {
            var mrt = wc[-1].As<Org>();
            wc.GiveRedirect("/" + mrt.ToCtrId + "/");
        }
    }

    [UserAuthorize(Org.TYP_DST, User.ORGLY_)]
    [Ui("中枢配运管理")]
    public class CtrlyPurchWork : PurchWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT sprid, fromid, sum(pay) FROM purchs WHERE ctrid = @1 AND status = 1 GROUP BY sprid, fromid ORDER BY sprid, fromid");
            await dc.QueryAsync(p => p.Set(org.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAIN_();
                int last = 0;
                while (dc.Next())
                {
                    dc.Let(out int sprid);
                    dc.Let(out int fromid);
                    dc.Let(out decimal sum);

                    if (sprid != last)
                    {
                        var spr = GrabObject<int, Org>(sprid);
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    var from = GrabObject<int, Org>(fromid);
                    h.TD(from.name);
                    h.TD_("uk-visible@l").T(sum)._TD();
                    h._TR();

                    last = sprid;
                }
                h._MAIN();
            });
        }

        [Ui("⌹", "以往", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT sprid, fromid, sum(pay) FROM purchs WHERE toid = @1 AND status = 1 GROUP BY sprid, fromid ORDER BY sprid, fromid");
            await dc.QueryAsync(p => p.Set(org.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAIN_();
                int last = 0;
                while (dc.Next())
                {
                    dc.Let(out int sprid);
                    dc.Let(out int fromid);
                    dc.Let(out decimal sum);

                    if (sprid != last)
                    {
                        var spr = GrabObject<int, Org>(sprid);
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    var from = GrabObject<int, Org>(fromid);
                    h.TD(from.name);
                    h.TD_("uk-visible@l").T(sum)._TD();
                    h._TR();

                    last = sprid;
                }
                h._MAIN();
            });
        }
    }


    [UserAuthorize(Org.TYP_PRV, User.ORGLY_SAL)]
    public abstract class PrvlyPurchWork : PurchWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PrvlyPurchVarWork>();
        }

        [Ui("当前销售"), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE prvid = @1 AND status > 0 ORDER BY id");
            var arr = await dc.QueryAsync<Purch>(p => p.Set(org.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.Key);
                    // h.TD(o.qty);
                });
            });
        }

        [Ui("历史", "以往销售"), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE prvid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Purch>(p => p.Set(org.id));

            wc.GivePage(200, h => { h.TOOLBAR(tip: "来自平台的订单"); });
        }
    }

    [Ui("版块标准供应管理", icon: "sign-out", fork: Org.FRK_STD)]
    public class PrvlyStdPurchWork : PrvlyPurchWork
    {
    }

    [Ui("版块自由供应管理", icon: "sign-out", fork: Org.FRK_FREE)]
    public class PrvlyFreePurchWork : PrvlyPurchWork
    {
    }

    [UserAuthorize(Org.TYP_SRC, User.ORGLY_SAL)]
    public abstract class SrclyPurchWork : PurchWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<MrtlyPurchVarWork>();
        }

        [Ui("当前"), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE srcid = @1 AND status > 0 ORDER BY id");
            var arr = await dc.QueryAsync<Purch>(p => p.Set(org.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: "当前销售订货");

                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.Key);
                    // h.TD(o.qty);
                });
            });
        }

        [Ui("⌹", "历史"), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE srcid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Purch>(p => p.Set(org.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: "历史销售订货");
                h.TABLE_();
                h._TABLE();
            });
        }
    }

    [Ui("产源标准供应业务", icon: "cloud-upload", fork: Org.FRK_STD)]
    public class SrclyCtrPurchWork : SrclyPurchWork
    {
    }

    [Ui("产源自由供应业务", icon: "cloud-upload", fork: Org.FRK_FREE)]
    public class SrclyFreePurchWork : SrclyPurchWork
    {
    }
}