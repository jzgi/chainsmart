using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;
using static ChainSmart.MainUtility;

namespace ChainSmart;

public abstract class ItemVarWork : WebWork
{
    internal static readonly string[] SubnavActs = { string.Empty, nameof(bat) };

    [Ui("商品信息")]
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
            h.SUBNAV(SubnavActs);

            var cats = Grab<short, Cat>();
            var src = o.srcid > 0 ? GrabTwin<int, Org>(o.srcid) : null;

            h.H4("基本", css: "uk-padding");
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("名称", o.name).FIELD("品类", o.cat, cats)._LI();
            h.LI_().FIELD("简介语", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
            h.LI_().FIELD("单位", o.unit).FIELD("附注", o.unitip)._LI();
            h.LI_().FIELD2("整件", o.unitx, o.unit)._LI();
            h.LI_().FIELD("产源", src?.name ?? "无")._LI();
            h._UL();

            h.H4("货架", css: "uk-padding");
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("单价", o.price, money: true)._LI();
            h.LI_().FIELD("大客户减", o.off, money: true).FIELD("全减", o.promo)._LI();
            h.LI_().FIELD2("起订量", o.min, o.unit).FIELD2("限订量", o.max, o.unit)._LI();
            h.LI_().FIELD2("库存", o.stock, o.unit)._LI();
            h._UL();

            h.H4("状态", css: "uk-padding");
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("状态", o.status, Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "上线", o.oker, o.oked, sep: "<br>")._LI();
            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }

    [Ui("货管", status: 8)]
    public abstract Task bat(WebContext wc);

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
            if (o.pic)
            {
                h.IMG("/item/", o.id, "/pic");
            }

            var cats = Grab<short, Cat>();

            h.ARTICLE_("uk-card uk-card-primary uk-margin-remove");
            h.UL_("uk-card-body uk-list uk-list-divider");
            h.LI_().FIELD("商品名", o.name).FIELD("分类", o.cat, cats)._LI();
            if (!string.IsNullOrEmpty(o.tip))
            {
                h.LI_().FIELD("简介语", o.tip)._LI();
            }
            h.LI_().FIELD_("溯源", css: "uk-col");
            if (o.tag > 0)
            {
                var tag = Grab<short, Tag>()?[o.tag];
                h.MARK(tag?.name).Q(tag?.tip);
            }
            else
            {
                h.T("无");
            }
            h._FIELD()._LI();
            h.LI_().FIELD_("特誉", css: "uk-col");
            if (o.sym > 0)
            {
                var sym = Grab<short, Sym>()?[o.sym];
                h.MARK(sym?.name).Q(sym?.tip);
            }
            else
            {
                h.T("无");
            }
            h._FIELD()._LI();
            h.LI_().FIELD("单位", o.unit).FIELD("附注", o.unitip)._LI();
            h.LI_().FIELD("单价", o.price, money: true);
            if (o.off > 0)
            {
                h.FIELD("大客户减", o.off);
            }
            h._LI();

            if (o.srcid > 0)
            {
                var src = o.srcid > 0 ? GrabTwin<int, Org>(o.srcid) : null;
                if (src != null)
                {
                    h.LI_().FIELD("产源", src.name)._LI();
                    h.LI_().FIELD(string.Empty, src.tip)._LI();
                }
            }

            h._UL();

            h._ARTICLE();
        }, true, 900, css: "uk-light");
    }

    public override Task bat(WebContext wc)
    {
        throw new NotImplementedException();
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
    [MgtAuthorize(Org.TYP_SHL, User.ROL_OPN)]
    public override async Task bat(WebContext wc)
    {
        int itemid = wc[0];
        var org = wc[-2].As<Org>();

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE orgid = @1 AND itemid = @2 ORDER BY id DESC LIMIT 10");
            var arr = await dc.QueryAsync<Bat>(p => p.Set(org.id).Set(itemid));

            wc.GivePane(200, h =>
            {
                h.SUBNAV(SubnavActs);

                if (arr == null)
                {
                    h.ALERT("尚无货管记录");
                }
                else
                {
                    h.LIST(arr, o =>
                    {
                        h.SPAN_("uk-width-1-2");
                        if (o.srcid > 0)
                        {
                            var src = GrabTwin<int, Org>(o.srcid);
                            h.T(src.name);
                        }
                        h._SPAN();
                        h.SPAN_("uk-width-1-4").T(Bat.Typs[o.typ]).SP().T(o.qty)._SPAN();
                        h.SPAN_("uk-width-1-4");
                        if (o.stock != 0)
                        {
                            h.T(o.stock);
                        }
                        h._SPAN();
                    });
                }

                h.TOOLBAR(bottom: true, status: 8, state: org.ToState());
            }, false, 6);
        }
    }

    [MgtAuthorize(Org.TYP_SHL, User.ROL_OPN)]
    [Ui(tip: "修改商品信息", icon: "pencil", status: 1 | 2 | 4), Tool(ButtonShow)]
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
                var cats = Grab<short, Cat>();

                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().TEXT("名称", nameof(o.name), o.name, max: 12).SELECT("类型", nameof(o.cat), o.cat, cats)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("单位", nameof(o.unit), o.unit, Unit.Typs).TEXT("附注", nameof(o.unitip), o.unitip, max: 6)._LI();
                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M)._LI();
                h.LI_().NUMBER("优惠额", nameof(o.off), o.off, min: 0.00M, max: 999.99M).CHECKBOX("无差别优惠", nameof(o.promo), o.promo)._LI();
                h.LI_().NUMBER("起订量", nameof(o.min), o.min, min: 1, max: o.stock).NUMBER("限订量", nameof(o.max), o.max, min: MAX)._LI();
                h.LI_().NUMBER("整售量", nameof(o.unitx), o.unitx, min: 1, money: false, onchange: $"this.form.min.value = this.value; this.form.max.value = this.value * {MAX}; ")._LI();

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


    [MgtAuthorize(Org.TYP_SHL, User.ROL_OPN)]
    [Ui(tip: "图标", icon: "github-alt", status: 1 | 2), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [MgtAuthorize(Org.TYP_SHL, User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image", status: 1 | 2), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [MgtAuthorize(Org.TYP_SHL, User.ROL_OPN)]
    [Ui("上线", "上线投入使用", status: 1 | 2), Tool(ButtonConfirm, state: Item.STA_OKABLE)]
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

    [MgtAuthorize(Org.TYP_SHL, User.ROL_OPN)]
    [Ui("下线", "下线停用或调整", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [MgtAuthorize(Org.TYP_SHL, User.ROL_OPN)]
    [Ui(tip: "确认作废该商品", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 0, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.Give(204);
    }

    [MgtAuthorize(Org.TYP_SHL, User.ROL_OPN)]
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

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui("标志", "给商品设定标志", status: 1 | 2 | 4), Tool(ButtonShow)]
    public async Task prove(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        short tag = 0;
        short sym = 0;

        if (wc.IsGet)
        {
            var tags = Grab<short, Tag>();

            var syms = Grab<short, Sym>();

            using var dc = NewDbContext();
            await dc.QueryTopAsync("SELECT tag, sym FROM items_vw WHERE id = @1", p => p.Set(id));
            dc.Let(out tag);
            dc.Let(out sym);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);
                h.LI_().SELECT("溯源", nameof(tag), tag, tags)._LI();
                h.LI_().SELECT("特誉", nameof(sym), sym, syms)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(prove))._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            tag = f[nameof(tag)];
            sym = f[nameof(sym)];

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE items SET tag = @1, sym = @2, proved = @3, prover = @4 WHERE id = @5 AND orgid = @6");
            await dc.ExecuteAsync(p => p.Set(tag).Set(sym).Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

            wc.GivePane(200); // close dialog
        }
    }


    [MgtAuthorize(Org.TYP_SHL, User.ROL_OPN)]
    [Ui(tip: "简单加数", icon: "plus", status: 8), Tool(ButtonShow)]
    public async Task add(WebContext wc)
    {
        int itemid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;
        var now = DateTime.Now;

        var o = new Bat
        {
            typ = Bat.TYP_ADD,
            orgid = org.id,
            itemid = itemid,
            created = now,
            creator = prin.name,
            oked = now,
            oker = prin.name,
            status = STU_OKED,
        };

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_(css: "uk-list uk-list-divider").FIELDSUL_(wc.Action.Tip);
                h.LI_().NUMBER("数量", nameof(o.qty), o.qty, min: 1, max: 99999)._LI();
                h.LI_().TEXT("备注", nameof(o.tip), o.tip, max: 20)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(add))._FORM();
            }, false, 6);
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT | MSK_STATUS;

            await wc.ReadObjectAsync(msk, instance: o);

            using var dc = NewDbContext();
            o.name = (string)await dc.ScalarAsync("SELECT name FROM items_vw WHERE id = @1", p => p.Set(itemid));
            dc.Sql("INSERT INTO bats ").colset(Bat.Empty, msk)._VALUES_(Bat.Empty, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_SHL, User.ROL_OPN)]
    [Ui("产源到货", tip: "产源到货加数", icon: "plus", status: 8), Tool(ButtonShow)]
    public async Task addsrc(WebContext wc, int cmd)
    {
        int itemid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;
        var now = DateTime.Now;

        var o = new Bat
        {
            typ = Bat.TYP_SRC,
            orgid = org.id,
            itemid = itemid,
            created = now,
            creator = prin.name,
            oked = now,
            oker = prin.name,
            status = STU_OKED,
        };

        if (wc.IsGet)
        {
            var tags = Grab<short, Tag>();

            o.tag = wc.Query[nameof(o.tag)];
            o.qty = wc.Query[nameof(o.qty)];
            o.nend = wc.Query[nameof(o.nend)];

            string name = null;

            if (cmd == 1)
            {
                using var dc = NewDbContext();

                o.nstart = o.nend - o.qty + 1;
                await dc.QueryTopAsync("SELECT name, orgid FROM codes WHERE tag = @1 AND @2 BETWEEN nstart AND nend", p => p.Set(o.tag).Set(o.nstart).Set(o.nend));
                dc.Let(out name);
                dc.Let(out o.srcid);
            }
            wc.GivePane(200, h =>
            {
                h.FORM_(css: "uk-list uk-list-divider").FIELDSUL_(wc.Action.Tip);
                h.LI_().NUMBER("数量", nameof(o.qty), o.qty, min: 0, max: 999999)._LI();
                h.LI_().SELECT("溯源标志", nameof(o.tag), o.tag, tags, required: true)._LI();
                h.LI_().NUMBER("截止号", nameof(o.nend), o.nend, min: 0, max: 99999999).BUTTON("查找", nameof(addsrc), subscript: 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();

                if (cmd == 1)
                {
                    if (name != null)
                    {
                        h.LI_().TEXT("产源", nameof(name), name, @readonly: true).HIDDEN(nameof(o.srcid), o.srcid)._LI();
                        h.LI_().TEXT("备注", nameof(o.tip), o.tip, max: 20)._LI();
                    }
                    else
                    {
                        h.LI_().FIELD(string.Empty, "没有发放，请确认标志类型以及号码范围")._LI();
                    }
                }
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(add), disabled: cmd == 0)._FORM();
            }, false, 6);
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT | MSK_STATUS;

            await wc.ReadObjectAsync(msk, instance: o);
            o.nstart = o.nend - o.qty + 1;

            using var dc = NewDbContext();

            o.name = (string)await dc.ScalarAsync("SELECT name FROM items_vw WHERE id = @1", p => p.Set(itemid));

            dc.Sql("INSERT INTO bats ").colset(Bat.Empty, msk)._VALUES_(Bat.Empty, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_SHP, User.ROL_OPN)]
    [Ui("云仓到货", tip: "云仓到货加数", icon: "plus", status: 8), Tool(ButtonShow)]
    public async Task addpur(WebContext wc)
    {
        int itemid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        short optyp = 0;
        int qty = 0;
        string tip = null;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE orgid = @1 AND itemid = @2 ORDER BY id DESC LIMIT 10");
            var arr = await dc.QueryAsync<Bat>(p => p.Set(org.id).Set(itemid));

            wc.GivePane(200, h =>
            {
                if (arr == null)
                {
                    h.ALERT("尚无货管单");
                    return;
                }

                h.TABLE(arr, o => { h.TD(o.name).TD(Bat.Typs[o.typ]); });
            }, false, 6);
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            optyp = f[nameof(optyp)];
            qty = f[nameof(qty)];

            // if (!StockOp.IsAddOp(optyp))
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

    [MgtAuthorize(Org.TYP_SHL, User.ROL_OPN)]
    [Ui(tip: "简单减数", icon: "minus", status: 8), Tool(ButtonShow)]
    public async Task subtr(WebContext wc)
    {
        int itemid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;
        var now = DateTime.Now;

        var o = new Bat
        {
            typ = Bat.TYP_SUB,
            orgid = org.id,
            itemid = itemid,
            created = now,
            creator = prin.name,
            oked = now,
            oker = prin.name,
            status = STU_OKED,
        };

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_(css: "uk-list uk-list-divider").FIELDSUL_(wc.Action.Tip);
                h.LI_().NUMBER("数量", nameof(o.qty), o.qty, min: 1, max: 99999)._LI();
                h.LI_().TEXT("备注", nameof(o.tip), o.tip, max: 20)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(subtr))._FORM();
            }, false, 6);
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT | MSK_STATUS;
            await wc.ReadObjectAsync(msk, instance: o);

            // update db
            using var dc = NewDbContext();

            o.name = (string)await dc.ScalarAsync("SELECT name FROM items_vw WHERE id = @1", p => p.Set(itemid));

            dc.Sql("INSERT INTO bats ").colset(Bat.Empty, msk)._VALUES_(Bat.Empty, msk);
            await dc.ExecuteAsync(p => { o.Write(p, msk); });

            wc.GivePane(200); // close dialog
        }
    }
}

public class SuplyItemVarWork : ItemVarWork
{
    [MgtAuthorize(Org.TYP_SUP, User.ROL_OPN)]
    public override async Task bat(WebContext wc)
    {
        int itemid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        short optyp = 0;
        int qty = 0;
        string tip = null;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE orgid = @1 AND itemid = @2 ORDER BY id DESC LIMIT 10");
            var arr = await dc.QueryAsync<Bat>(p => p.Set(org.id).Set(itemid));

            wc.GivePane(200, h =>
            {
                h.SUBNAV(SubnavActs);

                if (arr == null)
                {
                    h.ALERT("尚无货管单");
                    return;
                }

                h.TABLE(arr, o => { h.TD(o.name).TD(Bat.Typs[o.typ]); });

                h.TOOLBAR(bottom: true, status: 0x100);
            }, false, 6);
        }
    }


    [MgtAuthorize(Org.TYP_WHL_, User.ROL_OPN)]
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

    [MgtAuthorize(Org.TYP_WHL_, User.ROL_OPN)]
    [Ui(tip: "图标", icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [MgtAuthorize(Org.TYP_WHL_, User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [MgtAuthorize(Org.TYP_WHL_, User.ROL_OPN)]
    [Ui(tip: "资料", icon: "album", status: 3), Tool(ButtonCrop, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, nameof(m) + sub, false, 6);
    }

    [MgtAuthorize(Org.TYP_WHL_, User.ROL_MGT)]
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

    [MgtAuthorize(Org.TYP_WHL_, User.ROL_OPN)]
    [Ui("下线", "下线停用或调整", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE items SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [MgtAuthorize(Org.TYP_WHL_, User.ROL_OPN)]
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

    [MgtAuthorize(Org.TYP_WHL_, User.ROL_MGT)]
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


    [MgtAuthorize(Org.TYP_SUP, User.ROL_MGT)]
    [Ui(icon: "plus", status: 0x100), Tool(ButtonShow)]
    public async Task shelf(WebContext wc)
    {
        int itemid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        short optyp = 0;
        int qty = 0;
        string tip = null;

        if (wc.IsGet)
        {
            wc.GivePane(200, h => { h.TOOLBAR(bottom: true); }, false, 6);
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            optyp = f[nameof(optyp)];
            qty = f[nameof(qty)];

            // if (!StockOp.IsAddOp(optyp))
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

    [MgtAuthorize(Org.TYP_SUP, User.ROL_MGT)]
    [Ui("产源到货", icon: "plus", status: 0x100), Tool(ButtonShow)]
    public async Task src(WebContext wc)
    {
        int itemid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        short optyp = 0;
        int qty = 0;
        string tip = null;

        if (wc.IsGet)
        {
            wc.GivePane(200, h => { h.TOOLBAR(bottom: true); }, false, 6);
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            optyp = f[nameof(optyp)];
            qty = f[nameof(qty)];

            // if (!StockOp.IsAddOp(optyp))
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

    [MgtAuthorize(Org.TYP_SUP, User.ROL_MGT)]
    [Ui(icon: "minus", status: 0x100), Tool(ButtonShow)]
    public async Task dec(WebContext wc)
    {
    }
}