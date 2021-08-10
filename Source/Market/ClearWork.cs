using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using Zhnt;
using static System.Data.IsolationLevel;
using static SkyChain.Web.Modal;
using static Zhnt.Clear;
using static Zhnt.User;

namespace Zhnt.Market
{
    [UserAuthorize(admly: ADMLY_OP)]
    [Ui("结算")]
    public class AdmlyClearWork : WebWork
    {
        [Ui("已结", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var orgs = Fetch<Map<short, Org>>();
            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM clears WHERE status >= ").T(STATUS_RECKONED).T(" ORDER BY id DESC LIMIT 30 OFFSET @1 * 30");
            var arr = await dc.QueryAsync<Clear>(p => p.Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null) return;
                h.TABLE_();
                DateTime last = default;
                foreach (var o in arr)
                {
                    if (o.till != last)
                    {
                        h.TR_().TD_("uk-label", colspan: 6).T(o.till, 3, 0)._TD()._TR();
                    }
                    h.TR_();
                    h.TDCHECK(o.id, disabled: o.IsSettled);
                    h.TD_().T(Clear.Typs[o.typ]);
                    if (o.orgid > 0)
                    {
                        h.SP().T(orgs[o.orgid]?.name);
                    }
                    h._TD();
                    h.TD(o.amt, currency: true);
                    h.TD(Statuses[o.status]);

                    h._TR();
                    last = o.till;
                }
                h._TABLE();
                h.PAGINATION(arr?.Length == 20);
            }, false, 3);
        }

        [UserAuthorize(admly: 1)]
        [Ui("初算", group: 2), Tool(AnchorPrompt)]
        public async Task recalc(WebContext wc)
        {
            bool inner = wc.Query[nameof(inner)];
            if (inner)
            {
                var till = DateTime.Today;
                wc.GivePane(200, h =>
                {
                    h.FORM_(post: false).FIELDSUL_("选择截至日期（不包含）");
                    h.LI_().DATE("截至日期", nameof(till), till, max: till)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // OUTER
            {
                DateTime till = wc.Query[nameof(till)];
                using var dc = NewDbContext(RepeatableRead);

                await dc.ExecuteAsync("SELECT recalc(@1)", p => p.Set(till));

                dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE status = 0 ORDER BY id ");
                var arr = await dc.QueryAsync<Clear>();

                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    var orgs = Fetch<Map<short, Org>>();
                    h.TABLE(arr, o =>
                    {
                        h.TD(Clear.Typs[o.typ]);
                        h.TD(orgs[o.orgid]?.name);
                        h.TD_().T(o.till, 3, 0)._TD();
                        h.TD(o.amt, currency: true);
                    });
                }, false, 3);
            }
        }

        [Ui("结账", group: 2), Tool(ButtonOpen)]
        public async Task reckon(WebContext wc)
        {
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                await dc.QueryTopAsync("SELECT count(*), sum(amt) AS total FROM clears WHERE status = 0");
                dc.Let(out short count);
                dc.Let(out decimal total);
                wc.GivePane(200, h =>
                {
                    if (count == 0)
                    {
                        h.ALERT("没有待结账的记录");
                        return;
                    }
                    h.FORM_().FIELDSUL_("结账");
                    h.LI_().FIELD("总金额", total)._LI();
                    h._FIELDSUL();
                    h.BOTTOMBAR_().BUTTON("确认", nameof(reckon))._BOTTOMBAR();
                    h._FORM();
                });
            }
            else
            {
                using var dc = NewDbContext(ReadCommitted);
                await dc.QueryTopAsync("SELECT min(till), max(till) FROM clears WHERE status = 0");
                dc.Let(out DateTime min);
                dc.Let(out DateTime max);
                if (min == max)
                {
                    await dc.ExecuteAsync("SELECT reckon(@1)", p => p.Set(min));
                }
                wc.GivePane(200);
            }
        }

        [Ui("付款", group: 1), Tool(ButtonPickPrompt)]
        public async Task pay(WebContext wc)
        {
            var prin = (User) wc.Principal;
            short mode = 0;
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.ALERT("转款过程会将相关订单置为关闭状态");
                    h.FORM_().FIELDSUL_("转款方式");
                    h.LI_().RADIO(nameof(mode), 0, "手动转款（仅设置状态）")._LI();
                    h.LI_().RADIO(nameof(mode), 1, "即时微信转款")._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                mode = f[nameof(mode)];
                int[] key = f[nameof(key)];
                Clear[] arr;
                using (var dc = NewDbContext())
                {
                    dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE status = 1 AND id")._IN_(key);
                    arr = await dc.QueryAsync<Clear>(p => p.SetForIn(key));
                }
                if (arr != null)
                {
                    var now = DateTime.Now;
                    var orgs = Fetch<Map<short, Org>>();
                    foreach (var o in arr)
                    {
                        string fail = null;
                        if (mode == 1) // call payment gateway
                        {
                            var org = orgs[o.orgid];
                            fail = await WeChatUtility.PostTransferAsync(o.id, org.mgrim, org.mgrname, o.pay, Clear.Typs[o.typ] + "（截至" + o.till + "）");
                        }
                        if (fail == null) // update status
                        {
                            using var dc = NewDbContext(level: ReadCommitted);
                            dc.Sql("UPDATE clears SET status = 2, opred = @1, oprid = @2, WHERE id = @2 AND status = 1");
                            await dc.ExecuteAsync(p => p.Set(now).Set(prin.id).Set(o.id));
                        }
                    }
                }
                wc.GiveRedirect();
            }
        }
    }

    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("结算")]
    public class OrglyClearWork : WebWork
    {
        public void @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM clears WHERE orgid = @1 ORDER BY id DESC LIMIT 20 OFFSET 20 * @2");
            var arr = dc.Query<Clear>(p => p.Set(orgid).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: "应收款项和数字资产");
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD(Clear.Typs[o.typ]);
                    h.TD_().T(o.till, 3, 0)._TD();
                    h.TD(o.amt, currency: true);
                    h.TD(Statuses[o.status]);
                });
                h.PAGINATION(arr?.Length == 20);
            }, false, 3);
        }

        [UserAuthorize(orgly: ORGLY_MGR)]
        [Ui("统计"), Tool(ButtonShow)]
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