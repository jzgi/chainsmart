using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart
{
    public abstract class ClearWork<V> : WebWork where V : ClearVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>(state: State);
        }

        protected static void MainTable(HtmlBuilder h, Clear[] arr, bool orgname)
        {
            h.TABLE_();
            DateTime last = default;
            foreach (var o in arr)
            {
                if (o.till != last)
                {
                    h.TR_().TD_("uk-padding-tiny-left", colspan: 4).T(o.till, time: 0)._TD()._TR();
                }

                h.TR_();
                if (orgname)
                {
                    var org = GrabObject<int, Org>(o.orgid);
                    h.TD(org.name);
                }

                h.TD(Clear.Typs[o.typ]);
                h.TD_("uk-text-right").T(o.trans)._TD();
                if (!orgname)
                {
                    h.TD_("uk-text-right").CNY(o.amt)._TD();
                }

                h.TD_("uk-text-right").CNY(o.topay)._TD();
                h._TR();

                last = o.till;
            }

            h._TABLE();
        }
    }


    [AdmlyAuthorize(User.ROL_FIN)]
    [Ui("消费业务结算管理", "财务")]
    public class AdmlyBuyClearWork : ClearWork<AdmlyBuyClearVarWork>
    {
        [Ui("消费业务结算", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM buyclrs WHERE status BETWEEN 1 AND 2 ORDER BY id LIMIT 40 OFFSET @1 * 40");
            var arr = await dc.QueryAsync<Clear>(p => p.Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无结款");
                    return;
                }

                MainTable(h, arr, true);

                h.PAGINATION(arr.Length == 40);
            }, false, 60);
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

                MainTable(h, arr, true);

                h.PAGINATION(arr.Length == 40);
            }, false, 3);
        }

        [OrglyAuthorize(0, User.ROL_FIN)]
        [Ui("结算", "结算代收款项", icon: "plus-circle", group: 1), Tool(ButtonOpen)]
        public async Task gen(WebContext wc)
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
    [Ui("供应业务结算管理", "财务")]
    public class AdmlyBookClearWork : ClearWork<AdmlyBookClearVarWork>
    {
        [Ui("供应业务结算", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM bookclrs WHERE typ BETWEEN 1 AND 3 AND status = 1 ORDER BY id LIMIT 40 OFFSET @1 * 40");
            var arr = await dc.QueryAsync<Clear>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无结算");
                    return;
                }

                MainTable(h, arr, true);

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
                    // h.LI_().SELECT("版块", nameof(prv), prv, topOrgs, filter: (k, v) => v.EqZone, required: true);
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

                    h.PAGINATION(arr.Length == 40);
                }, false, 3);
            }
        }

        [OrglyAuthorize(0, User.ROL_FIN)]
        [Ui("结算", "结算代收款项", icon: "plus-circle", group: 1), Tool(ButtonOpen)]
        public async Task gen(WebContext wc)
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

    [Ui("消费业务收入")]
    public class PtylyBuyClearWork : ClearWork<PtylyClearVarWork>
    {
        [Ui("业务收入", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            var isOrg = (bool)State;

            var org = isOrg ? wc[-1].As<Org>() : null;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM buyclrs WHERE orgid = @1 AND status BETWEEN 1 AND 2 ORDER BY id DESC LIMIT 40 OFFSET @2 * 40");
            var arr = dc.Query<Clear>(p =>
            {
                if (org == null)
                {
                    p.SetNull();
                }
                else
                {
                    p.Set(org.id);
                }

                p.Set(page);
            });

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无结款");
                    return;
                }

                MainTable(h, arr, false);

                h.PAGINATION(arr?.Length == 20);
            }, false, 3);
        }
    }

    [Ui("供应业务结款")]
    public class PtylyBookClearWork : ClearWork<PtylyClearVarWork>
    {
        [Ui("业务收入", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            var isOrg = (bool)State;

            var org = isOrg ? wc[-1].As<Org>() : null;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM bookclrs WHERE orgid = @1 AND status BETWEEN 1 AND 2 ORDER BY id DESC LIMIT 40 OFFSET @2 * 40");
            var arr = dc.Query<Clear>(p =>
            {
                if (org == null)
                {
                    p.SetNull();
                }
                else
                {
                    p.Set(org.id);
                }

                p.Set(page);
            });

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无结款");
                    return;
                }

                MainTable(h, arr, false);

                h.PAGINATION(arr?.Length == 20);
            }, false, 3);
        }
    }
}