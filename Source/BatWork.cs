using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;
using static ChainFX.Entity;

namespace ChainSmart;

public abstract class BatWork<V> : WebWork where V : WebWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Bat> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name);

            h.SPAN((Code.Typs[o.typ]), "uk-badge");
            h._HEADER();

            h.Q(o.nstart, "uk-width-expand");
            // h.FOOTER_().SPAN(o.nend).SPAN_("uk-margin-auto-left").T(o.bal)._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

/// <summary>
/// To search for traceability.
/// </summary>
/// <code>/move-n/xxx</code>
public class PublyBatWork : BatWork<PublyBatVarWork>
{
    public async Task @default(WebContext wc, int code)
    {
        var tagtyp = (short)wc[0].Adscript;

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE tagtyp = @1 AND nend >= @2 AND nstart <= @2 ORDER BY typ, nend ASC LIMIT 1");
        var o = await dc.QueryTopAsync<Bat>(p => p.Set(tagtyp).Set(code));

        wc.GivePage(200, h =>
        {
            if (o == null)
            {
                h.ALERT("无效的溯源码");
                return;
            }

            h.TOPBARXL_();
            h.HEADER_("uk-width-expand uk-padding-small-left").H1(o.name, css: "h1")._HEADER();
            h.PIC("/item/", o.itemid, "/icon", circle: true, css: "uk-width-small");
            h._TOPBARXL();

            h.UL_("uk-list uk-list-divider");
            h.LI_().T("简介语").T(o.tip)._LI();


            var org = GrabTwin<int, Org>(o.orgid);
            h.LI_().T("商户").T(org.name)._LI();


            var src = GrabTwin<int, Org>(o.srcid);
            h.LI_().T("产源").T(src.name)._LI();

            if (o.hubid > 0)
            {
                var hub = GrabTwin<int, Org>(o.hubid);
                h.LI_().T("品控云仓").T(hub.name)._LI();
            }
            h._UL();
        }, true, 3600, title: "中惠农通产品溯源信息");
    }
}

[MgtAuthorize(Org.TYP_RTL_)]
[Ui("货管")]
public class RtllyBatWork : BatWork<RtllyBatVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE orgid = @1 AND status = 0 ORDER BY oked DESC limit 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Bat>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无货管单");
                return;
            }

            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 6);
    }

    [MgtAuthorize(Org.TYP_RTL, User.ROL_OPN)]
    [Ui("补仓", "新建补仓单", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task add(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        Org[] srcs = null;
        if (org.ties != null)
        {
            srcs = GrabTwinArray<int, Org>(0, filter: x => org.ties.Contains(x.id));
        }

        var o = new Bat
        {
            orgid = org.id,
            typ = Bat.TYP_ADD,
            created = DateTime.Now,
            creator = prin.name,
        };

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            await dc.QueryAsync("SELECT id, concat(name, '（剩 ', stock, ' ', unit, '）') FROM items WHERE orgid = @1 AND status > 0", p => p.Set(org.id));
            var map = dc.ToIntMap();


            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("操作类型", nameof(o.typ), o.typ, Bat.Typs, filter: (k, v) => k == 1)._LI();
                h.LI_().SELECT("商品", nameof(o.itemid), o.itemid, map)._LI();
                h.LI_().SELECT("产源", nameof(o.srcid), o.srcid, srcs)._LI();
                h.LI_().NUMBER("数量", nameof(o.qty), o.qty, min: 1, max: 9999)._LI();
                h.LI_().TEXTAREA("附加说明", nameof(o.tip), o.tip, max: 40)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(add))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, o);

            // insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO bats ").colset(Bat.Empty, msk)._VALUES_(Bat.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_RTL_, User.ROL_OPN)]
    [Ui("减仓", "新建减仓单", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task cut(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        var o = new Bat
        {
            orgid = org.id,
            created = DateTime.Now,
            creator = prin.name,
        };

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            await dc.QueryAsync("SELECT id, concat(name, '（剩 ', stock, ' ', unit, '）') FROM items WHERE orgid = @1 AND status > 0", p => p.Set(org.id));
            var map = dc.ToIntMap();

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("商品", nameof(o.itemid), o.itemid, map)._LI();
                h.LI_().SELECT("操作类型", nameof(o.typ), o.typ, Bat.Typs, filter: (k, v) => k > 1)._LI();
                h.LI_().TEXTAREA("附加说明", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().NUMBER("数量", nameof(o.qty), o.qty, min: 1, max: 9999)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(add))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, o);

            // insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO bats ").colset(Bat.Empty, msk)._VALUES_(Bat.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

[MgtAuthorize(Org.TYP_SUP_)]
[Ui("货管")]
public class SuplyBatWork : BatWork<SuplyBatVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE orgid = @1 AND status = 0 ORDER BY oked DESC limit 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Bat>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无货管单");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 6);
    }

    [MgtAuthorize(Org.TYP_SUP, User.ROL_OPN)]
    [Ui("补仓", "新建补仓单", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task add(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        Org[] srcs = null;
        if (org.ties != null)
        {
            srcs = GrabTwinArray<int, Org>(0, filter: x => org.ties.Contains(x.id));
        }
        var hubs = GrabTwinArray<int, Org>(0, filter: x => x.IsHub);

        var o = new Bat
        {
            orgid = org.id,
            typ = Bat.TYP_ADD,
            created = DateTime.Now,
            creator = prin.name,
        };

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            await dc.QueryAsync("SELECT id, concat(name, '（剩 ', stock, ' ', unit, '）') FROM items WHERE orgid = @1 AND status > 0", p => p.Set(org.id));
            var map = dc.ToIntMap();

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("操作类型", nameof(o.typ), o.typ, Bat.Typs, filter: (k, v) => k == 1)._LI();
                h.LI_().SELECT("商品", nameof(o.itemid), o.itemid, map)._LI();
                h.LI_().SELECT("产源", nameof(o.srcid), o.srcid, srcs)._LI();
                h.LI_().SELECT("品控云仓", nameof(o.hubid), o.hubid, hubs)._LI();
                h.LI_().NUMBER("数量", nameof(o.qty), o.qty, min: 1, max: 9999)._LI();
                h.LI_().TEXTAREA("附加说明", nameof(o.tip), o.tip, max: 40)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(add))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, o);

            // insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO bats ").colset(Bat.Empty, msk)._VALUES_(Bat.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_SUP, User.ROL_OPN)]
    [Ui("减仓", "新建减仓单", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task cut(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var hubs = GrabTwinArray<int, Org>(0, filter: x => x.IsHub);

        var o = new Bat
        {
            orgid = org.id,
            created = DateTime.Now,
            creator = prin.name,
        };

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            await dc.QueryAsync("SELECT id, concat(name, '（剩 ', stock, ' ', unit, '）') FROM items WHERE orgid = @1 AND status > 0", p => p.Set(org.id));
            var map = dc.ToIntMap();

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("操作类型", nameof(o.typ), o.typ, Bat.Typs, filter: (k, v) => k > 1)._LI();
                h.LI_().SELECT("商品", nameof(o.itemid), o.itemid, map)._LI();
                h.LI_().SELECT("品控云仓", nameof(o.hubid), o.hubid, hubs)._LI();
                h.LI_().NUMBER("数量", nameof(o.qty), o.qty, min: 1, max: 9999)._LI();
                h.LI_().TEXTAREA("附加说明", nameof(o.tip), o.tip, max: 40)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(add))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, o);

            // insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO bats ").colset(Bat.Empty, msk)._VALUES_(Bat.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

[MgtAuthorize(Org.TYP_HUB)]
[Ui("货流单")]
public class HublyBatWork : BatWork<HublyBatVarWork>
{
}