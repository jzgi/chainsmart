using System;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart;

public abstract class TestVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Test.Empty).T(" FROM tests WHERE id = @1 AND upperid = @2");
        var o = await dc.QueryTopAsync<Test>(p => p.Set(id).Set(org.id));

        var suborg = GrabTwin<int, Org>(o.orgid);


        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("受检物", o.name)._LI();
            h.LI_().FIELD("受检单位", suborg.name)._LI();
            h.LI_().FIELD("检测项目", o.tip)._LI();
            h.LI_().FIELD("分值", o.val)._LI();
            h.LI_().FIELD("结论", o.level, Test.Levels)._LI();

            h.LI_().FIELD("状态", o.status, Fact.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "发布", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "调整事务信息", icon: "pencil", status: 1 | 2), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var regs = Grab<short, Reg>();
        var org = wc[-2].As<Org>();
        var orgs = GrabTwinSet<int, Org>(org.id);
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Test.Empty).T(" FROM tests WHERE id = @1");
            var o = await dc.QueryTopAsync<Test>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);
                h.LI_().SELECT("类型", nameof(o.typ), o.typ, Test.Typs)._LI();
                h.LI_().TEXT("受检物", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().LABEL("受检单位").SELECT_ORG(nameof(o.orgid), o.orgid, orgs, regs)._LI();
                h.LI_().TEXTAREA("检测项目", nameof(o.tip), o.tip, min: 2, max: 10)._LI();
                h.LI_().NUMBER("分值", nameof(o.val), o.val)._LI();
                h.LI_().SELECT("结论", nameof(o.level), o.level, Test.Levels)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            var m = await wc.ReadObjectAsync(msk, new Test
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE tests ")._SET_(Test.Empty, msk).T(" WHERE id = @1 AND upperid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(id).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("发布", "公示该检测记录", status: 1 | 2), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE tests SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND upperid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("下线", "下线停用或调整", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE tests SET status = 1, oked = NULL, oker = NULL WHERE id = @1 AND upperid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.GivePane(200);
    }
}

public class MktlyTestVarWork : TestVarWork
{
}

public class CtrlyTestVarWork : TestVarWork
{
}