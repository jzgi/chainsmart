using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;
using static ChainFX.Web.ToolAttribute;

namespace ChainSmart;

public abstract class LotWork<V> : WebWork where V : LotVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Lot> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC("/void.webp", css: "uk-width-tiny");

            h.ASIDE_();
            h.HEADER_().H4(o.name);

            h.SPAN(Bat.Statuses[o.status], "uk-badge");
            h._HEADER();

            h.Q(o.tip, "uk-width-expand");
            h._ASIDE();

            h._A();
        });
    }

    protected static void MainGrid(HtmlBuilder h, IList<Bat> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC("/void.webp", css: "uk-width-tiny");

            h.ASIDE_();
            h.HEADER_().H4(o.name);

            h.SPAN(Bat.Statuses[o.status], "uk-badge");
            h._HEADER();

            h.Q(o.tip, "uk-width-expand");
            h._ASIDE();

            h._A();
        });
    }
}

[MgtAuthorize(Org.TYP_SUP)]
[Ui("云仓库存")]
public class SuplyLotWork : LotWork<SuplyLotVarWork>
{
    [Ui("云仓货管", status: 1), Tool(Anchor)]
    public async Task inv(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE orgid = @1 AND status > 0 ORDER BY oked DESC limit 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);

            if (arr == null)
            {
                h.ALERT("尚无云仓库存记录");
                return;
            }

            MainGrid(h, arr);

            h.PAGINATION(arr?.Length == 20);
        }, false, 6);
    }

    [Ui("操作请求", status: 2), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM jobs WHERE orgid = @1 AND status = 0 ORDER BY oked DESC limit 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Bat>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 2);

            if (arr == null)
            {
                h.ALERT("尚无操作请求");
                return;
            }

            MainGrid(h, arr);
            h.PAGINATION(arr?.Length == 20);
        }, false, 6);
    }

    [MgtAuthorize(Org.TYP_SUP, User.ROL_OPN)]
    [Ui("新建", "新建操作请求", icon: "plus", status: 2), Tool(ButtonOpen)]
    public async Task newop(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        var o = new Bat
        {
            // orgid = org.id,
            created = DateTime.Now,
            creator = prin.name,
        };
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("类型", nameof(o.typ), o.typ, Bat.Typs)._LI();
                h.LI_().TEXT("标题", nameof(o.name), o.name, max: 12)._LI();
                // h.LI_().TEXTAREA("内容", nameof(o.content), o.content, max: 300)._LI();
                h.LI_().TEXTAREA("注解", nameof(o.tip), o.tip, max: 40)._LI();
                // h.LI_().SELECT("级别", nameof(o.rank), o.rank, Lotop.Ranks)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(newop))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, o);

            // insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO lotops ").colset(Bat.Empty, msk)._VALUES_(Bat.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

[MgtAuthorize(Org.TYP_HUB)]
[Ui("库存")]
public class HublyLotWork : LotWork<HublyLotVarWork>
{
    [Ui("库存", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE hubid = @1 AND status > 0 ORDER BY oked DESC limit 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无产品仓管请求");
                return;
            }

            MainGrid(h, arr);

            h.PAGINATION(arr?.Length == 20);
        }, false, 6);
    }

}