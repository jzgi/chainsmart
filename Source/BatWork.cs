using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;
using static ChainFX.Entity;
using static ChainSmart.MainUtility;

namespace ChainSmart;

public abstract class BatWork<V> : WebWork where V : WebWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IEnumerable<Bat> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC(ItemUrl, o.itemid, "/icon", css: "uk-width-tiny");

            h.ASIDE_();
            h.HEADER_().H4(o.name);

            h.SPAN((Bat.Typs[o.typ]), "uk-badge");
            h._HEADER();

            h.Q(o.codeid, "uk-width-expand");
            // h.FOOTER_().SPAN(o.nend).SPAN_("uk-margin-auto-left").T(o.bal)._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

public class PublyBatWork : CodeWork<PublyBatVarWork>
{
    public async Task @default(WebContext wc, int codev)
    {
        var tag = (short)wc[0].Adscript;

        // try to seek id of the batch that includes the given code value
        using var dc = NewDbContext();
        dc.Sql("SELECT id FROM bats WHERE tag = @1 AND @2 BETWEEN nstart AND nend LIMIT 1");
        var batid = (int)await dc.ScalarAsync(p => p.Set(tag).Set(codev));

        wc.GiveRedirect("../bat/" + batid + "/");
    }
}

[MgtAuthorize(Org.TYP_SHP)]
[Ui("货管")]
public class ShplyBatWork : BatWork<ShplyBatVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE orgid = @1 ORDER BY id DESC LIMIT 20 OFFSET @2 * 20");
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

    [MgtAuthorize(Org.TYP_SHP, User.ROL_OPN)]
    [Ui("补仓", "新建补仓单", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task inc(WebContext wc, int typ)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        if (typ == 0)
        {
            wc.Subscript = typ = 1;
        }

        var o = new Bat
        {
            orgid = org.id,
            typ = (short)typ,
            created = DateTime.Now,
            creator = prin.name,
        };

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            await dc.QueryAsync("SELECT id, concat(name, '（剩 ', stock, ' ', unit, '）') FROM items_vw WHERE orgid = @1 AND status > 0", p => p.Set(org.id));
            var items = dc.ToIntMap();

            Map<int, string> srcmap = null;
            if (typ == Bat.TYP_PUR)
            {
                await dc.QueryAsync("SELECT srcid, string_agg(name,'，') FROM purs WHERE orgid = @1 AND status >= 4 AND oked > @2 GROUP BY srcid", p => p.Set(org.id).Set(DateTime.Now.AddMonths(-1)));
                srcmap = dc.ToIntMap();
            }

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("操作类型", nameof(o.typ), o.typ, Bat.Typs, filter: (k, _) => k <= Bat.TYP_PUR, onchange: "return goto('inc-' + this.value, event);")._LI();
                h.LI_().SELECT("商品", nameof(o.itemid), o.itemid, items, required: true)._LI();
                if (typ == Bat.TYP_SRC)
                {
                }
                else if (typ == Bat.TYP_PUR)
                {
                    h.LI_().SELECT("产源", nameof(o.srcid), o.srcid, srcmap, required: true)._LI();
                }
                h.LI_().NUMBER("数量", nameof(o.qty), o.qty, min: 1, max: 9999)._LI();
                h.LI_().TEXTAREA("附加说明", nameof(o.tip), o.tip, max: 40)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(inc))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;

            // populate 
            await wc.ReadObjectAsync(msk, o);

            using var dc = NewDbContext();
            // retrive item name
            o.name = (string)await dc.ScalarAsync("SELECT name FROM items_vw WHERE id = @1", p => p.Set(o.itemid));
            // insert
            dc.Sql("INSERT INTO bats ").colset(Bat.Empty, msk)._VALUES_(Bat.Empty, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_MKT_, User.ROL_OPN)]
    [Ui("减仓", "新建减仓单", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task dec(WebContext wc)
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
            var items = dc.ToIntMap();

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("操作类型", nameof(o.typ), o.typ, Bat.Typs, filter: (k, _) => k >= Bat.TYP_SUB)._LI();
                h.LI_().SELECT("商品", nameof(o.itemid), o.itemid, items)._LI();
                h.LI_().TEXTAREA("附加说明", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().NUMBER("数量", nameof(o.qty), o.qty, min: 1, max: 9999)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(inc))._FORM();
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
public class SrclyBatWork : BatWork<SrclyBatVarWork>
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
    public async Task inc(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        Org[] srcs = null;
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

                h.LI_().SELECT("操作类型", nameof(o.typ), o.typ, Bat.Typs, filter: (k, _) => k == 1)._LI();
                h.LI_().SELECT("商品", nameof(o.itemid), o.itemid, map)._LI();
                h.LI_().SELECT("产源", nameof(o.srcid), o.srcid, srcs)._LI();
                h.LI_().SELECT("品控云仓", nameof(o.hubid), o.hubid, hubs)._LI();
                h.LI_().NUMBER("数量", nameof(o.qty), o.qty, min: 1, max: 9999)._LI();
                h.LI_().TEXTAREA("附加说明", nameof(o.tip), o.tip, max: 40)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(inc))._FORM();
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
    public async Task dec(WebContext wc)
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

                h.LI_().SELECT("操作类型", nameof(o.typ), o.typ, Bat.Typs, filter: (k, _) => k > 1)._LI();
                h.LI_().SELECT("商品", nameof(o.itemid), o.itemid, map)._LI();
                h.LI_().SELECT("品控云仓", nameof(o.hubid), o.hubid, hubs)._LI();
                h.LI_().NUMBER("数量", nameof(o.qty), o.qty, min: 1, max: 9999)._LI();
                h.LI_().TEXTAREA("附加说明", nameof(o.tip), o.tip, max: 40)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(inc))._FORM();
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
[Ui("货管")]
public class HublyBatWork : BatWork<HublyBatVarWork>
{
    [Ui(tip: "按批操作", status: 2), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE hubid = @1 AND status > 2 ORDER BY oked DESC limit 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Bat>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无已作废的调运操作");
                return;
            }

            MainGrid(h, arr);
            h.PAGINATION(arr?.Length == 20);
        }, false, 6);
    }
}