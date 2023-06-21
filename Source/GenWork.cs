using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart;

public abstract class GenWork<V> : WebWork where V : GenVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }
}

[Ui("市场端结算")]
public class AdmlyBuyGenWork : GenWork<AdmlyBuyGenVarWork>
{
    [Ui("结算记录"), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Gen.Empty).T(" FROM buygens");
        var arr = dc.Query<Ldg>();

        wc.GivePage(200, h => { h.TOOLBAR(); });
    }

    [AdmlyAuthorize(User.ROL_FIN)]
    [Ui("结算", "市场业务分类汇总及应付帐款计算", icon: "plus-circle", status: 1), Tool(ButtonOpen)]
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

[Ui("供应端结算")]
public class AdmlyPurGenWork : GenWork<AdmlyPurGenVarWork>
{
    [Ui("结算记录"), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Gen.Empty).T(" FROM buygens");
        var arr = dc.Query<Ldg>();

        wc.GivePage(200, h => { h.TOOLBAR(); });
    }

    [AdmlyAuthorize(User.ROL_FIN)]
    [Ui("结算", "供应业务分类汇总及应付帐款计算", icon: "plus-circle", status: 1), Tool(ButtonOpen)]
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