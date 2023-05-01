using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Nodal;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public abstract class ProdVarWork : WebWork
{
    protected async Task doimg(WebContext wc, string col, bool shared, short maxage)
    {
        int id = wc[0];
        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").T(col).T(" FROM prods WHERE id = @1");
            if (await dc.QueryTopAsync(p => p.Set(id)))
            {
                dc.Let(out byte[] bytes);
                if (bytes == null) wc.Give(204); // no content 
                else wc.Give(200, new WebStaticContent(bytes), shared, maxage);
            }
            else
            {
                wc.Give(404, null, shared, maxage); // not found
            }
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            ArraySegment<byte> img = f[nameof(img)];
            using var dc = NewDbContext();
            dc.Sql("UPDATE prods SET ").T(col).T(" = @1 WHERE id = @2");
            if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
            {
                wc.Give(200); // ok
            }
            else wc.Give(500); // internal server error
        }
    }
}

public class PublyProdVarWork : ProdVarWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Prod.Empty).T(" FROM prods_vw WHERE id = @1");
        var o = await dc.QueryTopAsync<Prod>(p => p.Set(id));

        wc.GivePage(200, h =>
        {
            h.ARTICLE_("uk-card uk-card-primary");
            h.H4("产源设施", "uk-card-header");
            h.UL_("uk-card-body uk-list uk-list-divider");
            h.LI_().FIELD("名称", o.name)._LI();
            h.LI_().FIELD("类别", o.typ, Prod.Typs)._LI();
            h.LI_().FIELD("简介", o.tip)._LI();
            h.LI_().FIELD("碳减排项目", o.co2ep)._LI();
            h.LI_().FIELD("规格参数", o.specs)._LI();
            h.LI_().FIELD2("创建", o.created, o.creator)._LI();
            if (o.adapter != null)
            {
                h.LI_().FIELD2("制码", o.adapted, o.adapter)._LI();
            }

            if (o.oker != null)
            {
                h.LI_().FIELD2("上线", o.oked, o.oker)._LI();
            }

            h._UL();

            if (o.m1)
            {
                h.PIC("/prod/", o.id, "/m-1", css: "uk-width-1-1 uk-card-body");
            }

            if (o.m2)
            {
                h.PIC("/prod/", o.id, "/m-2", css: "uk-width-1-1 uk-card-body");
            }

            if (o.m3)
            {
                h.PIC("/prod/", o.id, "/m-3", css: "uk-width-1-1 uk-card-body");
            }

            if (o.m4)
            {
                h.PIC("/prod/", o.id, "/m-4", css: "uk-width-1-1 uk-card-body");
            }

            h._ARTICLE();
        }, true, 900, o.name);
    }


    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), true, 7200);
    }

    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), true, 7200);
    }

    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, nameof(m) + sub, true, 7200);
    }
}

public class SuplyProdVarWork : ProdVarWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Prod.Empty).T(" FROM prods_vw WHERE id = @1");
        var o = await dc.QueryTopAsync<Prod>(p => p.Set(id));

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("产品源名称", o.name)._LI();
            h.LI_().FIELD("类别", o.typ, Prod.Typs)._LI();
            h.LI_().FIELD("简介", o.tip)._LI();
            h.LI_().FIELD("说明", o.remark)._LI();
            h.LI_().FIELD("规格参数", o.specs)._LI();

            h.LI_().FIELD2("创建", o.created, o.creator)._LI();
            if (o.adapter != null) h.LI_().FIELD2("修改", o.adapted, o.adapter)._LI();
            if (o.oker != null) h.LI_().FIELD2("上线", o.oked, o.oker)._LI();
            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.State);
        }, false, 4);
    }

    [OrglyAuthorize(0, User.ROL_OPN, ulevel: 2)]
    [Ui(tip: "修改产源设施", icon: "pencil"), Tool(ButtonShow, status: 3)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var sup = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Prod.Empty).T(" FROM prods_vw WHERE id = @1");
            var o = dc.QueryTop<Prod>(p => p.Set(id));
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();

                h.LI_().TEXT("产品源名称", nameof(o.name), o.name, min: 2, max: 12)._LI();
                h.LI_().SELECT("类别", nameof(o.typ), o.typ, Prod.Typs, required: true)._LI();
                h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().NUMBER("经度", nameof(o.x), o.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(o.y), o.y, min: -90.000, max: 90.000)._LI();
                h.LI_().SELECT("等级", nameof(o.rank), o.rank, Prod.Ranks, required: true)._LI();
                h.LI_().TEXTAREA("说明", nameof(o.remark), o.remark, max: 100)._LI();
                h.LI_().TEXTAREA("规格参数", nameof(o.specs), o.specs, max: 100)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, new Prod
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE prods ")._SET_(Prod.Empty, msk).T(" WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(id).Set(sup.id);
            });

            wc.GivePane(200); // close dialog
        }
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "图标", icon: "github-alt"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image"), Tool(ButtonCrop, status: 3, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "资料", icon: "album"), Tool(ButtonCrop, status: 3, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, "m" + sub, false, 6);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("上线", "上线投入使用", icon: "cloud-upload"), Tool(ButtonConfirm, status: 3)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE prods SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("下线", "下线以便修改", icon: "cloud-download"), Tool(ButtonConfirm, status: STU_OKED)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE prods SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "确认删除或者作废此产品？", icon: "trash"), Tool(ButtonConfirm, status: 3)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE prods SET status = 0 WHERE id = @1 AND orgid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.Give(204);
    }
}