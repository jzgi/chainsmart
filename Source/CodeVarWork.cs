using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using NPOI.SS.Formula.PTG;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

namespace ChainSmart;

public abstract class CodeVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE id = @1");
        var o = await dc.QueryTopAsync<Code>(p => p.Set(id));

        var org = wc[-2].As<Org>() ?? GrabTwin<int, Org>(o.orgid);

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("名称", o.name)._LI();
            h.LI_().FIELD("申请方", org.name)._LI();
            h.LI_().FIELD("申请数量", o.num)._LI();
            h.LI_().FIELD("附注", o.tip)._LI();
            h.LI_().FIELD("起始号", o.nstart, digits: 8).FIELD("截止号", o.nend, digits: 8)._LI();

            h.LI_().FIELD("状态", o.status, Code.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("提交", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "发放", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }
}

public class SrclyCodeVarWork : CodeVarWork
{
    [MgtAuthorize(Org.TYP_SRC, User.ROL_MGT)]
    [Ui(tip: "修改申请", icon: "pencil", status: 1), Tool(ButtonShow)]
    public async Task upd(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;
        var tags = Grab<short, Tag>();

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE id = @1");
            var o = await dc.QueryTopAsync<Code>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("类型", nameof(o.typ), o.typ, tags, filter: (k, _) => k == o.typ, required: true)._LI();
                h.LI_().NUMBER("申请数量", nameof(o.num), o.num)._LI();
                h.LI_().TEXTAREA("附注", nameof(o.tip), o.tip, max: 30)._LI();

                h._FIELDSUL().BOTTOMBAR_().BUTTON("确认", nameof(upd))._BOTTOMBAR();
                h._FORM();
            });
        }
        else // POST
        {
            const short Msk = Code.MSK_EDIT | Code.MSK_TYP;
            var o = await wc.ReadObjectAsync(Msk, new Code
            {
                created = DateTime.Now,
                creator = prin.name,
            });
            var tag = tags[o.typ];
            o.name = tag.ToString();

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE codes ")._SET_(Code.Empty, Msk).T(" WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p =>
            {
                o.Write(p, Msk);
                p.Set(id).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_SRC, User.ROL_MGT)]
    [Ui("提交", "提交申请", status: 1), Tool(ButtonConfirm)]
    public async Task adapt(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE codes SET status = 2, adapted = @1, adapter = @2 WHERE id = @3 AND orgid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [MgtAuthorize(Org.TYP_SRC, User.ROL_MGT)]
    [Ui("撤回", "撤回申请", status: 2), Tool(ButtonConfirm)]
    public async Task unadapt(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE codes SET status = 1, adapted = NULL, adapter = NULL WHERE id = @3 AND orgid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.GivePane(200);
    }
}

public class AdmlyCodeVarWork : CodeVarWork
{
    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui("发放", "发放溯源码", status: 2), Tool(ButtonShow)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE id = @1");
            var o = await dc.QueryTopAsync<Code>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().NUMBER("起始号", nameof(o.nstart), o.nstart)._LI();
                h.LI_().NUMBER("截止号", nameof(o.nend), o.nend)._LI();

                h._FIELDSUL().BOTTOMBAR_().BUTTON("确认", nameof(ok))._BOTTOMBAR();
                h._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            int nstart = f[nameof(nstart)];
            int nend = f[nameof(nend)];

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE codes SET status = 4, oked = @1, oker = @2, nstart = @3, nend = @4 WHERE id = @5 AND status = 2");
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(nstart).Set(nend).Set(id));

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui("撤回", "撤回发放", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("UPDATE codes SET status = 2, oked = NULL, oker = NULL, nstart = NULL, nend = NULL WHERE id = @1 AND status = 4");
        await dc.ExecuteAsync(p => p.Set(id));

        wc.GivePane(200);
    }
}