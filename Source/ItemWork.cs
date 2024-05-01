using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.ToolAttribute;

namespace ChainSmart;

public abstract class ItemWork<V> : WebWork where V : ItemVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IEnumerable<Item> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            if (o.icon)
            {
                h.PIC(MainApp.WwwUrl, "/item/", o.id, "/icon", css: "uk-width-1-5");
            }
            else
            {
                h.PIC("/void.webp", css: "uk-width-1-5");
            }

            h.ASIDE_();
            h.HEADER_().H4(o.name);
            if (o.unitx > 1)
            {
                h.SP().SMALL_().T(o.unitx).T(o.unit).T("为整")._SMALL();
            }

            h.SPAN(Statuses[o.status], "uk-badge");
            h._HEADER();

            h.Q(o.tip, "uk-width-expand");
            h.FOOTER_().SPAN_().CNY(o.price)._SPAN();
            // h.VAR(nameof(ShplyItemVarWork.bat), o.id, caption: o.id.ToString(), css: "uk-margin-auto-left");
            h._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

public class PublyItemWork : ItemWork<PublyItemVarWork>
{
}

[MgtAuthorize(Org.TYP_SHP)]
[Ui("商品")]
public class ShplyItemWork : ItemWork<ShplyItemVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND status = 4 ORDER BY oked DESC");
        var arr = await dc.QueryAsync<Item>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无上线商品");
                return;
            }

            MainGrid(h, arr);
        }, false, 4);
    }

    [Ui(tip: "已下线", icon: "cloud-download", status: 2), Tool(Anchor)]
    public async Task down(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND status BETWEEN 1 AND 2 ORDER BY coalesce(adapted,created) DESC");
        var arr = await dc.QueryAsync<Item>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无下线商品");
                return;
            }

            MainGrid(h, arr);
        }, false, 4);
    }

    [Ui(tip: "已作废", icon: "trash", status: 4), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND status = 0 ORDER BY oked DESC");
        var arr = await dc.QueryAsync<Item>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无作废商品");
                return;
            }

            MainGrid(h, arr);
        }, false, 4);
    }

    [MgtAuthorize(Org.TYP_MKT_, User.ROL_MGT)]
    [Ui("新建", tip: "新建商品", icon: "plus", status: 2), Tool(ButtonOpen)]
    public async Task @new(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        const short MAX = 100;
        var o = new Item
        {
            typ = Item.TYP_MKT,
            orgid = org.id,
            created = DateTime.Now,
            creator = prin.name,
            cardinal = 1,
            unit = "斤",
            unitx = 1,
            min = 1,
            max = MAX
        };
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                var cats = Grab<short, Cat>();

                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().TEXT("名称", nameof(o.name), o.name, max: 12).SELECT("品类", nameof(o.cat), o.cat, cats)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("单位", nameof(o.unit), o.unit, Unit.Typs).TEXT("附注", nameof(o.unitip), o.unitip, max: 6)._LI();
                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M)._LI();
                h.LI_().NUMBER("大客户优惠", nameof(o.off), o.off, min: 0.00M, max: 999.99M).CHECKBOX("无差别优惠", nameof(o.promo), o.promo)._LI();
                h.LI_().NUMBER("起订量", nameof(o.min), o.min, min: 1, max: o.stock).NUMBER("限订量", nameof(o.max), o.max, min: MAX)._LI();
                h.LI_().NUMBER("整售量", nameof(o.unitx), o.unitx, min: 1, money: false, onchange: $"this.form.min.value = this.value; this.form.max.value = this.value * {MAX}; ")._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, o);

            // insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO items ").colset(Item.Empty, msk)._VALUES_(Item.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

[MgtAuthorize(Org.TYP_SUP)]
[Ui("商品")]
public class SuplyItemWork : ItemWork<SuplyItemVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND status = 4 ORDER BY oked DESC");
        var arr = await dc.QueryAsync<Item>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(state: org.ToState());

            if (arr == null)
            {
                h.ALERT("尚无上线商品");
                return;
            }

            MainGrid(h, arr);
        }, false);
    }

    [Ui(tip: "已下线", icon: "cloud-download", status: 2), Tool(Anchor)]
    public async Task down(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND status BETWEEN 1 AND 2 ORDER BY COALESCE(adapted,created) DESC");
        var arr = await dc.QueryAsync<Item>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(state: org.ToState());

            if (arr == null)
            {
                h.ALERT("尚无下线商品");
                return;
            }

            MainGrid(h, arr);
        }, false);
    }

    [Ui(tip: "已作废", icon: "trash", status: 4), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND status = 0 ORDER BY oked DESC");
        var arr = await dc.QueryAsync<Item>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(state: org.ToState());

            if (arr == null)
            {
                h.ALERT("尚无作废商品");
                return;
            }

            MainGrid(h, arr);
        }, false);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui("新建", "新建商品信息", icon: "plus", status: 2), Tool(ButtonOpen)]
    public async Task @new(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var cats = Grab<short, Cat>();

        var o = new Item
        {
            typ = Item.TYP_SUP,
            created = DateTime.Now,
            creator = prin.name,
            status = STU_CREATED,
            orgid = org.id,
            unit = "斤",
            unitx = 1,
            off = 0,
            min = 1,
            max = 100,
        };

        if (wc.IsGet)
        {
            var srcs = GrabTwinArray<int, Org>(o.orgid);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 12)._LI();
                h.LI_().SELECT("品类", nameof(o.cat), o.cat, cats, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("产源", nameof(o.srcid), o.srcid, srcs, required: false)._LI();
                h.LI_().SELECT("单位", nameof(o.unit), o.unit, Unit.Typs).TEXT("单位附注", nameof(o.unitip), o.unitip, max: 8)._LI();
                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("优惠额", nameof(o.off), o.off, min: 0.00M, max: 999.99M)._LI();
                h.LI_().NUMBER("整件含量", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();
                h.LI_().NUMBER("起订件数", nameof(o.min), o.min, min: 0, max: o.stock).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: o.stock)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认");

                h._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            await wc.ReadObjectAsync(msk, instance: o);

            // db insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO items ").colset(Item.Empty, msk)._VALUES_(Item.Empty, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}