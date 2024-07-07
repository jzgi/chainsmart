using System;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

namespace ChainSmart;

public abstract class CodeVarWork : WebWork
{
    public virtual async Task @default(WebContext wc)
    {
        int id = wc[0];
        var tags = Grab<short, Tag>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE id = @1");
        var o = await dc.QueryTopAsync<Code>(p => p.Set(id));


        wc.GivePane(200, h =>
        {
            h.H4("基本", css: "uk-padding");
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("产源", o.name)._LI();
            h.LI_().FIELD(string.Empty, o.tel)._LI();
            h.LI_().FIELD(string.Empty, o.addr)._LI();
            h.LI_().FIELD("码类型", o.tag, tags).FIELD("码数量", o.Num)._LI();
            h.LI_().FIELD("起始号", o.nstart, digits: 8).FIELD("截止号", o.nend, digits: 8)._LI();
            h.LI_().FIELD("备注", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
            h._UL();

            h.H4("状态", css: "uk-padding");
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("状态", o.status, Code.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("修改", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "发放", o.oker, o.oked, sep: "<br>")._LI();
            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }
}

public class SrclyCodeVarWork : CodeVarWork
{
}

public class AdmlyCodeVarWork : CodeVarWork
{
    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui("发放", "发放溯源码", status: 1 | 2), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE codes SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND (status = 1 OR status = 2)");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id));

        wc.GivePane(200);
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui("撤回", tip: "撤回发放", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("UPDATE codes SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND status = 4");
        await dc.ExecuteAsync(p => p.Set(id));

        wc.GivePane(200);
    }
}