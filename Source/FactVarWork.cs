using System;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart;

public abstract class FactVarWork : WebWork
{
    public  async Task @default(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        const short msk = 255 | MSK_AUX;

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Fact.Empty, msk).T(" FROM facts WHERE id = @1 AND orgid = @2");
        var o = await dc.QueryTopAsync<Fact>(p => p.Set(id).Set(org.id), msk);

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("事务名", o.name)._LI();
            h.LI_().FIELD("内容", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();

            h.LI_().FIELD("状态", o.status, Fact.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "发布", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }
}

public class MktlyFactVarWork : FactVarWork
{
    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "调整事务信息", icon: "pencil", status: 1 | 2), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM facts WHERE id = @1");
            var o = await dc.QueryTopAsync<Item>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("新建" + Fact.Typs[o.typ]);

                h.LI_().TEXT("事务名", nameof(o.name), o.name, max: 12)._LI();
                h.LI_().TEXTAREA("内容", nameof(o.tip), o.tip, max: 40)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            var m = await wc.ReadObjectAsync(msk, new Fact
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE facts ")._SET_(Fact.Empty, msk).T(" WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(id).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }


    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("发布", "安排发布", status: 1 | 2 | 4), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE facts SET status = 4, oked = @1, oker = @2, num = num + 1 WHERE id = @3 AND orgid = @4 RETURNING ").collst(Fact.Empty);
        var o = await dc.QueryTopAsync<Fact>(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        org.EventPack.AddFact(o);

        wc.GivePane(200);
    }
}

public class CtrlyFactVarWork : FactVarWork
{
}