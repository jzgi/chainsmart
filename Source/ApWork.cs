using System.Collections.Generic;
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

    protected static void MainTable(HtmlBuilder h, IList<Ap> lst, bool orgname)
    {
        h.TABLE_();

        h.THEAD_();
        h.TH("日期");
        h.TH("订单", css: "uk-text-right");
        h.TH("总额", css: "uk-text-right");
        h.TH("返率％", css: "uk-text-right");
        h.TH("应收", css: "uk-text-right");
        h._THEAD();

        foreach (var o in lst)
        {
            h.TR_();
            h.TD(o.dt, time: 0);
            h.TD(o.trans);
            h.TD(o.amt, true, true);
            h.TD(o.rate);
            h.TD(o.topay, true, true);
            h._TR();
        }
        h._TABLE();
    }
}

[Ui("市场端应付结款")]
public class AdmlyBuyApWork : ApWork<AdmlyBuyApVarWork>
{
    [Ui("应付结款", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM buyaps WHERE level = 1 ORDER BY dt DESC LIMIT 30 OFFSET @2 * 30");
        var arr = dc.Query<Ap>(p => p.Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无结款");
                return;
            }

            MainTable(h, arr, false);

            h.PAGINATION(arr?.Length == 30);
        }, false, 120);
    }

    [Ui(tip: "已付款", icon: "credit-card", status: 2), Tool(Anchor)]
    public async Task oked(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM buyaps WHERE level = 2 ORDER BY id LIMIT 40 OFFSET @1 * 40");
        var arr = await dc.QueryAsync<Ap>(p => p.Set(page));

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
}

[AdmlyAuthorize(User.ROL_FIN)]
[Ui("供应端应付帐款")]
public class AdmlyPurApWork : ApWork<AdmlyPurApVarWork>
{
    [Ui("应付帐款", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM puraps WHERE typ BETWEEN 1 AND 3 AND status = 1 ORDER BY id LIMIT 40 OFFSET @1 * 40");
        var arr = await dc.QueryAsync<Ap>();

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
            dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM clears WHERE level = 2 AND sprid = @1 AND status > 0 ORDER BY id DESC LIMIT 40 OFFSET 40 * @2");
            var arr = await dc.QueryAsync<Ap>(p => p.Set(prv).Set(page));
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
}

[Ui("网售结款")]
public class RtllyBuyApWork : ApWork<PtylyApVarWork>
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

            MainTable(h, arr, false);

            h.PAGINATION(arr?.Length == 30);
        }, false, 120);
    }
}

[Ui("销售结款")]
public class SuplyPurApWork : ApWork<PtylyApVarWork>
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

            MainTable(h, arr, false);

            h.PAGINATION(arr?.Length == 30);
        }, false, 120);
    }
}