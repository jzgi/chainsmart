using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public abstract class ApWork<V> : WebWork where V : ApVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>(state: State);
    }

    protected static void MainTable(HtmlBuilder h, IList<Ap> lst, short level = 1)
    {
        h.TABLE_();

        h.THEAD_();
        h.TH("机构");
        h.TH("日期", css: "uk-text-right");
        h.TH("总额", css: "uk-text-right");
        h.TH("应收", css: "uk-text-right");
        h._THEAD();

        foreach (var o in lst)
        {
            var org = GrabTwin<int, Org>(o.orgid);

            h.TR_();
            h.TD(level == 2 ? org.cover : org.name);
            h.TD(o.dt, date: 2, time: 0);
            h.TD(o.amt, true, true);
            h.TD_().ADIALOG_(o.orgid, "/", mode: 32, pick: false).T(o.topay)._A()._TD();
            h._TR();
        }
        h._TABLE();
    }
}

[Ui("市场端应付")]
public class AdmlyBuyApWork : ApWork<AdmlyBuyApVarWork>
{
    [Ui("市场端应付", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        using var dc = NewDbContext();

        var now = DateTime.Now;
        Ap[] arr = null;
        if (await dc.QueryTopAsync("SELECT till, last FROM buygens WHERE ended < @1 AND ended >= @2 LIMIT 1", p => p.Set(now).Set(now.Date)))
        {
            dc.Let(out DateTime till);
            dc.Let(out DateTime last);

            dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM buyaps WHERE level = 2 AND dt BETWEEN @1 AND @2");
            arr = await dc.QueryAsync<Ap>(p => p.Set(last).Set(till));
        }

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("今日尚无结算记录");
                return;
            }

            MainTable(h, arr, 2);

            h.PAGINATION(arr?.Length == 30);
        }, false, 120);
    }

    [Ui(tip: "历史", icon: "history", status: 2), Tool(AnchorPrompt)]
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
            dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM puraps WHERE level = 1 ORDER BY dt LIMIT 40 OFFSET @1 * 40");
            var arr = await dc.QueryAsync<Ap>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无结算");
                    return;
                }
                MainTable(h, arr, 2);
                h.PAGINATION(arr.Length == 40);
            }, false, 12);
        }
    }

    [AdmlyAuthorize(User.ROL_FIN)]
    [Ui("结算", icon: "plus-circle", status: 1), Tool(ButtonOpen)]
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

            dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM clears WHERE status = 0 ORDER BY id ");
            var arr = await dc.QueryAsync<Ap>();

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
[Ui("供应端应付")]
public class AdmlyPurApWork : ApWork<AdmlyPurApVarWork>
{
    [Ui("供应端应付", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM puraps WHERE level = 1 ORDER BY dt LIMIT 40 OFFSET @1 * 40");
        var arr = await dc.QueryAsync<Ap>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无结算");
                return;
            }
            MainTable(h, arr);
            h.PAGINATION(arr.Length == 40);
        }, false, 12);
    }

    [Ui(tip: "历史", icon: "history", status: 2), Tool(AnchorPrompt)]
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
            dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM puraps WHERE level = 1 ORDER BY dt LIMIT 40 OFFSET @1 * 40");
            var arr = await dc.QueryAsync<Ap>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无结算");
                    return;
                }
                MainTable(h, arr, 2);
                h.PAGINATION(arr.Length == 40);
            }, false, 12);
        }
    }
}

[Ui("网售结款")]
public class RtllyBuyApWork : ApWork<OrglyApVarWork>
{
    [Ui("网售结款", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM buyaps WHERE level = 1 AND orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET @2 * 30");
        var arr = await dc.QueryAsync<Ap>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无结款");
                return;
            }

            MainTable(h, arr);

            h.PAGINATION(arr?.Length == 30);
        }, false, 120);
    }
}

[Ui("销售业务结款")]
public class SuplyPurApWork : ApWork<OrglyApVarWork>
{
    [Ui("销售结款", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM puraps WHERE level = 1 AND orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET @2 * 30");
        var arr = await dc.QueryAsync<Ap>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无结款");
                return;
            }

            MainTable(h, arr);

            h.PAGINATION(arr?.Length == 30);
        }, false, 120);
    }
}