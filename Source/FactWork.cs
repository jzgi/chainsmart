using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

public abstract class FactWork<V> : WebWork where V : FactVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Fact> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name);

            h.SPAN(Fact.Statuses[o.status], "uk-badge");
            h._HEADER();

            h.Q(o.tip, "uk-width-expand");
            h._ASIDE();

            h._A();
        });
    }
}

[Ui("事项")]
public class MktlyFactWork : FactWork<MktlyFactVarWork>
{
    [Ui("通知", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Fact.Empty).T(" FROM facts WHERE orgid = @1 AND typ = 1 ORDER BY oked DESC");
        var arr = await dc.QueryAsync<Fact>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);

            if (arr == null)
            {
                h.ALERT("尚无通知");
                return;
            }

            MainGrid(h, arr);
        }, false, 4);
    }

    [Ui("其他事项", status: 2), Tool(Anchor)]
    public async Task other(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Fact.Empty).T(" FROM facts WHERE orgid = @1 AND typ > 1 ORDER BY oked DESC");
        var arr = await dc.QueryAsync<Fact>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 2);

            if (arr == null)
            {
                h.ALERT("尚无其他事项");
                return;
            }

            MainGrid(h, arr);
        }, false, 4);
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("新建", icon: "plus", status: 7), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int typ)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        var o = new Fact
        {
            typ = (short)typ,
            orgid = org.id,
            created = DateTime.Now,
            creator = prin.name,
        };
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("新建" + Fact.Typs[o.typ]);

                h.LI_().TEXT("事务名", nameof(o.name), o.name, max: 12)._LI();
                h.LI_().TEXTAREA("内容", nameof(o.tip), o.tip, max: 40)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new), subscript: typ)._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, o);

            // insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO facts ").colset(Fact.Empty, msk)._VALUES_(Fact.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

[Ui("事项")]
public class CtrlyFactWork : FactWork<CtrlyFactVarWork>
{
}