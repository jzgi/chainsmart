using System;
using System.Data;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("平台｜代收款项结算", "table")]
    public class AdmlyClearWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyClearVarWork>();
        }

        [Ui("零售商户", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE typ = ").T(Clear.TYP_RETAIL).T(" ORDER BY dt DESC");
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
                    h.TD(o.dt);
                    h.TD(o.name);
                    h._TR();

                    last = o.sprid;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            }, false, 3);
        }

        [Ui("历史", group: 2), Tool(AnchorPrompt, Appear.Small)]
        public async Task past(WebContext wc, int page)
        {
            bool inner = wc.Query[nameof(inner)];
            string tel = null;
            if (inner)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("按市场");
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h._FIELDSUL()._FORM();
                    h.FORM_().FIELDSUL_("按日期");
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h._FIELDSUL()._FORM();
                });
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Clear.Empty).T(" FROM clears WHERE typ = ").T(Clear.TYP_RETAIL).T(" ORDER BY dt DESC");
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
                        h.TD(o.dt);
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
        [Ui("∑", "结算零售代收款项", group: 1), Tool(ButtonOpen, Appear.Small)]
        public async Task calcrtl(WebContext wc)
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

    [Ui("账户｜平台代收款项", "credit-card")]
    public class OrglyClearWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<OrglyClearVarWork>();
        }

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
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD(Clear.Typs[o.typ]);
                    h.TD(o.amt, currency: true);
                    h.TD(Clear.Statuses[o.status]);
                });
                h.PAGINATION(arr?.Length == 20);
            }, false, 3);
        }

        [UserAuthorize(orgly: 1)]
        [Ui("统计", "时段统计"), Tool(ButtonShow)]
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