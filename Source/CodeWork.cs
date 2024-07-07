using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;
using static ChainFX.Web.ToolAttribute;

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
            h.HEADER_().H4_().T(o.name)._H4().SUB(o.tel).SPAN((Code.Statuses[o.status]), "uk-badge");
            h._HEADER();

            h.Q(o.addr, "uk-width-expand");

            var tags = Grab<short, Tag>();

            h.FOOTER_();
            if (o.nstart > 0 || o.nend > 0)
            {
                h.SPAN_().T(o.nstart, digits: 8).T('-').T(o.nend, digits: 8)._SPAN();
            }
            h.SPAN_("uk-margin-auto-left").T(tags[o.tag]?.name).SP().T(o.Num).T(" 个")._SPAN();
            h._FOOTER();
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
}

[MgtAuthorize(0, User.ROL_OPN)]
[Ui("溯源码")]
public class AdmlyCodeWork : CodeWork<AdmlyCodeVarWork>
{
    [Ui(tip: "新的溯源码发放单", status: 1 | 2), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE status = 1 OR status = 2 LIMIT 20 OFFSET @1 * 20");
        var arr = await dc.QueryAsync<Code>(p => p.Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(status: 1);
            if (arr == null)
            {
                h.ALERT("尚无新的溯源码发放单");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }


    [Ui(tip: "已发放", icon: "arrow-right", status: 4), Tool(Anchor)]
    public async Task oked(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE status = 4 LIMIT 20 OFFSET @1 * 20");
        var arr = await dc.QueryAsync<Code>(p => p.Set(page * 20));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(status: 2);
            if (arr == null)
            {
                h.ALERT("尚无已发放的溯源码组");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }

    [Ui("新建", tip: "新建溯源码发放单", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int cmd)
    {
        var prin = (User)wc.Principal;
        var tags = Grab<short, Tag>();
        var now = DateTime.Now;

        var o = new Code
        {
            created = now,
            creator = prin.name,
            status = STU_CREATED,
        };

        if (wc.IsGet)
        {
            string word = wc.Query[nameof(word)];

            wc.GivePane(200, h =>
            {
                if (cmd == 0)
                {
                    h.FORM_().FIELDSUL_(wc.Action.Tip);
                    h.LI_().TEXT("产源", nameof(word), word, required: true).BUTTON("查找", nameof(@new), 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();
                    h._FIELDSUL()._FORM();
                }
                else if (cmd == 1)
                {
                    var arr = GrabTwinArray<int, Org>(0, filter: x => x.IsSrc && x.name.Contains(word));
                    h.FORM_().FIELDSUL_(wc.Action.Tip);

                    h.LI_().TEXT(string.Empty, nameof(word), word, required: true).BUTTON("查找", nameof(@new), 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();
                    h.LI_().SELECT("产源", nameof(o.orgid), o.orgid, arr)._LI();
                    h.LI_().SELECT("码类型", nameof(o.tag), o.tag, tags)._LI();
                    h.LI_().NUMBER("起始号", nameof(o.nstart), o.nstart)._LI();
                    h.LI_().NUMBER("截止号", nameof(o.nend), o.nend)._LI();
                    h.LI_().TEXTAREA("备注", nameof(o.tip), o.tip, max: 30)._LI();

                    h._FIELDSUL().BOTTOMBAR_().BUTTON("确认", nameof(@new), 2)._BOTTOMBAR();
                    h._FORM();
                }
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            await wc.ReadObjectAsync(msk, instance: o);

            var org = GrabTwin<int, Org>(o.orgid);
            o.name = org.name;
            o.addr = org.addr;
            o.tel = org.tel;

            using var dc = NewDbContext();

            dc.Sql("INSERT INTO codes ").colset(Code.Empty, msk)._VALUES_(o, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // ok
        }
    }
}