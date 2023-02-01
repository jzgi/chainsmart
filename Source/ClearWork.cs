using System;
using System.Collections.Generic;
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

        protected static void ClearTable(HtmlBuilder h, IEnumerable<Clear> arr)
        {
            h.TABLE_();
            var last = 0;
            foreach (var o in arr)
            {
                if (o.prtid != last)
                {
                    var spr = GrabObject<int, Org>(o.prtid);
                    h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
                }
                h.TR_();
                h.TD(o.till);
                h.TD(o.name);
                h._TR();

                last = o.prtid;
            }
            h._TABLE();
        }
    }


    [AdmlyAuthorize(User.ROL_FIN)]
    [Ui("消费订单结款", "财务")]
    public class AdmlyBuyClearWork : ClearWork<AdmlyBuyClearVarWork>
    {
        [Ui("当前结款", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE typ BETWEEN ").T(Clear.TYP_SHP).T(" AND ").T(Clear.TYP_MKT).T(" ANd status = 1 ORDER BY id LIMIT 40 OFFSET @1 * 40");
            var arr = await dc.QueryAsync<Clear>(p => p.Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无结款");
                    return;
                }

                ClearTable(h, arr);

                h.PAGINATION(arr.Length == 40);
            }, false, 3);
        }

        [Ui(tip: "已付款", icon: "credit-card", group: 2), Tool(Anchor)]
        public async Task oked(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE typ BETWEEN ").T(Clear.TYP_SHP).T(" AND ").T(Clear.TYP_MKT).T(" AND status = 4 ORDER BY id LIMIT 40 OFFSET @1 * 40");
            var arr = await dc.QueryAsync<Clear>(p => p.Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无结款");
                    return;
                }

                ClearTable(h, arr);

                h.PAGINATION(arr.Length == 40);
            }, false, 3);
        }

        [AdmlyAuthorize(1)]
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

    [AdmlyAuthorize(User.ROL_FIN)]
    [Ui("供应链订单结款", "财务")]
    public class AdmlyBookClearWork : ClearWork<AdmlyBookClearVarWork>
    {
        [Ui("当前结款", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE typ BETWEEN 1 AND 3 AND status = 1 ORDER BY id LIMIT 40 OFFSET @1 * 40");
            var arr = await dc.QueryAsync<Clear>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无结款");
                    return;
                }

                ClearTable(h, arr);

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
                    h.LI_().SELECT("版块", nameof(prv), prv, topOrgs, filter: (k, v) => v.EqZone, required: true);
                    h._FIELDSUL()._FORM();
                });
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE typ = ").T(Clear.TYP_SRC).T(" AND sprid = @1 AND status > 0 ORDER BY id DESC LIMIT 40 OFFSET 40 * @2");
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
                        if (o.prtid != last)
                        {
                            var spr = GrabObject<int, Org>(o.prtid);
                            h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
                        }
                        h.TR_();
                        h.TD(o.till);
                        h.TD(o.name);
                        h._TR();

                        last = o.prtid;
                    }
                    h._TABLE();
                    h.PAGINATION(arr.Length == 40);
                }, false, 3);
            }
        }

        [AdmlyAuthorize(1)]
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

    [Ui("业务结款", "常规")]
    public class OrglyClearWork : ClearWork<OrglyClearVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE orgid = @1 AND status = 1 ORDER BY id LIMIT 40 OFFSET @2 * 40");
            var arr = dc.Query<Clear>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.Key);
                    h.TD_().T(o.till, 3, 0)._TD();
                    h.TD(Clear.Typs[o.typ]);
                    h.TD(o.amt, money: true);
                    h.TD(Clear.Statuses[((Entity) o).state]);
                });
                h.PAGINATION(arr?.Length == 20);
            }, false, 3);
        }

        [OrglyAuthorize(0, User.ROL_MGT)]
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