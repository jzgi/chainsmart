using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Nodality;

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

        var till = await dc.ScalarAsync("SELECT max(till) FROM buygens") as DateTime?;

        dc.Sql("SELECT xorgid, sum(trans), sum(amt), first(rate), sum(topay) FROM buyaps WHERE level = 1 AND dt = @1 GROUP BY xorgid");
        await dc.QueryAsync(p => p.Set(till ?? default(DateTime)));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("机构").TH("笔数", "uk-text-right").TH("总营业额", "uk-text-right").TH("总应付额", "uk-text-right")._THEAD();
            while (dc.Next())
            {
                dc.Let(out int xorgid);
                dc.Let(out int trans);
                dc.Let(out decimal amt);
                dc.Let(out short rate);
                dc.Let(out decimal topay);

                var xorg = GrabTwin<int, Org>(xorgid);

                h.TR_();
                h.TD_().A_(xorgid, "/?dt=", till).T(xorg.cover)._A()._TD();
                h.TD(trans);
                h.TD(amt, money: true, right: true);
                h.TD(topay, money: true, right: true)._TD();
                h._TR();
            }
            h._TABLE();
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
            dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM buyaps WHERE level = 1 ORDER BY dt LIMIT 40 OFFSET @1 * 40");
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

    [UserAuthorize(0, User.ROL_FIN)]
    [Ui("结算", icon: "plus-circle", status: 1), Tool(ButtonOpen)]
    public async Task gen(WebContext wc)
    {
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Gen.Empty).T(" FROM buygens ORDER BY till LIMIT 8");
            var arr = await dc.QueryAsync<Gen>();

            var till = DateTime.Today.AddDays(-1);
            wc.GivePane(200, h =>
            {
                h.FORM_();
                h.FIELDSUL_("生成业务汇总及应付帐");
                h.LI_().DATE("截止日期", nameof(till), till, max: till)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(gen));
                h._FORM();

                h.TABLE(arr, o => h.TD(o.till).TD(o.last).TD(o.opr), () => h.TH("截止").TH("上次").TH("操作"));
            });
        }
        else // OUTER
        {
            var f = await wc.ReadAsync<Form>();
            DateTime till = f[nameof(till)];
            using var dc = NewDbContext(IsolationLevel.RepeatableRead);
            await dc.ExecuteAsync("SELECT buygen(@1, @2)", p => p.Set(till).Set(prin.name));

            wc.GivePane(200);
        }
    }
}

[Ui("供应端应付")]
public class AdmlyPurApWork : ApWork<AdmlyPurApVarWork>
{
    [Ui("供应端应付", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        using var dc = NewDbContext();

        var till = await dc.ScalarAsync("SELECT max(till) FROM purgens") as DateTime?;

        dc.Sql("SELECT xorgid, sum(trans), sum(amt), first(rate), sum(topay) FROM puraps WHERE level = 1 AND dt = @1 GROUP BY xorgid");
        await dc.QueryAsync(p => p.Set(till ?? default(DateTime)));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("机构").TH("笔数", "uk-text-right").TH("总营业额", "uk-text-right").TH("总应付额", "uk-text-right")._THEAD();
            while (dc.Next())
            {
                dc.Let(out int xorgid);
                dc.Let(out int trans);
                dc.Let(out decimal amt);
                dc.Let(out short rate);
                dc.Let(out decimal topay);

                var xorg = GrabTwin<int, Org>(xorgid);

                h.TR_();
                h.TD_().A_(xorgid, "/?dt=", till).T(xorg.cover)._A()._TD();
                h.TD(trans);
                h.TD(amt, money: true, right: true);
                h.TD(topay, money: true, right: true)._TD();
                h._TR();
            }
            h._TABLE();
        }, false, 120);
    }

    [Ui(tip: "历史", icon: "history", status: 2), Tool(AnchorPrompt)]
    public async Task past(WebContext wc, int page)
    {
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

    [UserAuthorize(0, User.ROL_FIN)]
    [Ui("结算", icon: "plus-circle", status: 1), Tool(ButtonOpen)]
    public async Task gen(WebContext wc)
    {
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Gen.Empty).T(" FROM purgens ORDER BY till LIMIT 8");
            var arr = await dc.QueryAsync<Gen>();

            var till = DateTime.Today.AddDays(-1);
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("生成业务汇总及应付帐");
                h.LI_().DATE("截止日期", nameof(till), till, max: till)._LI();
                h._FIELDSUL();
                h.BOTTOM_BUTTON("确认", nameof(gen));
                h._FORM();

                h.TABLE(arr, o => h.TD(o.till).TD(o.last).TD(o.opr), () => h.TH("截止").TH("上次").TH("操作"));
            });
        }
        else // OUTER
        {
            var f = await wc.ReadAsync<Form>();
            DateTime till = f[nameof(till)];
            using var dc = NewDbContext(IsolationLevel.RepeatableRead);
            await dc.ExecuteAsync("SELECT purgen(@1, @2)", p => p.Set(till).Set(prin.name));

            wc.GivePane(200);
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

[Ui("销售结款")]
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