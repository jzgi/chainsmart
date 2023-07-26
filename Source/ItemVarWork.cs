using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

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

            h.LI_().FIELD("商品名", o.name)._LI();
            h.LI_().FIELD("简介语", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
            h.LI_().FIELD("零售单位", o.unit);
            if (o.IsProduct) h.FIELD("单位含重", o.unitw, Unit.Weights);
            h._LI();
            h.LI_().FIELD("单价", o.price, money: true).FIELD2("整售", o.step, o.unit)._LI();
            h.LI_().FIELD("VIP立减", o.off, money: true).FIELD("全民立减", o.promo)._LI();
            h.LI_().FIELD2("起订量", o.min, o.unit).FIELD2("限订量", o.max, o.unit)._LI();
            h.LI_().FIELD2("货架", o.stock, o.unit)._LI();

            h.LI_().FIELD("状态", o.status, Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2(o.IsVoid ? "删除" : "修改", o.adapter, o.adapted, sep: "<br>").FIELD2("上线", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TABLE(o.ops, o =>
                {
                    h.TD_().T(o.dt, date: 2, time: 1)._TD();
                    h.TD_("uk-text-right").T(StockOp.Typs[o.typ])._TD();
                    h.TD(o.qty, right: true);
                    h.TD(o.stock, right: true);
                    h.TD(o.by);
                },
                thead: () => h.TH("日期").TH("类型").TH("发生", css: "uk-text-right").TH("数量", css: "uk-text-right").TH("操作"),
                reverse: true
            );

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

public class PublyItemVarWork : ItemVarWork
{
    public override async Task @default(WebContext wc)
    {
        int itemid = wc[0];

        using var dc = NewDbContext();

        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE id = @1");
        var o = await dc.QueryTopAsync<Item>(p => p.Set(itemid));

        Lot lot = null;
        if (o.lotid > 0)
        {
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1");
            lot = await dc.QueryTopAsync<Lot>(p => p.Set(o.lotid));
        }

        wc.GivePane(200, h =>
        {
            if (o.lotid > 0)
            {
                var org = GrabTwin<int, Org>(o.orgid);
                var src = lot?.srcid > 0 ? GrabTwin<int, Src>(lot.srcid) : null;

                LotVarWork.ShowLot(h, lot, org, src, false);
            }
            else
            {
                h.ARTICLE_("uk-card uk-card-primary");
                h.H2(o.name, css: "uk-card-header");
                // h.SECTION_("uk-card-body");
                if (o.pic)
                {
                    h.IMG("/item/", o.id, "/pic", css: "uk-card-body");
                }
                // h._SECTION();

                h.UL_("uk-card-body uk-list uk-list-divider");
                h.LI_().FIELD("商品名", o.name)._LI();
                h.LI_().FIELD("简介语", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
                h.LI_().FIELD("零售单位", o.unit);
                if (o.unitw > 0) h.FIELD("含重", o.unitw);
                h._LI();
                h.LI_().FIELD("单价", o.price, money: true);
                if (o.off > 0) h.FIELD("VIP立减", o.off);
                h._LI();
                h._UL();

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

public class RtllyItemVarWork : ItemVarWork
{
    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改商品信息", icon: "pencil", status: 3), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int itemid = wc[0];
        var rtl = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        const short MAX = 100;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE id = @1");
            var o = await dc.QueryTopAsync<Item>(p => p.Set(itemid));

            var prod = !rtl.IsService;

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("自建" + (prod ? "产品型商品" : "服务型商品"));

                h.LI_().TEXT(o.IsFromSupply ? "供应产品名" : "商品名", nameof(o.name), o.name, max: 12)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();

                h.LI_().SELECT("零售单位", nameof(o.unit), o.unit, Unit.Typs, showkey: true);
                if (prod)
                {
                    h.SELECT("单位含重", nameof(o.unitw), o.unitw, Unit.Weights);
                }
                h._LI();

                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("整售", nameof(o.step), o.step, min: 1, money: false, onchange: $"this.form.min.value = this.value; this.form.max.value = this.value * {MAX}; ")._LI();
                h.LI_().NUMBER("VIP立减", nameof(o.off), o.off, min: 0.00M, max: 999.99M).CHECKBOX("全民立减", nameof(o.promo), o.promo)._LI();
                h.LI_().NUMBER("起订量", nameof(o.min), o.min, min: 1, max: o.stock).NUMBER("限订量", nameof(o.max), o.max, min: MAX)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
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
                p.Set(itemid).Set(rtl.id);
            });

            wc.GivePane(200); // close dialog
        }
    }


    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "图标", icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("货架", status: 7), Tool(ButtonShow)]
    public async Task stock(WebContext wc)
    {
        int itemid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        short optyp = 0;
        int qty = 0;
        string tip = null;

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("货架数量");
                h.LI_().SELECT("操作", nameof(optyp), optyp, StockOp.Typs, required: true)._LI();
                h.LI_().NUMBER("数量", nameof(qty), qty, min: 1)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(stock))._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            optyp = f[nameof(optyp)];
            qty = f[nameof(qty)];

            if (!StockOp.IsAddOp(optyp))
            {
                qty = -qty;
            }

            // update db
            using var dc = NewDbContext();
            dc.Sql("UPDATE items SET ops = (CASE WHEN ops[12] IS NULL THEN ops ELSE ops[2:] END) || ROW(@1, @2, (stock + @2), @3, @4, NULL)::stockop, stock = stock + @2 WHERE id = @5 AND orgid = @6");
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(qty).Set(optyp).Set(prin.name).Set(itemid).Set(org.id));

            wc.GivePane(200); // close dialog
        }
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
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

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("下线", "下线以便修改", status: STU_OKED), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "确认删除或者作废", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 0 WHERE id = @1 AND orgid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.Give(204);
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui(tip: "恢复", icon: "arrow-left", status: 0), Tool(ButtonConfirm)]
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