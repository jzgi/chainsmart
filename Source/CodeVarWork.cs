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
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM tags WHERE id = @1 AND parentid = @2");
        var o = await dc.QueryTopAsync<Code>(p => p.Set(id).Set(org.id));

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("用户名", o.name)._LI();
            h.LI_().FIELD("受检商户", o.name)._LI();
            h.LI_().FIELD("说明", o.tip)._LI();
            h.LI_().FIELD("身份证号", o.nstart)._LI();
            h.LI_().FIELD("民生卡号", o.nend)._LI();

            h.LI_().FIELD("状态", o.status, Code.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "生效", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }
}

public class PublyCodeVarWork : CodeVarWork
{
    public override async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT id FROM lots_vw WHERE nend >= @1 AND nstart <= @1 ORDER BY nend ASC LIMIT 1");
        if (await dc.QueryTopAsync(p => p.Set(id)))
        {
            dc.Let(out int lotid);
            wc.GiveRedirect("/lot/" + lotid + "/");
        }
        else
        {
            wc.GivePage(304, h => h.ALERT("此溯源码没有绑定产品"));
        }
    }
}

public class SuplyCodeVarWork : CodeVarWork
{
    [MgtAuthorize(Org.TYP_MKT, User.ROL_OPN)]
    [Ui(tip: "修改或调整孵化对象", icon: "pencil", status: 1), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var regs = Grab<short, Reg>();
        var org = wc[-2].As<Org>();
        var orgs = GrabTwinArray<int, Org>(org.id);
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Code.Empty).T(" FROM tags WHERE id = @1");
            var o = await dc.QueryTopAsync<Code>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);
                // h.LI_().TEXT("身份证号", nameof(o.nstart), o.nstart, min: 18, max: 18)._LI();
                h.LI_().SELECT("民生卡类型", nameof(o.typ), o.typ, Code.Typs, filter: (k, _) => k <= 2, required: true)._LI();
                // h.LI_().TEXT("民生卡号", nameof(o.nend), o.nend, min: 4, max: 8)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = Code.MSK_EDIT;
            var m = await wc.ReadObjectAsync(msk, new Test
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE tags ")._SET_(Test.Empty, msk).T(" WHERE id = @1 AND parentid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(id).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_MKT, User.ROL_MGT)]
    [Ui("发布", "公示该检测记录", status: 1 | 2), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE tags SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND parentid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [MgtAuthorize(Org.TYP_MKT, User.ROL_MGT)]
    [Ui("下线", "下线停用或调整", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE tags SET status = 1, oked = NULL, oker = NULL WHERE id = @1 AND parentid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.GivePane(200);
    }
}

public class AdmlyCodeVarWork : CodeVarWork
{
}