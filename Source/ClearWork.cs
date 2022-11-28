using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class ClearWork<V> : WebWork where V : ClearVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    [UserAuthorize(admly: User.ROLE_)]
    [Ui("消费结款", "财务")]
    public class AdmlyBuyClearWork : ClearWork<AdmlyBuyClearVarWork>
    {
        [Ui("当前结算", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE typ = ").T(Clear.TYP_BUY).T(" ORDER BY dt DESC");
            var arr = await dc.QueryAsync<Clear>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    return;
                }
                h.TABLE_();
                var last = 0;
                foreach (var o in arr)
                {
                    if (o.sprid != last)
                    {
                        var spr = GrabObject<int, Org>(o.sprid);
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    h.TD(o.till);
                    h.TD(o.name);
                    h._TR();

                    last = o.sprid;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            }, false, 3);
        }

        [Ui(tip: "历史", icon: "history", group: 2), Tool(AnchorPrompt)]
        public async Task past(WebContext wc, int page)
        {
            var topOrgs = Grab<int, Org>();
            int prvid = 0;
            bool inner = wc.Query[nameof(inner)];
            if (inner)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("按市场");
                    h.LI_().SELECT("市场", nameof(prvid), prvid, topOrgs, filter: (k, v) => v.IsMarket, required: true);
                    h._FIELDSUL()._FORM();
                });
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE typ = ").T(Clear.TYP_BUY).T(" AND sprid = @1 AND status > 0 ORDER BY dt DESC LIMIT 40 OFFSET 40 * @2");
                var arr = await dc.QueryAsync<Clear>();
                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    if (arr == null)
                    {
                        return;
                    }
                    h.TABLE_();
                    var last = 0;
                    foreach (var o in arr)
                    {
                        if (o.sprid != last)
                        {
                            var spr = GrabObject<int, Org>(o.sprid);
                            h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
                        }
                        h.TR_();
                        h.TD(o.till);
                        h.TD(o.name);
                        h._TR();

                        last = o.sprid;
                    }
                    h._TABLE();
                    h.PAGINATION(arr.Length == 40);
                }, false, 3);
            }
        }

        [UserAuthorize(admly: 1)]
        [Ui("结算", "结算代收款项", icon: "plus-circle", group: 1), Tool(ButtonOpen)]
        public async Task calc(WebContext wc)
        {
            if (wc.IsGet)
            {
                var till = DateTime.Today.AddDays(-1);
                wc.GivePane(200, h =>
                {
                    h.FORM_(post: false).FIELDSUL_("选择截止（包含）日期");
                    h.LI_().DATE("截止日期", nameof(till), till, max: till)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // OUTER
            {
                DateTime till = wc.Query[nameof(till)];
                using var dc = NewDbContext(IsolationLevel.RepeatableRead);

                await dc.ExecuteAsync("SELECT recalc(@1)", p => p.Set(till));

                dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE status = 0 ORDER BY id ");
                var arr = await dc.QueryAsync<Clear>();

                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    var orgs = Grab<short, Org>();
                    h.TABLE(arr, o =>
                    {
                        // h.TD(Clear.Typs[o.typ]);
                        // h.TD(orgs[o.orgid]?.name);
                        // h.TD_().T(o.till, 3, 0)._TD();
                        // h.TD(o.amt, currency: true);
                    });
                }, false, 3);
            }
        }
    }

    [UserAuthorize(admly: User.ROLE_)]
    [Ui("供应链结款", "财务")]
    public class AdmlyBookClearWork : ClearWork<AdmlySupplyClearVarWork>
    {
        [Ui("当前结算", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE typ = ").T(Clear.TYP_SUPPLY).T(" ORDER BY dt DESC");
            var arr = await dc.QueryAsync<Clear>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    return;
                }
                h.TABLE_();
                var last = 0;
                foreach (var o in arr)
                {
                    if (o.sprid != last)
                    {
                        var spr = GrabObject<int, Org>(o.sprid);
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    h.TD(o.till);
                    h.TD(o.name);
                    h._TR();

                    last = o.sprid;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            }, false, 3);
        }

        [Ui(tip: "历史", icon: "history", group: 2), Tool(AnchorPrompt)]
        public async Task past(WebContext wc, int page)
        {
            var topOrgs = Grab<int, Org>();
            bool inner = wc.Query[nameof(inner)];
            int prv = 0;
            if (inner)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("按供应版块");
                    h.LI_().SELECT("版块", nameof(prv), prv, topOrgs, filter: (k, v) => v.IsZone, required: true);
                    h._FIELDSUL()._FORM();
                });
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE typ = ").T(Clear.TYP_SUPPLY).T(" AND sprid = @1 AND status > 0 ORDER BY dt DESC LIMIT 40 OFFSET 40 * @2");
                var arr = await dc.QueryAsync<Clear>(p => p.Set(prv).Set(page));
                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    if (arr == null)
                    {
                        return;
                    }
                    h.TABLE_();
                    var last = 0;
                    foreach (var o in arr)
                    {
                        if (o.sprid != last)
                        {
                            var spr = GrabObject<int, Org>(o.sprid);
                            h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
                        }
                        h.TR_();
                        h.TD(o.till);
                        h.TD(o.name);
                        h._TR();

                        last = o.sprid;
                    }
                    h._TABLE();
                    h.PAGINATION(arr.Length == 40);
                }, false, 3);
            }
        }

        [UserAuthorize(admly: 1)]
        [Ui("结算", "结算代收款项", icon: "plus-circle", group: 1), Tool(ButtonOpen)]
        public async Task calc(WebContext wc)
        {
            if (wc.IsGet)
            {
                var till = DateTime.Today.AddDays(-1);
                wc.GivePane(200, h =>
                {
                    h.FORM_(post: false).FIELDSUL_("选择截止（包含）日期");
                    h.LI_().DATE("截止日期", nameof(till), till, max: till)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // OUTER
            {
                DateTime till = wc.Query[nameof(till)];
                using var dc = NewDbContext(IsolationLevel.RepeatableRead);

                await dc.ExecuteAsync("SELECT recalc(@1)", p => p.Set(till));

                dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE status = 0 ORDER BY id ");
                var arr = await dc.QueryAsync<Clear>();

                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    var orgs = Grab<short, Org>();
                    h.TABLE(arr, o =>
                    {
                        // h.TD(Clear.Typs[o.typ]);
                        // h.TD(orgs[o.orgid]?.name);
                        // h.TD_().T(o.till, 3, 0)._TD();
                        // h.TD(o.amt, currency: true);
                    });
                }, false, 3);
            }
        }
    }

    [Ui("账户款项结算", "基础")]
    public class OrglyClearWork : ClearWork<OrglyClearVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM clears WHERE orgid = @1 ORDER BY dt DESC LIMIT 20 OFFSET 20 * @2");
            var arr = dc.Query<Clear>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.Key);
                    h.TD_().T(o.till, 3, 0)._TD();
                    h.TD(Clear.Typs[o.typ]);
                    h.TD(o.amt, currency: true);
                    h.TD(Clear.Statuses[((Entity) o).state]);
                });
                h.PAGINATION(arr?.Length == 20);
            }, false, 3);
        }

        [UserAuthorize(0, User.ROLE_MGT)]
        [Ui("统计", "时段统计"), Tool(ButtonOpen)]
        public async Task sum(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-1];
            DateTime date;
            short typ = 0;
            decimal amt = 0;
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("指定统计区间");
                    h.LI_().DATE("从日期", nameof(date), DateTime.Today, required: true)._LI();
                    h.LI_().DATE("到日期", nameof(date), DateTime.Today, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                date = f[nameof(date)];
                date = f[nameof(date)];
                wc.GivePane(200); // close dialog
            }
        }
    }
}