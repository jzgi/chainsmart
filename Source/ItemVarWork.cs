﻿using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

namespace ChainSmart;

public class ItemVarWork : WebWork
{
    public virtual async Task @default(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        const short msk = 255 | MSK_AUX;
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty, msk).T(" FROM items_vw WHERE id = @1 AND orgid = @2");
        var o = await dc.QueryTopAsync<Item>(p => p.Set(id).Set(org.id), msk);

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("商品名", o.name).FIELD("类型", o.typ, Item.Typs)._LI();
            h.LI_().FIELD("简介语", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
            h.LI_().FIELD("单位", o.unit).FIELD("附注", o.unitip)._LI();
            h.LI_().FIELD("单价", o.price, money: true).FIELD2("整售", o.unitx, o.unit)._LI();
            h.LI_().FIELD("VIP立减", o.off, money: true).FIELD("全民立减", o.promo)._LI();
            h.LI_().FIELD2("起订量", o.min, o.unit).FIELD2("限订量", o.max, o.unit)._LI();
            h.LI_().FIELD2("货架", o.stock, o.unit)._LI();

            h.LI_().FIELD("状态", o.status, Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "上线", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }

    protected async Task doimg(WebContext wc, string col, bool shared, int maxage)
    {
        int id = wc[0];
        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").T(col).T(" FROM items WHERE id = @1");
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
            dc.Sql("UPDATE items SET ").T(col).T(" = @1 WHERE id = @2");
            if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
            {
                wc.Give(200); // ok
            }
            else
                wc.Give(500); // internal server error
        }
    }
}

public class PubItemVarWork : ItemVarWork
{
    static readonly string
        SrcUrl = MainApp.WwwUrl + "/org/",
        ItemUrl = MainApp.WwwUrl + "/item/";

    public override async Task @default(WebContext wc)
    {
        int itemid = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE id = @1");
        var o = await dc.QueryTopAsync<Item>(p => p.Set(itemid));

        Item itm = null;
        if (o.srcid > 0)
        {
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM lots_vw WHERE id = @1");
            itm = await dc.QueryTopAsync<Item>(p => p.Set(o.srcid));
        }

        wc.GivePane(200, h =>
        {
            if (o.pic)
            {
                h.IMG("/item/", o.id, "/pic", css: "uk-card-body");
            }

            h.ARTICLE_("uk-card uk-card-primary");
            h.UL_("uk-card-body uk-list uk-list-divider");
            h.LI_().FIELD("商品名", o.name).FIELD("分类", o.typ, Item.Typs)._LI();
            if (!string.IsNullOrEmpty(o.tip))
            {
                h.LI_().FIELD("简介语", o.tip)._LI();
            }
            h.LI_().FIELD("单位", o.unit).FIELD("附注", o.unitip)._LI();
            h.LI_().FIELD("单价", o.price, money: true);
            if (o.off > 0)
            {
                h.FIELD("VIP立减", o.off);
            }
            h._LI();
            h._UL();

            h._ARTICLE();

            if (o.srcid > 0)
            {
                var src = itm?.srcid > 0 ? GrabTwin<int, Org>(itm.srcid) : null;

                h.ARTICLE_("uk-card uk-card-primary");
                h.H2("产品信息", "uk-card-header");
                h.SECTION_("uk-card-body");
                h.PIC(ItemUrl, itm.id, "/pic", css: "uk-width-1-1");
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品名", itm.name)._LI();

                h.LI_().FIELD("单位", itm.unit).FIELD("附注", itm.unitip, itm.unitip)._LI();
                h.LI_().FIELD2("整件", itm.unitx, itm.unit)._LI();
                h.LI_().FIELD("单价", itm.price, true).FIELD("优惠立减", itm.off, true)._LI();
                h.LI_().FIELD("起订件数", itm.min).FIELD("限订件数", itm.max)._LI();

                h._UL();

                if (src != null)
                {
                    h.UL_("uk-list uk-list-divider");
                    h.LI_().FIELD("产源设施", src.name)._LI();
                    h.LI_().FIELD(string.Empty, src.tip)._LI();
                    // h.LI_().FIELD("等级", src.rank, Src.Ranks)._LI();
                    h._UL();

                    if (src.tip != null)
                    {
                        h.ALERT_().T(src.tip)._ALERT();
                    }
                    if (src.pic)
                    {
                        h.PIC(SrcUrl, src.id, "/pic", css: "uk-width-1-1 uk-padding-bottom");
                    }
                    if (src.m1)
                    {
                        h.PIC(SrcUrl, src.id, "/m-1", css: "uk-width-1-1 uk-padding-bottom");
                    }
                    if (src.m2)
                    {
                        h.PIC(SrcUrl, src.id, "/m-2", css: "uk-width-1-1 uk-padding-bottom");
                    }
                    if (src.m3)
                    {
                        h.PIC(SrcUrl, src.id, "/m-3", css: "uk-width-1-1 uk-padding-bottom");
                    }
                    if (src.m4)
                    {
                        h.PIC(SrcUrl, src.id, "/m-4", css: "uk-width-1-1 uk-padding-bottom");
                    }
                }
                h._SECTION();
                h._ARTICLE();

                h.ARTICLE_("uk-card uk-card-primary");
                h.H2("批次检验", "uk-card-header");
                h.SECTION_("uk-card-body");

                if (itm.m1)
                {
                    h.PIC(ItemUrl, itm.id, "/m-1", css: "uk-width-1-1 uk-padding-bottom");
                }
                if (itm.m2)
                {
                    h.PIC(ItemUrl, itm.id, "/m-2", css: "uk-width-1-1 uk-padding-bottom");
                }
                if (itm.m3)
                {
                    h.PIC(ItemUrl, itm.id, "/m-3", css: "uk-width-1-1 uk-padding-bottom");
                }
                if (itm.m4)
                {
                    h.PIC(ItemUrl, itm.id, "/m-4", css: "uk-width-1-1 uk-padding-bottom");
                }

                h._SECTION();
                h._ARTICLE();
            }
        }, true, 900);
    }

    const int MAXAGE = 3600 * 6;

    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), true, MAXAGE);
    }

    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), true, MAXAGE);
    }
}

public class ShplyItemVarWork : ItemVarWork
{
    [MgtAuthorize(Org.TYP_MKT_, User.ROL_OPN)]
    [Ui(tip: "修改商品信息", icon: "pencil", status: 3), Tool(ButtonShow)]
    public async Task upd(WebContext wc)
    {
        int itemid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        const short MAX = 100;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE id = @1");
            var o = await dc.QueryTopAsync<Item>(p => p.Set(itemid));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("类型", nameof(o.typ), o.typ, Item.Typs, filter: (x, _) => x <= 2)._LI();
                h.LI_().TEXT("名称", nameof(o.name), o.name, max: 12)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("单位", nameof(o.unit), o.unit, Unit.Typs).TEXT("附注", nameof(o.unitip), o.unitip, max: 6)._LI();
                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("优惠额", nameof(o.off), o.off, min: 0.00M, max: 999.99M)._LI();
                h.LI_().NUMBER("整售", nameof(o.unitx), o.unitx, min: 1, money: false, onchange: $"this.form.min.value = this.value; this.form.max.value = this.value * {MAX}; ").CHECKBOX("全员优惠", nameof(o.promo), o.promo)._LI();
                h.LI_().NUMBER("起订量", nameof(o.min), o.min, min: 1, max: o.stock).NUMBER("限订量", nameof(o.max), o.max, min: MAX)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(upd))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            var m = await wc.ReadObjectAsync(msk, new Item
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE items ")._SET_(Item.Empty, msk).T(" WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(itemid).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_MKT_, User.ROL_OPN)]
    [Ui(tip: "图标", icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [MgtAuthorize(Org.TYP_MKT_, User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [MgtAuthorize(Org.TYP_MKT_, User.ROL_MGT)]
    [Ui("上线", "上线投入使用", status: 3), Tool(ButtonConfirm, state: Item.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [MgtAuthorize(Org.TYP_MKT_, User.ROL_OPN)]
    [Ui("下线", "下线停用或调整", status: STU_OKED), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [MgtAuthorize(Org.TYP_MKT_, User.ROL_OPN)]
    [Ui(tip: "确认删除或者作废", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 0, oked = @1, oker = @2 WHERE id = @1 AND orgid = @2");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.Give(204);
    }

    [MgtAuthorize(Org.TYP_MKT_, User.ROL_MGT)]
    [Ui(tip: "恢复此项删除的商品", icon: "reply", status: 0), Tool(ButtonConfirm)]
    public async Task restore(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        try
        {
            dc.Sql("UPDATE items SET status = CASE WHEN adapter IS NULL 1 ELSE 2 END WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));
        }
        catch (Exception)
        {
        }

        wc.Give(204); // no content
    }
}

public class SuplyItemVarWork : ItemVarWork
{
    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui(tip: "修改商品信息", icon: "pencil", status: 3), Tool(ButtonShow)]
    public async Task upd(WebContext wc)
    {
        int itemid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        const short MAX = 100;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE id = @1");
            var o = await dc.QueryTopAsync<Item>(p => p.Set(itemid));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("类型", nameof(o.typ), o.typ, Item.Typs, filter: (x, _) => x <= 2)._LI();
                h.LI_().TEXT("名称", nameof(o.name), o.name, max: 12)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("单位", nameof(o.unit), o.unit, Unit.Typs).TEXT("附注", nameof(o.unitip), o.unitip, max: 6)._LI();
                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("优惠额", nameof(o.off), o.off, min: 0.00M, max: 999.99M)._LI();
                h.LI_().NUMBER("整售", nameof(o.unitx), o.unitx, min: 1, money: false, onchange: $"this.form.min.value = this.value; this.form.max.value = this.value * {MAX}; ").CHECKBOX("全员优惠", nameof(o.promo), o.promo)._LI();
                h.LI_().NUMBER("起订量", nameof(o.min), o.min, min: 1, max: o.stock).NUMBER("限订量", nameof(o.max), o.max, min: MAX)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(upd))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            var m = await wc.ReadObjectAsync(msk, new Item
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE items ")._SET_(Item.Empty, msk).T(" WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(itemid).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui(tip: "图标", icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui(tip: "资料", icon: "album", status: 3), Tool(ButtonCrop, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, nameof(m) + sub, false, 6);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_MGT)]
    [Ui("上线", "上线投入使用", status: 3), Tool(ButtonConfirm, state: Item.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui("下线", "下线停用或调整", status: STU_OKED), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui(tip: "确认删除或者作废", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 0, oked = @1, oker = @2 WHERE id = @1 AND orgid = @2");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.Give(204);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_MGT)]
    [Ui(tip: "恢复此项删除的商品", icon: "reply", status: 0), Tool(ButtonConfirm)]
    public async Task restore(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        try
        {
            dc.Sql("UPDATE items SET status = CASE WHEN adapter IS NULL 1 ELSE 2 END WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));
        }
        catch (Exception)
        {
        }

        wc.Give(204); // no content
    }
}