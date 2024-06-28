using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;
using static ChainFX.Web.ToolAttribute;
using static ChainSmart.MainUtility;

namespace ChainSmart;

public abstract class CodeWork<V> : WebWork where V : WebWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Code> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC("/void.webp", css: "uk-width-1-6");

            h.ASIDE_();
            h.HEADER_().H4_().T(o.name).SP().T(o.num).T(" 个")._H4();
            h.SPAN((Code.Statuses[o.status]), "uk-badge");
            h._HEADER();

            h.Q(o.tip, "uk-width-expand");

            var org = GrabTwin<int, Org>(o.orgid);

            h.FOOTER_();
            if (o.nstart > 0 || o.nend > 0)
            {
                h.SPAN_().T(o.nstart, digits: 8).T('-').T(o.nend, digits: 8)._SPAN();
            }
            h.SPAN(org.name, css: "uk-margin-auto-left")._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}


[MgtAuthorize(Org.TYP_SRC)]
[Ui("溯源码")]
public class SrclyCodeWork : CodeWork<SrclyCodeVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE orgid = @1 ORDER BY created DESC LIMIT 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Code>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无申请");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }

    [Ui("申请", tip: "新建溯源码申请", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task @new(WebContext wc)
    {
        var prin = (User)wc.Principal;
        var org = wc[-1].As<Org>();
        var tags = Grab<short, Tag>();
        var now = DateTime.Now;


        var o = new Code
        {
            created = now,
            creator = prin.name,
            adapted = now,
            adapter = prin.name,
            orgid = org.id,
            status = STU_ADAPTED,
        };

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().FIELD("码类型", o.typ, tags)._LI();
                h.LI_().NUMBER("申请数量", nameof(o.num), o.num)._LI();
                h.LI_().TEXT("收件地址", nameof(o.addr), o.addr, max: 40)._LI();
                h.LI_().TEXTAREA("备注", nameof(o.tip), o.tip, max: 30)._LI();

                h._FIELDSUL().BOTTOMBAR_().BUTTON("确认", nameof(@new), 2)._BOTTOMBAR();
                h._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;

            await wc.ReadObjectAsync(msk, instance: o);

            using var dc = NewDbContext();
            dc.Sql("INSERT INTO codes ").colset(Code.Empty, msk)._VALUES_(o, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // ok
        }
    }
}

[MgtAuthorize(0, User.ROL_OPN)]
[Ui("溯源码发放")]
public class AdmlyCodeWork : CodeWork<AdmlyCodeVarWork>
{
    [Ui("溯源码", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE status = 1 LIMIT 20 OFFSET @1 * 20");
        var arr = await dc.QueryAsync<Code>(p => p.Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);
            if (arr == null)
            {
                h.ALERT("尚无产源提交的申请");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }

    [Ui(tip: "已发放", icon: "arrow-right", status: 2), Tool(Anchor)]
    public async Task adapted(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE status = 2 LIMIT 20 OFFSET @1 * 20");
        var arr = await dc.QueryAsync<Code>(p => p.Set(page * 20));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 2);
            if (arr == null)
            {
                h.ALERT("尚无已发放");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }

    [Ui(tip: "已用完", icon: "ban", status: 2), Tool(Anchor)]
    public async Task oked(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE status = 4 LIMIT 20 OFFSET @1 * 20");
        var arr = await dc.QueryAsync<Code>(p => p.Set(page * 20));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 2);
            if (arr == null)
            {
                h.ALERT("尚无已用完");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }
}