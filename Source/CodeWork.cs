﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

namespace ChainSmart;

public abstract class CodeWork<V> : WebWork where V : CodeVarWork, new()
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

[MgtAuthorize(Org._BCK)]
[Ui("溯源码领单")]
public class SuplyCodeWork : CodeWork<SuplyCodeVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE parentid = @1 AND status = 1 LIMIT 20 OFFSET @1");
        var arr = await dc.QueryAsync<Code>(p => p.Set(org.id).Set(page * 20));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 4);
            if (arr == null)
            {
                h.ALERT("尚无新的领单");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }

    [Ui(tip: "已提交", icon: "arrow-right", status: 2), Tool(Anchor)]
    public async Task adapted(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE parentid = @1 AND status = 2 ORDER BY adapted DESC");
        var arr = await dc.QueryAsync<Code>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无已提交的申请");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }


    [Ui(tip: "已拒绝", icon: "trash", status: 4), Tool(Anchor)]
    public async Task @void(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE parentid = @1 AND status = 0 ORDER BY adapted DESC");
        var arr = await dc.QueryAsync<Code>(p => p.Set(org.id).Set(page * 20));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无已拒绝的申请");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }

    [Ui("新建", tip: "新建溯源码申请", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int cmd)
    {
        var prin = (User)wc.Principal;
        var org = wc[-1].As<Org>();

        var o = new Code
        {
            created = DateTime.Now,
            creator = prin.name,
            orgid = org.id,
            status = 4,
        };

        if (wc.IsGet)
        {
            string tel = wc.Query[nameof(tel)];

            wc.GivePane(200, h =>
            {
                h.FORM_();

                h.FIELDSUL_(wc.Action.Tip);
                h.LI_().TEXT("手机号", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(@new), 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();
                h._FIELDSUL();

                if (cmd == 1) // search user
                {
                    using var dc = NewDbContext();
                    dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                    var u = dc.QueryTop<User>(p => p.Set(tel));

                    if (u == null)
                    {
                        h.ALERT("该手机号没有注册！");
                        return;
                    }
                    h.FIELDSUL_();

                    h.HIDDEN(nameof(o.cnt), u.id);
                    h.LI_().TEXT("用户名", nameof(o.name), u.name, @readonly: true)._LI();
                    // h.LI_().TEXT("身份证号", nameof(o.nstart), o.nstart, min: 18, max: 18)._LI();
                    h.LI_().SELECT("民生卡类型", nameof(o.typ), o.typ, Code.Typs, filter: (k, _) => k <= 2, required: true)._LI();
                    // h.LI_().TEXT("民生卡号", nameof(o.nend), o.nend, min: 4, max: 8)._LI();

                    h._FIELDSUL();
                    h.BOTTOMBAR_().BUTTON("确认", nameof(@new), 2)._BOTTOMBAR();
                }
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