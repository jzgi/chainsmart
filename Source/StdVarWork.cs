using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Storage;

namespace ChainSmart;

public abstract class StdVarWork : WebWork
{
    protected static void Show<M>(HtmlBuilder h, M o, string icon) where M : Std
    {
        h.SECTION_("uk-section");
        h.IMG(icon, css: "uk-width-1-4");
        h.H3(o.ToString(), css: "uk-flex-center uk-width-expand");
        h._SECTION();

        h.SECTION_("uk-section");

        h.T(o.tip);

        h._SECTION();
    }
}

public class PublyCatVarWork : StdVarWork
{
    public void @default(WebContext wc)
    {
        short typ = wc[0];
        var o = Grab<short, Cat>()[typ];

        wc.GivePane(200, h => Show(h, o, "/cat.webp"), true, 3600 * 6);
    }
}

public class PublyEnvVarWork : StdVarWork
{
    public void @default(WebContext wc)
    {
        short typ = wc[0];
        var o = Grab<short, Env>()[typ];

        wc.GivePane(200, h => Show(h, o, "/env.webp"), true, 3600 * 6);
    }
}

public class PublyTagVarWork : StdVarWork
{
    public void @default(WebContext wc)
    {
        short typ = wc[0];
        var o = Grab<short, Tag>()[typ];

        wc.GivePane(200, h => Show(h, o, "/tag.webp"), true, 3600 * 6);
    }
}

public class PublySymVarWork : StdVarWork
{
    public void @default(WebContext wc)
    {
        short typ = wc[0];
        var o = Grab<short, Sym>()[typ];

        wc.GivePane(200, h => Show(h, o, "/sym.webp"), true, 3600 * 6);
    }
}

public class AdmlyStdVarWork : StdVarWork
{
    public async Task @default(WebContext wc, int sub)
    {
        short typ = wc[0];
        var descr = Std.Descrs[(short)sub];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Std.Empty).T(" FROM ").T(descr.DbTable).T(" WHERE typ = @1");
        var o = await dc.QueryTopAsync<Std>(p => p.Set(typ));

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD(descr.Title + "编号", o.typ)._LI();
            h.LI_().FIELD("名称", o.name)._LI();
            h.LI_().FIELD("简介语", o.tip)._LI();
            h.LI_().FIELD("序号", o.idx)._LI();
            h.LI_().FIELD("风格", o.style, Std.Styles)._LI();
            h.LI_().FIELD("状态", o.status, Entity.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "发布", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, subscript: sub, status: o.Status, state: o.ToState());
        }, false, 6);
    }

    [Ui(icon: "pencil", status: 1 | 2), Tool(ButtonShow)]
    public async Task upd(WebContext wc, int sub)
    {
        short typ = wc[0];
        var descr = Std.Descrs[(short)sub];

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Std.Empty).T(" FROM ").T(descr.DbTable).T(" WHERE typ = @1");
            var o = await dc.QueryTopAsync<Std>(p => p.Set(typ));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(descr.Title + "属性");

                h.LI_().NUMBER("编号", nameof(o.typ), o.typ, min: 1, max: 99, required: true)._LI();
                h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, min: 2, max: 40)._LI();
                h.LI_().NUMBER("排序", nameof(o.idx), o.idx, min: 1, max: 99)._LI();

                h._FIELDSUL()._FORM();
            });
        }
        else
        {
            var o = await wc.ReadObjectAsync<Reg>();
            using var dc = NewDbContext();
            dc.Sql("UPDATE ").T(descr.DbTable)._SET_(o).T(" WHERE id = @1");
            await dc.ExecuteAsync(p =>
            {
                o.Write(p);
                p.Set(typ);
            });
            wc.GivePane(200);
        }
    }

    [MgtAuthorize(0, User.ROL_MGT)]
    [Ui("上线", "上线投入使用", status: 1 | 2), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc, int sub)
    {
        int typ = wc[0];
        var prin = (User)wc.Principal;
        var descr = Std.Descrs[(short)sub];

        using var dc = NewDbContext();
        dc.Sql("UPDATE ").T(descr.DbTable).T(" SET status = 4, oked = @1, oker = @2 WHERE typ = @3 AND status BETWEEN 1 AND 2");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(typ));

        wc.Give(200);
    }

    [MgtAuthorize(0, User.ROL_MGT)]
    [Ui("下线", "下线停用或调整", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc, int sub)
    {
        short typ = wc[0];
        var descr = Std.Descrs[(short)sub];

        using var dc = NewDbContext();
        dc.Sql("UPDATE ").T(descr.DbTable).T(" SET status = 2, oked = NULL, oker = NULL WHERE typ = @1 AND status = 4")._MEET_(wc);
        await dc.ExecuteAsync(p => p.Set(typ));

        wc.Give(200);
    }
}