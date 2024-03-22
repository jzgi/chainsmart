using System;
using System.Data;
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
            h.LI_().FIELD("单价", o.price, money: true).FIELD2("整售", o.lotid, o.unit)._LI();
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

                h.ShowLot(o, src, false, false);
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
    [MgtAuthorize(Org.TYP_RTL_, User.ROL_OPN)]
    [Ui(tip: "调整商品信息", icon: "pencil", status: 3), Tool(ButtonShow)]
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

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().TEXT(o.IsImported ? "供应产品名" : "商品名", nameof(o.name), o.name, max: 12)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("单位", nameof(o.unit), o.unit, Unit.Typs).TEXT("附注", nameof(o.unitip), o.unitip, max: 6)._LI();
                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("整售", nameof(o.lotid), o.lotid, min: 1, money: false, onchange: $"this.form.min.value = this.value; this.form.max.value = this.value * {MAX}; ")._LI();
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


    [MgtAuthorize(Org.TYP_RTL_, User.ROL_OPN)]
    [Ui(tip: "图标", icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [MgtAuthorize(Org.TYP_RTL_, User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [MgtAuthorize(Org.TYP_RTL_, User.ROL_OPN)]
    [Ui("货架", tip: "货架数量操作", status: 7), Tool(ButtonShow)]
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
                h.FORM_().FIELDSUL_(wc.Action.Tip);
                h.LI_().SELECT("操作", nameof(optyp), optyp, Flow.Typs, required: true)._LI();
                h.LI_().NUMBER("数量", nameof(qty), qty, min: 1)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(stock))._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            optyp = f[nameof(optyp)];
            qty = f[nameof(qty)];

            // if (!ItemOp.IsAddOp(optyp))
            // {
            //     qty = -qty;
            // }

            // update db
            using var dc = NewDbContext();
            dc.Sql("UPDATE items SET ops = (CASE WHEN ops[12] IS NULL THEN ops ELSE ops[2:] END) || ROW(@1, @2, (stock + @2), @3, @4, NULL)::stockop, stock = stock + @2 WHERE id = @5 AND orgid = @6");
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(qty).Set(optyp).Set(prin.name).Set(itemid).Set(org.id));

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_RTL_, User.ROL_MGT)]
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

    [MgtAuthorize(Org.TYP_RTL_, User.ROL_OPN)]
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

    [MgtAuthorize(Org.TYP_RTL_, User.ROL_OPN)]
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

    [MgtAuthorize(Org.TYP_RTL_, User.ROL_MGT)]
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
    [Ui(tip: "调整产品批次", icon: "pencil", status: 3), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int lotid = wc[0];
        var org = wc[-2].As<Org>();
        var cats = Grab<short, Cat>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM lots_vw WHERE id = @1 AND orgid = @2");
            var o = await dc.QueryTopAsync<Item>(p => p.Set(lotid).Set(org.id));

            var srcs = GrabTwinArray<int, Org>(o.orgid);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().TEXT("产品名", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().SELECT("分类", nameof(o.cattyp), o.cattyp, cats, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("产源设施", nameof(o.srcid), o.srcid, srcs)._LI();
                h.LI_().SELECT("基本单位", nameof(o.unit), o.unit, Unit.Typs).TEXT("附注", nameof(o.unitip), o.unitip, max: 8)._LI();
                h.LI_().NUMBER("整件", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();

                h._FIELDSUL().FIELDSUL_("销售参数");


                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("优惠立减", nameof(o.off), o.off, min: 0.00M, max: 999.99M)._LI();
                // h.LI_().NUMBER("起订件数", nameof(o.min), o.min, min: 0, max: o.stock).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: o.stock)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            // populate 
            var o = await wc.ReadObjectAsync(msk, instance: new Item
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE lots ")._SET_(Item.Empty, msk).T(" WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p =>
            {
                o.Write(p, msk);
                p.Set(lotid).Set(org.id);
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

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui("质控", "溯源码以及质检材料", status: 3), Tool(ButtonShow)]
    public async Task tag(WebContext wc, int cmd)
    {
        int lotid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM lots_vw WHERE id = @1 AND orgid = @2");
            var o = dc.QueryTop<Item>(p => p.Set(lotid).Set(org.id));

            if (cmd == 0)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("溯源编号绑定");

                    h._FIELDSUL().FIELDSUL_("质控材料链接");

                    h.LI_().URL("合格证", nameof(o.link), o.link)._LI();

                    h._FIELDSUL().FIELDSUL_("打印溯源贴标");

                    h.LI_().LABEL(string.Empty).AGOTO_(nameof(tag), 1, parent: false, css: "uk-button uk-button-secondary").T("打印专属贴标")._A()._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(tag), subscript: cmd)._FORM();
                });
            }
            else // cmd = (page - 1)
            {
                const short NUM = 90;

                var src = o.srcid == 0 ? null : GrabTwin<int, Org>(o.srcid);

                wc.GivePane(200, h =>
                {
                    h.UL_(css: "uk-grid uk-child-width-1-6");

                    var today = DateTime.Today;
                    var idx = (cmd - 1) * NUM;
                    for (var i = 0; i < NUM; i++)
                    {
                        h.LI_("height-1-15");

                        h.HEADER_();
                        h.QRCODE(MainApp.WwwUrl + "/lot/" + o.id + "/", css: "uk-width-1-3");
                        h.ASIDE_().H6_().T(Application.Nodal.name)._H6().SMALL_().T(today, date: 3, time: 0)._SMALL()._ASIDE();
                        h._HEADER();

                        // h.H6_("uk-flex").T(lotid, digits: 8).T('-').T(idx + 1).SPAN(Src.Ranks[src?.rank ?? 0], "uk-margin-auto-left")._H6();

                        h._LI();

                        // if (++idx >= o.cap)
                        // {
                        //     break;
                        // }
                    }
                    h._UL();

                    // h.PAGINATION(idx < o.cap, begin: 2, print: true);
                });
            }
        }
        else // POST
        {
            if (cmd == 0)
            {
                var f = await wc.ReadAsync<Form>();
                int nstart = f[nameof(nstart)];
                int nend = f[nameof(nend)];
                string linka = f[nameof(linka)];
                string linkb = f[nameof(linkb)];

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE lots SET status = 2, adapted = @1, adapter = @2, nstart = @3, nend = @4, linka = @5, linkb = @6 WHERE id = @7");
                await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(nstart).Set(nend).Set(linka).Set(linkb).Set(lotid));
            }

            wc.GivePane(200); // close
        }
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_LOG)]
    [Ui("货架", "管理供应数量", status: 7), Tool(ButtonShow)]
    public async Task stock(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        short optyp = 0;
        int hubid = 0;
        int qtyx = 1;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            await dc.QueryTopAsync("SELECT ops FROM lots_vw WHERE id = @1", p => p.Set(id));
            dc.Let(out Flow[] ops);

            var arr = GrabTwinArray<int, Org>(0, filter: x => x.IsHub, sorter: (x, y) => y.id - x.id);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("货架操作");
                h.LI_().SELECT("操作", nameof(optyp), optyp, Flow.Typs, required: true)._LI();
                h.LI_().NUMBER("件数", nameof(qtyx), qtyx, min: 1)._LI();
                h.LI_().SELECT("云仓", nameof(hubid), hubid, arr)._LI();
                h._FIELDSUL();

                // h.TABLE(ops, o =>
                //     {
                //         h.TD_().T(o.dt, date: 2, time: 1)._TD();
                //         h.TD2(ItemOp.Typs[o.typ], o.qty, css: "uk-text-right");
                //         h.TD(o.stock, right: true);
                //         h.TD(o.by);
                //     },
                //     thead: () => h.TH("时间").TH("摘要").TH("云仓").TH("余量").TH("操作")
                // );

                h.BOTTOM_BUTTON("确认", nameof(stock))._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            optyp = f[nameof(optyp)];
            qtyx = f[nameof(qtyx)];
            hubid = f[nameof(hubid)];

            // if (!ItemOp.IsAddOp(optyp))
            // {
            //     qtyx = -qtyx;
            // }
            using var dc = NewDbContext(IsolationLevel.ReadUncommitted);

            await dc.QueryTopAsync("SELECT unitx FROM lots_vw WHERE id = @1", p => p.Set(id));
            dc.Let(out int unitx);
            int qty = qtyx * unitx;

            if (hubid > 0)
            {
                await dc.QueryTopAsync("INSERT INTO lotinvs VALUES (@1, @2, @3) ON CONFLICT (lotid, hubid) DO UPDATE SET stock = (lotinvs.stock + @3) RETURNING stock", p => p.Set(id).Set(hubid).Set(qty));
            }
            else
            {
                await dc.QueryTopAsync("UPDATE lots SET stock = stock + @1 WHERE id = @2 RETURNING stock", p => p.Set(qty).Set(id));
            }
            dc.Let(out int stock);

            dc.Sql("UPDATE lots SET ops = ops || ROW(@1, @2, @3, @4, @5, @6)::StockOp WHERE id = @7 AND orgid = @8");
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(qty).Set(stock).Set(optyp).Set(prin.name).Set(hubid).Set(id).Set(org.id));

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_MGT)]
    [Ui("上线", "上线投入使用", status: 3), Tool(ButtonConfirm, state: Item.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE lots SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.Give(200);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_MGT)]
    [Ui("下线", "下线停用或调整", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE lots SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2")._MEET_(wc);
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.Give(200);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_MGT)]
    [Ui(tip: "作废此产品批次", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task @void(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE lots SET status = 0, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4 AND status BETWEEN 1 AND 2");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.Give(200);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_MGT)]
    [Ui(tip: "恢复", icon: "reply", status: 0), Tool(ButtonConfirm)]
    public async Task unvoid(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE lots SET status = CASE WHEN adapter IS NULL THEN 1 ELSE 2 END, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2 AND status = 0");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.Give(204); // no content
    }
}