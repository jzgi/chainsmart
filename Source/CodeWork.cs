﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

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


[MgtAuthorize(Org.TYP_SRC)]
[Ui("溯源码")]
public class SrclyCodeWork : CodeWork<SuplyCodeVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE orgid = @1 ORDER BY created DESC LIMIT 20 OFFSET @1 * 20");
        var arr = await dc.QueryAsync<Code>(p => p.Set(org.id).Set(page * 20));

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

        var o = new Code
        {
            name = org.name,
            typ = org.tagtyp,
            created = DateTime.Now,
            creator = prin.name,
            orgid = org.id,
            status = 1,
        };

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);
                h.LI_().SELECT("类型", nameof(o.typ), o.typ, tags)._LI();
                h.LI_().NUMBER("申请数量", nameof(o.num), o.num, @readonly: true)._LI();
                h._FIELDSUL();
                h.BOTTOMBAR_().BUTTON("确认", nameof(@new), 2)._BOTTOMBAR();
                h._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            o.Read(f);

            using var dc = NewDbContext();
            dc.Sql("INSERT INTO codes ").colset(Code.Empty)._VALUES_(o);
            await dc.ExecuteAsync(p => o.Write(p));

            wc.GivePane(200); // ok
        }
    }
}

[MgtAuthorize(0, User.ROL_OPN)]
[Ui("溯源码发放")]
public class AdmlyCodeWork : CodeWork<AdmlyCodeVarWork>
{
    [Ui("溯源码申请", status: 1), Tool(Anchor)]
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
                h.ALERT("尚无申请");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }

    [Ui(tip: "已发放", icon: "arrow-right", status: 2), Tool(Anchor)]
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
                h.ALERT("尚无发放");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }
}