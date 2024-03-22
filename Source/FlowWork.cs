using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;
using static ChainFX.Entity;

namespace ChainSmart;

public abstract class FlowWork<V> : WebWork where V : WebWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Flow> arr)
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
/// To search for traceability tags.
/// </summary>
public class PublyFlowWork : FlowWork<PublyFlowVarWork>
{
    public async Task @default(WebContext wc, int num)
    {
        var tagtyp = (short)wc[0].Adscript;

        using var dc = NewDbContext();
        if (!await dc.QueryTopAsync("SELECT itemid, orgid, srcid, hubid FROM flows WHERE tagtyp = @1 AND nend >= @2 AND nstart <= @2 ORDER BY typ, nend ASC LIMIT 1", p => p.Set(tagtyp).Set(num)))
        {
            wc.GivePage(300, h => h.ALERT("此溯源码没有绑定产品"));
            return;
        }

        dc.Let(out int itemid);
        dc.Let(out int orgid);
        dc.Let(out int srcid);
        dc.Let(out int hubid);

        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE id = @1");
        var itm = await dc.QueryTopAsync<Item>(p => p.Set(itemid));

        if (itm == null)
        {
            wc.GivePage(200, h => { h.ALERT("无效的溯源产品批次"); });
            return;
        }

        Org src = null;
        if (itm.srcid > 0)
        {
            src = GrabTwin<int, Org>(itm.srcid);
        }

        wc.GivePage(200, h =>
        {
            h.TOPBARXL_();

            h.HEADER_("uk-width-expand uk-col uk-padding-small-left").H1(itm.name, css: "h1-lot")._HEADER();
            if (itm.icon)
            {
                h.PIC("/lot/", itm.id, "/icon", circle: true, css: "uk-width-small");
            }
            else
            {
                h.PIC("/void.webp", circle: true, css: "uk-width-small");
            }
            h._TOPBARXL();

            h.ShowLot(itm, src, false, true, num);

            h.FOOTER_("uk-col uk-flex-middle uk-padding-large");
            h.SPAN("金中关（北京）信息技术研究院", css: "uk-padding-small");
            h.SPAN("江西同其成科技有限公司", css: "uk-padding-small");
            h._FOOTER();
        }, true, 3600, title: "中惠农通产品溯源信息");
    }
}

[MgtAuthorize(Org.TYP_RTL_)]
[Ui("货管")]
public class RtllyFlowWork : FlowWork<RtllyFlowVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Flow.Empty).T(" FROM flows WHERE orgid = @1 AND status = 0 ORDER BY oked DESC limit 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Flow>(p => p.Set(org.id).Set(page));

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

        var o = new Flow
        {
            orgid = org.id,
            typ = Flow.TYP_ADD,
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

                h.LI_().SELECT("操作类型", nameof(o.typ), o.typ, Flow.Typs, filter: (k, v) => k == 1)._LI();
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
            dc.Sql("INSERT INTO flows ").colset(Flow.Empty, msk)._VALUES_(Flow.Empty, msk);
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

        var o = new Flow
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
                h.LI_().SELECT("操作类型", nameof(o.typ), o.typ, Flow.Typs, filter: (k, v) => k > 1)._LI();
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
            dc.Sql("INSERT INTO flows ").colset(Flow.Empty, msk)._VALUES_(Flow.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

[MgtAuthorize(Org.TYP_SUP_)]
[Ui("货管")]
public class SuplyFlowWork : FlowWork<SuplyFlowVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Flow.Empty).T(" FROM flows WHERE orgid = @1 AND status = 0 ORDER BY oked DESC limit 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Flow>(p => p.Set(org.id).Set(page));

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

        var o = new Flow
        {
            orgid = org.id,
            typ = Flow.TYP_ADD,
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

                h.LI_().SELECT("操作类型", nameof(o.typ), o.typ, Flow.Typs, filter: (k, v) => k == 1)._LI();
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
            dc.Sql("INSERT INTO flows ").colset(Flow.Empty, msk)._VALUES_(Flow.Empty, msk);
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

        var o = new Flow
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

                h.LI_().SELECT("操作类型", nameof(o.typ), o.typ, Flow.Typs, filter: (k, v) => k > 1)._LI();
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
            dc.Sql("INSERT INTO flows ").colset(Flow.Empty, msk)._VALUES_(Flow.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

[MgtAuthorize(Org.TYP_HUB)]
[Ui("货流单")]
public class HublyFlowWork : FlowWork<HublyFlowVarWork>
{
}