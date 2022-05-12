using System.Threading.Tasks;
using Chainly.Web;
using static Chainly.Web.Modal;
using static Chainly.Nodal.Store;

namespace Revital
{
    public class PurchWork : WebWork
    {
    }

    [Ui("平台供应链交易报告", "平台", "table")]
    public class AdmlyPurchWork : PurchWork
    {
        public async Task @default(WebContext wc)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [Ui("消费者溯源")]
    public class MyPurchWork : PurchWork
    {
        public async Task @default(WebContext wc, int code)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE id = @1 LIMIT 1");
            var o = await dc.QueryTopAsync<Purch>(p => p.Set(code));
            wc.GivePage(200, h =>
            {
                if (o == null || o.srcid - o.srcid >= code)
                {
                    h.ALERT("编码没有找到");
                }
                else
                {
                    var plan = GrabObject<short, Prod>(o.itemid);
                    var frm = GrabObject<int, Org>(o.prodid);
                    var ctr = GrabObject<int, Org>(o.prodid);

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
    public class MrtlyPurchWork : PurchWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
        }
    }

    [UserAuthorize(Org.TYP_BIZ, 1)]
    [Ui("商户线上采购", "商户", "file-text")]
    public class BizlyPurchWork : PurchWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
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

        [Ui("历史", group: 2), Tool(Anchor)]
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


        [Ui("✚", "新增进货", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            var ctr = GrabObject<int, Org>(2);
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Prod.Empty).T(" FROM prods WHERE orgid = @1 AND status > 0 ORDER BY cat, status DESC");
                var arr = await dc.QueryAsync<Prod>(p => p.Set(2));
                var prods = Grab<int, Prod>();
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_(ctr.name);
                    short last = 0;
                    foreach (var o in arr)
                    {
                        if (o.typ != last)
                        {
                            h.LI_().SPAN_("uk-label").T(Item.Typs[o.typ])._SPAN()._LI();
                        }
                        h.LI_("uk-flex");
                        h.SPAN_("uk-width-1-4").T(o.name)._SPAN();
                        h.SPAN_("uk-visible@l").T(o.tip)._SPAN();
                        h.SPAN_().CNY(o.price, true).T("／").T(o.unit)._SPAN();
                        // h.SPAN(Item.Typs[o.authreq]);
                        h.SPAN(Info.Statuses[o.status]);
                        h.BUTTON("✕", "", 1, onclick: "this.form.targid.value = ", css: "uk-width-micro uk-button-secondary");
                        h._LI();

                        last = o.typ;
                    }
                    h._FIELDSUL();

                    h.BOTTOMBAR_();

                    h._BOTTOMBAR();
                    h._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Purch>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO buys ").colset(Purch.Empty, 0)._VALUES_(Purch.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }

    [UserAuthorize(Org.TYP_DST, User.ORGLY_)]
    [Ui("中枢品控配运管理", "中枢")]
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

        [Ui("以往销售"), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE prvid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Purch>(p => p.Set(org.id));

            wc.GivePage(200, h => { h.TOOLBAR(tip: "来自平台的订单"); });
        }
    }

    [Ui("版块销售管理", "版块", "sign-out", fork: Org.FRK_CTR)]
    public class PrvlyStandardPurchWork : PrvlyPurchWork
    {
    }

    [Ui("版块销售管理", "版块", "sign-out", fork: Org.FRK_OWN)]
    public class PrvlyCustomPurchWork : PrvlyPurchWork
    {
    }

    [UserAuthorize(Org.TYP_SRC, User.ORGLY_SAL)]
    [Ui("产源线上销售管理", "产源")]
    public abstract class FrmlyPurchWork : PurchWork
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

        [Ui("历史"), Tool(Anchor)]
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

    [Ui("产源线上销售管理", "cloud-upload", fork: Org.FRK_CTR)]
    public class SrclyCtrPurchWork : FrmlyPurchWork
    {
    }

    [Ui("产源线上销售管理", "cloud-upload", fork: Org.FRK_OWN)]
    public class SrclyOwnPurchWork : FrmlyPurchWork
    {
    }
}