using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Storage;

namespace ChainSmart;

public abstract class RegVarWork : WebWork
{
}

public class AdmlyRegVarWork : RegVarWork
{
    public async Task @default(WebContext wc, int typ)
    {
        short id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE id = @1");
        var o = await dc.QueryTopAsync<Reg>(p => p.Set(id));

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("编号", o.id)._LI();
            h.LI_().FIELD("名称", o.name)._LI();
            h.LI_().FIELD("简介语", o.tip)._LI();
            h.LI_().FIELD("序号", o.idx)._LI();
            h.LI_().FIELD("市场模式", o.style, Org.Styles)._LI();
            h.LI_().FIELD("状态", o.status, Entity.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "发布", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }

    [Ui(icon: "pencil", status: 1 | 2), Tool(ButtonShow)]
    public async Task upd(WebContext wc)
    {
        short id = wc[0];
        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE id = @1");
            var o = await dc.QueryTopAsync<Reg>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("区域属性");

                h.LI_().NUMBER("编号", nameof(o.id), o.id, min: 1, max: 99, required: true)._LI();
                h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, min: 2, max: 40)._LI();
                h.LI_().NUMBER("排序", nameof(o.idx), o.idx, min: 1, max: 99)._LI();
                if (o.IsSector)
                {
                    h.LI_().SELECT("市场模式", nameof(o.style), o.style, Org.Styles)._LI();
                }

                h._FIELDSUL()._FORM();
            });
        }
        else
        {
            var o = await wc.ReadObjectAsync<Reg>();
            using var dc = NewDbContext();
            dc.Sql("UPDATE regs")._SET_(o).T(" WHERE id = @1");
            await dc.ExecuteAsync(p =>
            {
                o.Write(p);
                p.Set(id);
            });
            wc.GivePane(200);
        }
    }

    [MgtAuthorize(0, User.ROL_MGT)]
    [Ui("上线", "上线投入使用", status: 1 | 2), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE regs SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND status BETWEEN 1 AND 2");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id));

        wc.Give(200);
    }

    [MgtAuthorize(0, User.ROL_MGT)]
    [Ui("下线", "下线停用或调整", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("UPDATE regs SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND status = 4")._MEET_(wc);
        await dc.ExecuteAsync(p => p.Set(id));

        wc.Give(200);
    }
}