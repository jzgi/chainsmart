using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public abstract class CarbWork<V> : WebWork where V : CarbVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>(state: State);
    }

    protected static void MainTable(HtmlBuilder h, IList<Carb> lst, short level = 1)
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
            // h.TD_().ADIALOG_(o.orgid, "/", mode: 32, pick: false).T(o.topay)._A()._TD();
            h._TR();
        }
        h._TABLE();
    }
}

[Ui("碳汇信用", "采购生态环保产品的额外奖励", icon: "world")]
public class OrglyCarbApWork : CarbWork<OrglyCarbVarWork>
{
    protected override void OnCreate()
    {
        CreateVarWork<AdmlyRegVarWork>();
    }

    [Ui("碳积分", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();

        await dc.QueryTopAsync("SELECT carb FROM orgs_vw WHERE id = @1", p => p.Set(org.id));
        dc.Let(out decimal carb);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.ALERT("余额：", carb.ToString());
        }, false, 12);
    }

    [Ui(tip: "兑换记录", icon: "history", status: 2), Tool(Anchor)]
    public async Task past(WebContext wc)
    {
        var org = wc[0].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Carb.Empty).T(" FROM carbs WHERE orgid = @1 ORDER BY dt DESC");
        var arr = await dc.QueryAsync<Carb>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("没有核销记录");
                return;
            }

            MainTable(h, arr);
        }, false, 12);
    }
}