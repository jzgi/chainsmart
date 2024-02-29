using System;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

namespace ChainSmart;

public abstract class FlowVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Flow.Empty).T(" FROM flows WHERE id = @1 AND parentid = @2");
        var o = await dc.QueryTopAsync<Flow>(p => p.Set(id).Set(org.id));

        var suborg = GrabTwin<int, Org>(o.orgid);


        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("受检商品", o.name)._LI();
            h.LI_().FIELD("受检商户", suborg.name)._LI();
            h.LI_().FIELD("说明", o.tip)._LI();
            h.LI_().FIELD("分值", o.dataid)._LI();

            h.LI_().FIELD("状态", o.status, Flow.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "发布", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }

    [MgtAuthorize(Org.TYP_RTL_MKT, User.ROL_OPN)]
    [Ui(tip: "修改或调整检测记录", icon: "pencil", status: 1 | 2), Tool(ButtonShow)]
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
            dc.Sql("SELECT ").collst(Flow.Empty).T(" FROM flows WHERE id = @1");
            var o = await dc.QueryTopAsync<Flow>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);
                h.LI_().SELECT("检测类型", nameof(o.typ), o.typ, Flow.Typs)._LI();
                h.LI_().TEXT("受检商品", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().LABEL("受检商户").SELECT_ORG(nameof(o.orgid), o.orgid, orgs, regs)._LI();
                h.LI_().TEXTAREA("说明", nameof(o.tip), o.tip, min: 2, max: 20)._LI();
                h.LI_().NUMBER("分值", nameof(o.dataid), o.dataid)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            var m = await wc.ReadObjectAsync(msk, new Flow
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE flows ")._SET_(Flow.Empty, msk).T(" WHERE id = @1 AND parentid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(id).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_RTL_MKT, User.ROL_MGT)]
    [Ui("发布", "公示该检测记录", status: 1 | 2), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE flows SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND parentid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [MgtAuthorize(Org.TYP_RTL_MKT, User.ROL_MGT)]
    [Ui("下线", "下线停用或调整", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE flows SET status = 1, oked = NULL, oker = NULL WHERE id = @1 AND parentid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.GivePane(200);
    }
}

public class AdmlyFlowVarWork : FlowVarWork
{
}

public class MktlyFlowVarWork : FlowVarWork
{
}