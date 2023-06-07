using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart;

public abstract class AggWork<V> : WebWork where V : AggVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainTable(HtmlBuilder h, Agg[] arr)
    {
        h.TABLE(arr, o =>
        {
            h.TD_().T(o.dt, 3, 0)._TD();
            h.TD(o.name);
            h.TD_("uk-text-right").T(o.trans)._TD();
            h.TD_("uk-text-right").CNY(o.amt)._TD();
        }, thead: () =>
        {
            h.TH("日期", css: "uk-width-medium");
            h.TH("类型");
            h.TH("数量");
            h.TH("金额");
        });
    }

    [Ui("结算", "结算代收款项", icon: "list", group: 1), Tool(ButtonOpen)]
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

    protected static void ClearTable(HtmlBuilder h, IEnumerable<Clear> arr)
    {
        // h.TABLE_();
        // var last = 0;
        // foreach (var o in arr)
        // {
        //     if (o.since != last)
        //     {
        //         var spr = GrabObject<int, Org>(o.since);
        //         h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
        //     }
        //     h.TR_();
        //     h.TD(o.till);
        //     h.TD(o.name);
        //     h._TR();
        //
        //     last = o.since;
        // }
        // h._TABLE();
    }
}

[AdmlyAuthorize(User.ROL_FIN)]
[Ui("市场业务汇总")]
public class AdmlyBuyAggWork : AggWork<AdmlyBuyAggVarWork>
{
    [Ui("市场业务", group: 1), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Agg.Empty).T(" FROM buyaggs_typ ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
        var arr = dc.Query<Agg>(p => p.Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE(arr, o =>
            {
                h.TD_().T(o.dt, 3, 0)._TD();
                h.TD(Buy.Typs[(short)o.typ]);
                h.TD_("uk-text-right").T(o.trans)._TD();
                h.TD_("uk-text-right").CNY(o.amt)._TD();
            }, thead: () =>
            {
                h.TH("日期", css: "uk-width-medium");
                h.TH("类型");
                h.TH("笔数");
                h.TH("金额");
            });
            h.PAGINATION(arr?.Length == 30);
        }, false, 60);
    }
}

[AdmlyAuthorize(User.ROL_FIN)]
[Ui("供应业务汇总")]
public class AdmlyPurAggWork : AggWork<AdmlyPurAggVarWork>
{
    [Ui("供应业务", group: 1), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        wc.GivePage(200, h => { h.TOOLBAR(); });
    }
}

[Ui("销售业务日总")]
public class RtllyBuyAggWork : AggWork<RtllyBuyAggVarWork>
{
    [Ui("按商品", group: 1), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Agg.Empty).T(" FROM buyaggs_itemid WHERE orgid = @1 ORDER BY dt DESC, typ LIMIT 30 OFFSET 30 * @2");
        var arr = dc.Query<Agg>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无汇总记录");
                return;
            }

            MainTable(h, arr);

            h.PAGINATION(arr.Length == 30);
        }, false, 60);
    }

    [Ui("按交易", group: 2), Tool(Anchor)]
    public void typ(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Agg.Empty).T(" FROM buyaggs_typ WHERE orgid = @1 ORDER BY dt DESC, typ LIMIT 30 OFFSET 30 * @2");
        var arr = dc.Query<Agg>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无汇总记录");
                return;
            }

            MainTable(h, arr);

            h.PAGINATION(arr.Length == 30);
        }, false, 60);
    }
}

[Ui("采购业务日总")]
public class RtllyPurAggWork : AggWork<RtllyPurAggVarWork>
{
    [Ui("按产品批次", group: 1), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Agg.Empty).T(" FROM puraggs_lotid WHERE orgid = @1 ORDER BY dt DESC, typ LIMIT 30 OFFSET 30 * @2");
        var arr = dc.Query<Agg>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无汇总记录");
                return;
            }

            MainTable(h, arr);

            h.PAGINATION(arr.Length == 30);
        }, false, 60);
    }

    [Ui("按交易", group: 2), Tool(Anchor)]
    public void typ(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Agg.Empty).T(" FROM puraggs_typ WHERE orgid = @1 ORDER BY dt DESC, typ LIMIT 30 OFFSET 30 * @2");
        var arr = dc.Query<Agg>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无汇总记录");
                return;
            }

            MainTable(h, arr);

            h.PAGINATION(arr.Length == 30);
        }, false, 60);
    }
}

[Ui("销售业务日总")]
public class SuplyPurAggWork : AggWork<SuplyPurAggVarWork>
{
    [Ui("按产品批次", group: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT * FROM puraggs_lotid WHERE orgid = @1 ORDER BY dt DESC, typ LIMIT 30 OFFSET 30 * @2");
        var arr = await dc.QueryAsync<Agg>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无汇总记录");
                return;
            }

            MainTable(h, arr);

            h.PAGINATION(arr.Length == 30);
        }, false, 60);
    }

    [Ui("按交易", group: 2), Tool(Anchor)]
    public async Task typ(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT * FROM puraggs_typ WHERE orgid = @1 ORDER BY dt DESC, typ LIMIT 30 OFFSET 30 * @2");
        var arr = await dc.QueryAsync<Agg>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无汇总记录");
                return;
            }

            h.TABLE(arr, o =>
            {
                h.TD_().T(o.dt, 3, 0)._TD();
                h.TD(Buy.Typs[(short)o.typ]);
                h.TD_("uk-text-right").T(o.trans)._TD();
                h.TD_("uk-text-right").CNY(o.amt)._TD();
            }, thead: () =>
            {
                h.TH("日期", css: "uk-width-medium");
                h.TH("类型");
                h.TH("笔数");
                h.TH("金额");
            });
            h.PAGINATION(arr.Length == 30);
        }, false, 60);
    }
}

[OrglyAuthorize(Org.TYP_MKT)]
[Ui("销售情况汇总")]
public class MktlyBuyAggWork : AggWork<AggVarWork>
{
    public void @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            h.ALERT("暂无生成报表");
        }, false, 60);
    }
}

[OrglyAuthorize(Org.TYP_MKT)]
[Ui("采购情况汇总")]
public class MktlyPurAggWork : AggWork<AggVarWork>
{
    public void @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            h.ALERT("暂无生成报表");
        }, false, 60);
    }
}

[OrglyAuthorize(Org.TYP_CTR)]
[Ui("销售情况汇总")]
public class CtrlyPurAggWork : AggWork<AggVarWork>
{
    public void @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            h.ALERT("暂无生成报表");
        }, false, 60);
    }
}