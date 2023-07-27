using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart;

public abstract class EvalWork<V> : WebWork where V : EvalVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Eval> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name);

            // h.SPAN(Statuses[o.status], "uk-badge");
            h._HEADER();

            h.Q(o.tip, "uk-width-expand");
            h.FOOTER_().SPAN2("未用量", o.level).SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }


    [Ui("信用管理", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Eval.Empty).T(" FROM evals WHERE orgid = @1 AND status = 4");
        var arr = await dc.QueryAsync<Eval>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 4);
            if (arr == null)
            {
                h.ALERT("尚无上榜的信用记录");
                return;
            }
            MainGrid(h, arr);
        }, false, 12);
    }

    [Ui(tip: "已下榜", icon: "cloud-download", status: 2), Tool(Anchor)]
    public async Task down(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Eval.Empty).T(" FROM evals WHERE orgid = @1 AND status BETWEEN 1 AND 2 ORDER BY adapted DESC");
        var arr = await dc.QueryAsync<Eval>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);

            if (arr == null)
            {
                h.ALERT("尚无已下榜的信用记录");
                return;
            }

            MainGrid(h, arr);
        }, false, 4);
    }

    [Ui(tip: "已删除", icon: "trash", status: 8), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Eval.Empty).T(" FROM evals WHERE orgid = @1 AND status = 0 ORDER BY adapted DESC");
        var arr = await dc.QueryAsync<Eval>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无已删除的信用记录");
                return;
            }

            MainGrid(h, arr);
        }, false, 4);
    }

    [Ui("新建", icon: "plus", status: 1 | 2), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int stu)
    {
        var prin = (User)wc.Principal;
        var o = new Eval
        {
            created = DateTime.Now,
            creator = prin.name,
            status = (short)stu
        };
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("新建信用记录");
                h.LI_().TEXT("标题", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().SELECT("类型", nameof(o.typ), o.typ, Eval.Typs)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, min: 2, max: 40)._LI();
                h.LI_().NUMBER("层级", nameof(o.level), o.level, min: 0, max: 9)._LI();
                h.LI_().SELECT("状态", nameof(o.status), o.status, Eval.Statuses)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
            });
        }
        else // POST
        {
            const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;

            o = await wc.ReadObjectAsync(msk, instance: o);
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO evals ").colset(Eval.Empty, msk)._VALUES_(Eval.Empty, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

[OrglyAuthorize(Org.TYP_MKT)]
[Ui("信用管理")]
public class MktlyEvalWork : EvalWork<MktlyEvalVarWork>
{
}

[OrglyAuthorize(Org.TYP_CTR)]
[Ui("信用管理")]
public class CtrlyEvalWork : EvalWork<CtrlyEvalVarWork>
{
}