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

        const short msk = 255 | MSK_EXTRA;

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty, msk).T(" FROM items_vw WHERE id = @1 AND rtlid = @2");
        var m = await dc.QueryTopAsync<Item>(p => p.Set(id).Set(org.id), msk);

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("商品名", m.name)._LI();
            h.LI_().FIELD("简介", string.IsNullOrEmpty(m.tip) ? "无" : m.tip)._LI();
            h.LI_().FIELD("单位", m.unit).FIELD2("每件含", m.unitx, m.unit)._LI();
            h.LI_().FIELD("单价", m.price, money: true).FIELD("大客户降价", m.off, money: true)._LI();
            h.LI_().FIELD("起订件数", m.minx)._LI();
            h.LI_().FIELD2("库存量", m.stock, m.StockX).FIELD2("未用量", m.avail, m.AvailX, "（")._LI();

            if (m.creator != null) h.LI_().FIELD2("创编", m.creator, m.created)._LI();
            if (m.adapter != null) h.LI_().FIELD2("修改", m.adapter, m.adapted)._LI();
            if (m.oker != null) h.LI_().FIELD2("上线", m.oker, m.oked)._LI();

            h._UL();

            h.TABLE(m.ops, o =>
            {
                h.TD_().T(o.dt, date: 2, time: 1)._TD();
                h.TD_("uk-text-right").T(o.tip)._TD();
                h.TD(o.qty, right: true);
                h.TD(o.avail, right: true);
                h.TD(o.by);
            }, caption: "库存操作记录", reverse: true);

            h.TOOLBAR(bottom: true, status: m.Status, state: m.State);
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

        wc.GivePane(200, h =>
        {
            if (o.lotid > 0)
            {
                var lot = GrabRow<int, Lot>(o.lotid);
                var sup = GrabRow<int, Org>(lot.supid);
                var fab = lot.fabid > 0 ? GrabSet<int, int, Fab>(lot.supid)[lot.fabid] : null;
                LotVarWork.LotShow(h, lot, sup, fab, false);
            }
            else
            {
                h.ARTICLE_("uk-card uk-card-primary");
                if (o.pic)
                {
                    h.PIC("/item/", o.id, "/pic");
                }
                h.H4("商品信息", "uk-card-header");

                h.SECTION_("uk-card-body");
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("商品名", o.name)._LI();
                h.LI_().FIELD("简介", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
                h._UL();
                h._SECTION();

                h._ARTICLE();
            }
        }, true, 900);
    }

    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), true, 3600);
    }

    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), true, 3600);
    }
}

public class RtllyItemVarWork : ItemVarWork
{
    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改商品信息", icon: "pencil"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
    public async Task edit(WebContext wc)
    {
        int itemid = wc[0];
        var rtl = wc[-2].As<Org>();
        var prin = (User)wc.Principal;
        var cats = Grab<short, Cat>();

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE id = @1");
            var o = await dc.QueryTopAsync<Item>(p => p.Set(itemid));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("商品信息");

                h.LI_().TEXT("商品名", nameof(o.name), o.name, max: 12).SELECT("类别", nameof(o.catid), o.catid, cats, required: true)._LI();
                h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("每件含", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();
                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("大客户降价", nameof(o.off), o.off, min: 0.00M, max: 9999.99M)._LI();
                h.LI_().NUMBER("起订件数", nameof(o.minx), o.minx)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, new Item
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE items ")._SET_(Item.Empty, msk).T(" WHERE id = @1 AND rtlid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(itemid).Set(rtl.id);
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
    [Ui(tip: "照片", icon: "image"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("库存", icon: "database"), Tool(ButtonShow, status: 7)]
    public async Task stock(WebContext wc)
    {
        int itemid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        short optyp = 0;
        short qty = 0;
        string tip = null;

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("库存操作");
                h.LI_().SELECT("操作类型", nameof(optyp), optyp, StockOp.Typs, required: true)._LI();
                h.LI_().NUMBER("数量", nameof(qty), qty, money: false)._LI();
                h.LI_().TEXT("摘要", nameof(tip), tip, max: 20, datalst: StockOp.Tips)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(stock))._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            optyp = f[nameof(optyp)];
            qty = f[nameof(qty)];
            tip = f[nameof(tip)];

            // update
            using var dc = NewDbContext();

            var now = DateTime.Now;
            if (optyp == StockOp.TYP_ADD)
            {
                dc.Sql("UPDATE items SET ops = (CASE WHEN ops[16] IS NULL THEN ops ELSE ops[2:] END) || ROW(@1, @2, @3, (avail + @3), @4)::StockOp, avail = avail + @3, stock = stock + @3 WHERE id = @5 AND rtlid = @6");
                await dc.ExecuteAsync(p => p.Set(now).Set(tip).Set(qty).Set(prin.name).Set(itemid).Set(org.id));
            }
            else // TYP_SUBSTRACT
            {
                dc.Sql("UPDATE items SET ops = (CASE WHEN ops[16] IS NULL THEN ops ELSE ops[2:] END) || ROW(@1, @2, @3, (avail - @3), @4)::StockOp, avail = avail - @3, stock = stock - @3 WHERE id = @5 AND rtlid = @6");
                await dc.ExecuteAsync(p => p.Set(now).Set(tip).Set(qty).Set(prin.name).Set(itemid).Set(org.id));
            }

            wc.GivePane(200); // close dialog
        }
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("上线", "上线投入使用", icon: "cloud-upload"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED, state: Item.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND rtlid = @4");
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
        dc.Sql("UPDATE items SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND rtlid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "确认删除或者作废？", icon: "trash"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
    public async Task @void(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 0 WHERE id = @1 AND rtlid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.Give(204);
    }
}