using System.Threading.Tasks;
using Chainly.Web;
using static Chainly.Web.Modal;
using static Chainly.Nodal.Store;

namespace Revital
{
    public class SupplyWork : WebWork
    {
    }

    [Ui("平台供应链交易报告", "平台", "table")]
    public class AdmlySupplyWork : SupplyWork
    {
        public async Task @default(WebContext wc)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [Ui("消费者溯源")]
    public class MySupplyWork : SupplyWork
    {
        public async Task @default(WebContext wc, int code)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Supply.Empty).T(" FROM purchs WHERE id = @1 LIMIT 1");
            var o = await dc.QueryTopAsync<Supply>(p => p.Set(code));
            wc.GivePage(200, h =>
            {
                if (o == null || o.srcid - o.srcid >= code)
                {
                    h.ALERT("编码没有找到");
                }
                else
                {
                    var plan = GrabObject<short, Ware>(o.itemid);
                    var frm = GrabObject<int, Org>(o.wareid);
                    var ctr = GrabObject<int, Org>(o.wareid);

                    h.FORM_();
                    h.FIELDSUL_("溯源信息");
                    h.LI_().FIELD("生产户", frm.name);
                    h.LI_().FIELD("分拣中心", ctr.name);
                    h._FIELDSUL();
                    h._FORM();
                }
            }, title: "中惠农通溯源系统");
        }
    }

    [UserAuthorize(Org.TYP_MRT, 1)]
    [Ui("市场供应链交易管理", "市场", "sign-in")]
    public class MrtlySupplyWork : SupplyWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
        }
    }

    [UserAuthorize(Org.TYP_BIZ, 1)]
    [Ui("商户供应链采购", "商户在线上直接向产源下单订货，货到后安排上架销售", "file-text")]
    public class BizlySupplyWork : SupplyWork
    {
        [Ui("采购记录", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Supply.Empty).T(" FROM purchs WHERE bizid = @1 AND status = 0 ORDER BY id");
            var arr = await dc.QueryAsync<Supply>(p => p.Set(org.id));

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

        [Ui("上架销售", group: 2), Tool(Anchor)]
        public async Task onsale(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Supply.Empty).T(" FROM purchs WHERE bizid = @1 AND status >= 1 ORDER BY id");
            var arr = await dc.QueryAsync<Supply>(p => p.Set(org.id));

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

        [Ui("✚", "新增采购", group: 1), Tool(ButtonOpen)]
        public void @new(WebContext wc)
        {
            var mrt = wc[-1].As<Org>();
            wc.GiveRedirect("/" + mrt.ToCtrId + "/");
        }
    }

    [UserAuthorize(Org.TYP_DST, User.ORGLY_)]
    [Ui("中枢品控配运管理", "中枢")]
    public class CtrlySupplyWork : SupplyWork
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

        [Ui("以往", group: 2), Tool(Anchor)]
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
    [Ui("版块销售管理", "版块")]
    public abstract class PrvlySupplyWork : SupplyWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PrvlySupplyVarWork>();
        }

        [Ui("当前销售"), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Supply.Empty).T(" FROM purchs WHERE prvid = @1 AND status > 0 ORDER BY id");
            var arr = await dc.QueryAsync<Supply>(p => p.Set(org.id));
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

        [Ui("以往销售"), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Supply.Empty).T(" FROM purchs WHERE prvid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Supply>(p => p.Set(org.id));

            wc.GivePage(200, h => { h.TOOLBAR(tip: "来自平台的订单"); });
        }
    }

    [Ui("版块销售管理", "版块", "sign-out", fork: Org.FRK_BY_CTR)]
    public class PrvlyStandardSupplyWork : PrvlySupplyWork
    {
    }

    [Ui("版块销售管理", "版块", "sign-out", fork: Org.FRK_ON_OWN)]
    public class PrvlyCustomSupplyWork : PrvlySupplyWork
    {
    }

    [UserAuthorize(Org.TYP_SRC, User.ORGLY_SAL)]
    [Ui("产源线上销售管理", "产源")]
    public abstract class FrmlySupplyWork : SupplyWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<MrtlySupplyVarWork>();
        }

        [Ui("当前"), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Supply.Empty).T(" FROM purchs WHERE srcid = @1 AND status > 0 ORDER BY id");
            var arr = await dc.QueryAsync<Supply>(p => p.Set(org.id));
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

        [Ui("历史"), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Supply.Empty).T(" FROM purchs WHERE srcid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Supply>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: "历史销售订货");
                h.TABLE_();
                h._TABLE();
            });
        }
    }

    [Ui("产源线上销售管理", "cloud-upload", fork: Org.FRK_BY_CTR)]
    public class SrclyCtrSupplyWork : FrmlySupplyWork
    {
    }

    [Ui("产源线上销售管理", "cloud-upload", fork: Org.FRK_ON_OWN)]
    public class SrclyOwnSupplyWork : FrmlySupplyWork
    {
    }
}