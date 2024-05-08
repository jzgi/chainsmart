using System;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

namespace ChainSmart;

public abstract class BatVarWork : WebWork
{
    public virtual async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE id = @1");
        var o = await dc.QueryTopAsync<Bat>(p => p.Set(id));

        wc.GivePane(200, h =>
        {
            h.FORM_().FIELDSUL_("基本");

            h.LI_().FIELD("操作类型", o.typ, Bat.Typs)._LI();
            h.LI_().FIELD("商品", o.name)._LI();
            h.LI_().FIELD("附加说明", o.tip)._LI();
            h.LI_().FIELD("当前存量", o.stock)._LI();
            h.LI_().FIELD("数量", o.qty)._LI();
            if (o.srcid > 0)
            {
                var src = GrabTwin<int, Org>(o.srcid);
                h.LI_().FIELD("产源", src.name)._LI();
            }
            if (o.hubid > 0)
            {
                var hub = GrabTwin<int, Org>(o.hubid);
                h.LI_().FIELD("品控云仓", hub.name)._LI();
            }

            h._FIELDSUL().FIELDSUL_("状态");

            h.LI_().FIELD("状态", o.status, Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "生效", o.oker, o.oked, sep: "<br>")._LI();

            h._FIELDSUL()._FORM();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }
}

public class ShplyBatVarWork : BatVarWork
{
    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改或调整消息", icon: "pencil", status: 1 | 2 | 4), Tool(ButtonShow)]
    public async Task upd(WebContext wc)
    {
    }
}

// supplier or source
//
public class SrclyBatVarWork : BatVarWork
{
    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改或调整消息", icon: "pencil", status: 1 | 2 | 4), Tool(ButtonShow)]
    public async Task upd(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE id = @1");
            var o = await dc.QueryTopAsync<Bat>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("消息类型", nameof(o.typ), o.typ, Bat.Typs)._LI();
                h.LI_().TEXT("标题", nameof(o.name), o.name, max: 12)._LI();
                // h.LI_().TEXTAREA("内容", nameof(o.content), o.content, max: 300)._LI();
                h.LI_().TEXTAREA("注解", nameof(o.tip), o.tip, max: 40)._LI();
                // h.LI_().SELECT("级别", nameof(o.rank), o.rank, Lotop.Ranks)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(upd))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            var m = await wc.ReadObjectAsync(msk, new Bat
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE bats ")._SET_(Bat.Empty, msk).T(" WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(id).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }


    [MgtAuthorize(Org.TYP_SRC, User.ROL_MGT)]
    [Ui("发货", "安排发布", status: 1), Tool(ButtonConfirm)]
    public async Task adapt(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;


        short tag = org.tag;
        int nstart = 0, nend = 0;

        if (wc.IsGet)
        {
            var tags = Grab<short, Tag>();

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();

                h.LI_().SELECT("溯源标签", nameof(tag), tag, tags)._LI();
                h.LI_().NUMBER("起始号", nameof(nstart), nstart, min: 1)._LI();
                h.LI_().NUMBER("截至号", nameof(nend), nend, min: 1)._LI();

                h._FIELDSUL().BUTTON(nameof(adapt), "确认")._FORM();
            });
        }
        else
        {
            using var dc = NewDbContext();
            dc.Sql("UPDATE bats SET status = 2, adapted = @1, adapter = @2 WHERE id = @3 AND orgid = @4 RETURNING ").collst(Bat.Empty);
            var o = await dc.QueryTopAsync<Bat>(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

            // org.EventPack.AddMsg(o);

            wc.GivePane(200);
        }
    }
}

public class HublyBatVarWork : BatVarWork
{
    [MgtAuthorize(Org.TYP_HUB, User.ROL_OPN)]
    [Ui("收货", "收货入仓", status: 2), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var hub = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE bats SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND hubidid = @4 RETURNING ").collst(Bat.Empty);
        var o = await dc.QueryTopAsync<Bat>(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(hub.id));

        // var org = GrabTwin<int, Org>(o.orgid);
        // hub.EventPack.a.AddMsg(o);

        wc.GivePane(200);
    }
}