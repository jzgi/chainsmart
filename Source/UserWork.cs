﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.ToolAttribute;

namespace ChainSmart;

public abstract class UserWork<V> : WebWork where V : UserVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>(state: State);
    }

    protected static void MainGrid(HtmlBuilder h, IEnumerable<User> lst, bool? mktly)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

            if (o.icon)
            {
                h.PIC_("uk-width-tiny").T(MainApp.WwwUrl).T("/user/").T(o.id).T("/icon")._PIC();
            }
            else
            {
                h.PIC("/void.webp", css: "uk-width-tiny");
            }

            h.ASIDE_();

            h.HEADER_().H4(o.name);
            var role = mktly.HasValue ? mktly.Value ? User.Roles[o.mktly] : User.Roles[o.suply] : User.Roles[o.admly];
            h.SPAN(role, "uk-badge");

            h._HEADER();

            h.Q(o.tel, "uk-width-expand");
            h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

[Ui("人员权限")]
public class AdmlyMbrWork : UserWork<AdmlyMbrVarWork>
{
    [Ui, Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users_vw WHERE admly > 0");
        var arr = await dc.QueryAsync<User>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            MainGrid(h, arr, null);
        }, false, 12);
    }

    [MgtAuthorize(0, User.ROL_MGT)]
    [Ui("添加", icon: "plus"), Tool(ButtonOpen)]
    public async Task add(WebContext wc, int cmd)
    {
        short admly = 0;

        if (wc.IsGet)
        {
            string tel = wc.Query[nameof(tel)];

            wc.GivePane(200, h =>
            {
                h.FORM_();

                h.FIELDSUL_("授权给指定用户", css: "uk-list uk-list-divider");
                h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(add), 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();
                h._FIELDSUL();

                if (cmd == 1) // search user
                {
                    using var dc = NewDbContext();
                    dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                    var o = dc.QueryTop<User>(p => p.Set(tel));

                    if (o != null)
                    {
                        h.FIELDSUL_(css: "uk-list uk-list-divider");
                        h.HIDDEN(nameof(o.id), o.id);
                        h.LI_().FIELD("用户姓名", o.name)._LI();
                        if (o.supid > 0)
                        {
                            var org = GrabTwin<int, Org>(o.supid);
                            h.LI_().FIELD2("现有权限", org.name, User.Roles[o.suply])._LI();
                        }
                        else
                        {
                            h.LI_().FIELD("现有权限", "无")._LI();
                        }

                        h.LI_().SELECT("权限", nameof(admly), admly, User.Roles, filter: (k, _) => k > 0)._LI();
                        h._FIELDSUL();
                        h.BOTTOMBAR_().BUTTON("确认", nameof(add), 2)._BOTTOMBAR();
                    }
                }

                h._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            int id = f[nameof(id)];
            admly = f[nameof(admly)];

            using var dc = NewDbContext();
            await dc.ExecuteAsync("UPDATE users SET admly = @1 WHERE id = @2", p => p.Set(admly).Set(id));

            wc.GivePane(200); // ok
        }
    }
}

[Ui("用户")]
public class AdmlyUserWork : UserWork<AdmlyUserVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        using var dc = NewDbContext();
        var num = (int)(await dc.ScalarAsync("SELECT count(*)::int FROM users"));
        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.ALERT("平台用户总数： " + num);
        });
    }

    [Ui(tip: "浏览用户", icon: "list", status: 2), Tool(Anchor)]
    public async Task browse(WebContext wc, int page)
    {
        using var dc = NewDbContext();

        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users_vw OFFSET @1 * 20 LIMIT 20");
        var arr = await dc.QueryAsync<User>(p => p.Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无用户");
                return;
            }

            MainGrid(h, arr, false);

            h.PAGINATION(arr.Length == 20);
        }, false, 6);
    }

    [Ui(tip: "查询用户", icon: "search", status: 4), Tool(AnchorPrompt)]
    public async Task search(WebContext wc)
    {
        bool inner = wc.Query[nameof(inner)];
        string tel = null;
        if (inner)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("用户账号");
                h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true)._LI();
                h._FIELDSUL()._FORM();
            });
        }
        else // OUTER
        {
            tel = wc.Query[nameof(tel)];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
            var arr = await dc.QueryAsync<User>(p => p.Set(tel));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("无此用户");
                    return;
                }

                MainGrid(h, arr, null);
            }, false, 30);
        }
    }
}

[Ui("人员权限")]
public class ShplyMbrWork : UserWork<ShplyMbrVarWork>
{
    [Ui("人员权限"), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users_vw WHERE mktid = @1 AND mktly > 0");
        var arr = await dc.QueryAsync<User>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无人员权限");
                return;
            }

            MainGrid(h, arr, true);
        }, false, 6);
    }

    [MgtAuthorize(Org.TYP_RTL_, User.ROL_MGT)]
    [Ui("添加", icon: "plus"), Tool(ButtonOpen)]
    public async Task add(WebContext wc, int cmd)
    {
        var org = wc[-1].As<Org>();
        string password = null;

        short orgly = 0;
        if (wc.IsGet)
        {
            string tel = wc.Query[nameof(tel)];

            wc.GivePane(200, h =>
            {
                h.FORM_();

                h.FIELDSUL_("添加人员权限");
                h.LI_().TEXT("手机号", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(add), 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();
                h._FIELDSUL();

                if (cmd == 1) // search user
                {
                    using var dc = NewDbContext();
                    dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                    var o = dc.QueryTop<User>(p => p.Set(tel));

                    if (o == null)
                    {
                        h.ALERT("该手机号没有注册！");
                        return;
                    }
                    h.FIELDSUL_();

                    h.HIDDEN(nameof(o.id), o.id);
                    h.HIDDEN(nameof(o.tel), o.tel);

                    h.LI_().FIELD("用户名", o.name)._LI();
                    var yes = true;
                    if (o.supid > 0)
                    {
                        var exOrg = GrabTwin<int, Org>(o.mktid);
                        if (exOrg != null)
                        {
                            h.LI_().FIELD2("现有权限", exOrg.name, User.Roles[o.mktly])._LI();
                            if (exOrg.id != org.id)
                            {
                                h.LI_("uk-flex-center").SPAN("必须先撤销现有权限", css: "uk-text-danger")._LI();
                                yes = false;
                            }
                        }
                    }
                    else
                    {
                        h.LI_().FIELD("现有权限", "无")._LI();
                    }

                    if (yes)
                    {
                        h.LI_().SELECT("授予权限", nameof(orgly), orgly, User.Roles, filter: (k, _) => k > 1 && k <= User.ROL_MGT, required: true)._LI();
                        h.LI_().PASSWORD("操作密码", nameof(password), password, tip: "四到八位数", min: 4, max: 8)._LI();
                    }

                    h._FIELDSUL();
                    h.BOTTOMBAR_().BUTTON("确认", nameof(add), 2, disabled: !yes)._BOTTOMBAR();
                }
                h._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();

            int id = f[nameof(id)];
            string tel = f[nameof(tel)];
            orgly = f[nameof(orgly)];
            password = f[nameof(password)];
            string credential = string.IsNullOrEmpty(password) ? null : MainUtility.ComputeCredential(tel, password);

            using var dc = NewDbContext();
            dc.Sql("UPDATE users SET mktid = @1, mktly = @2, credential = @3 WHERE id = @4");
            await dc.ExecuteAsync(p => p.Set(org.id).Set(orgly).Set(credential).Set(id));

            wc.GivePane(200); // ok
        }
    }
}

[Ui("人员权限")]
public class SuplyMbrWork : UserWork<SuplyMbrVarWork>
{
    [Ui("人员权限"), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users_vw WHERE supid = @1 AND suply > 0");
        var arr = await dc.QueryAsync<User>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无人员权限");
                return;
            }

            MainGrid(h, arr, false);
        }, false, 6);
    }

    [MgtAuthorize(Org.TYP_WHL_, User.ROL_MGT)]
    [Ui("添加", icon: "plus"), Tool(ButtonOpen)]
    public async Task add(WebContext wc, int cmd)
    {
        var org = wc[-1].As<Org>();
        string password = null;
        short orgly = 0;

        if (wc.IsGet)
        {
            string tel = wc.Query[nameof(tel)];

            wc.GivePane(200, h =>
            {
                h.FORM_();

                h.FIELDSUL_("添加人员权限");
                h.LI_().TEXT("手机号", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(add), 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();
                h._FIELDSUL();

                if (cmd == 1) // search user
                {
                    using var dc = NewDbContext();
                    dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                    var o = dc.QueryTop<User>(p => p.Set(tel));

                    if (o == null)
                    {
                        h.ALERT("该手机号没有注册！");
                        return;
                    }
                    h.FIELDSUL_();

                    h.HIDDEN(nameof(o.id), o.id);
                    h.HIDDEN(nameof(o.tel), o.tel);

                    h.LI_().FIELD("用户名", o.name)._LI();
                    var yes = true;
                    if (o.supid > 0)
                    {
                        var exOrg = GrabTwin<int, Org>(o.supid);
                        if (exOrg != null)
                        {
                            h.LI_().FIELD2("现有权限", exOrg.name, User.Roles[o.suply])._LI();
                            if (exOrg.id != org.id)
                            {
                                h.LI_("uk-flex-center").SPAN("必须先撤销现有权限", css: "uk-text-danger")._LI();
                                yes = false;
                            }
                        }
                    }
                    else
                    {
                        h.LI_().FIELD("现有权限", "无")._LI();
                    }

                    if (yes)
                    {
                        h.LI_().SELECT("授予权限", nameof(orgly), orgly, User.Roles, filter: (k, _) => k > 1 && k <= User.ROL_MGT, required: true)._LI();
                        h.LI_().PASSWORD("操作密码", nameof(password), password, tip: "四到八位数", min: 4, max: 8)._LI();
                    }

                    h._FIELDSUL();
                    h.BOTTOMBAR_().BUTTON("确认", nameof(add), 2, disabled: !yes)._BOTTOMBAR();
                }
                h._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();

            int id = f[nameof(id)];
            string tel = f[nameof(tel)];
            orgly = f[nameof(orgly)];
            password = f[nameof(password)];
            string credential = string.IsNullOrEmpty(password) ? null : MainUtility.ComputeCredential(tel, password);

            using var dc = NewDbContext();
            dc.Sql("UPDATE users SET supid = @1, suply = @2, credential = @3 WHERE id = @4");
            await dc.ExecuteAsync(p => p.Set(org.id).Set(orgly).Set(credential).Set(id));

            wc.GivePane(200); // ok
        }
    }
}

[MgtAuthorize(Org.TYP_SHL)]
[Ui("大客户")]
public class ShplyVipWork : UserWork<ShplyVipVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE vip @> ARRAY[@1] LIMIT 20 OFFSET 20 * @2");
        var arr = dc.Query<User>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无大客户");
                return;
            }

            MainGrid(h, arr, true);

            h.PAGINATION(arr.Length == 20);
        });
    }

    [Ui(tip: "查询", icon: "search"), Tool(AnchorPrompt)]
    public void search(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        bool inner = wc.Query[nameof(inner)];
        string tel = null;
        if (inner)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("用户账号");
                h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true)._LI();
                h._FIELDSUL()._FORM();
            });
        }
        else // OUTER
        {
            tel = wc.Query[nameof(tel)];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1 AND vip @> ARRAY[@2]");
            var arr = dc.Query<User>(p => p.Set(tel).Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("没有找到");
                    return;
                }
                MainGrid(h, arr, true);
            }, false, 30);
        }
    }

    [MgtAuthorize(Org.TYP_RTL_, User.ROL_MGT)]
    [Ui("添加", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task add(WebContext wc, int cmd)
    {
        var org = wc[-1].As<Org>();

        if (wc.IsGet)
        {
            string tel = wc.Query[nameof(tel)];
            wc.GivePane(200, h =>
            {
                h.FORM_();

                h.FIELDSUL_("查询用户账号");
                h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(add), 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();
                h._FIELDSUL();

                if (cmd == 1) // search user
                {
                    using var dc = NewDbContext();
                    dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                    var o = dc.QueryTop<User>(p => p.Set(tel));

                    if (o != null)
                    {
                        if (o.IsVipOf(org.id))
                        {
                            h.ALERT("该用户已经是VIP");
                        }
                        else
                        {
                            h.HIDDEN(nameof(o.id), o.id);
                            h.FIELDSUL_().LI_().FIELD("用户名称", o.name)._LI()._FIELDSUL();
                            h.BOTTOMBAR_().BUTTON("确认", nameof(add), 2)._BOTTOMBAR();
                        }
                    }
                    else
                    {
                        h.ALERT("无此用户账号");
                    }
                }

                h._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            int id = f[nameof(id)];

            using var dc = NewDbContext();
            await dc.ExecuteAsync("UPDATE users SET vip = array_append(vip, @1) WHERE id = @2", p => p.Set(org.id).Set(id));

            wc.GivePane(200); // ok
        }
    }
}

[Ui("认证")]
public class MktlyCerWork : UserWork<MktlyCerVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users_vw WHERE orgid = @1 ORDER BY oked DESC");
        var arr = await dc.QueryAsync<User>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无认证用户");
                return;
            }
            MainGrid(h, arr, null);
        }, false, 12);
    }

    [MgtAuthorize(Org.TYP_MKV, User.ROL_OPN)]
    [Ui("添加", icon: "plus"), Tool(ButtonOpen)]
    public async Task add(WebContext wc, int cmd)
    {
        var org = wc[-1].As<Org>();
        var cers = Grab<short, Cer>();
        var prin = (User)wc.Principal;
        short typ;

        if (wc.IsGet)
        {
            string tel = wc.Query[nameof(tel)];

            wc.GivePane(200, h =>
            {
                h.FORM_();

                h.FIELDSUL_("添加用户认证");
                h.LI_().TEXT("手机号", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(add), 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();
                h._FIELDSUL();

                if (cmd == 1) // search user
                {
                    using var dc = NewDbContext();
                    dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                    var o = dc.QueryTop<User>(p => p.Set(tel));

                    if (o == null)
                    {
                        h.ALERT("该手机号没有注册！");
                        return;
                    }
                    h.FIELDSUL_();

                    h.HIDDEN(nameof(o.id), o.id);
                    h.HIDDEN(nameof(o.tel), o.tel);

                    h.LI_().FIELD("用户名", o.name)._LI();
                    var yes = true;
                    if (o.orgid > 0)
                    {
                        var exOrg = GrabTwin<int, Org>(o.orgid);
                        if (exOrg != null)
                        {
                            h.LI_().FIELD2("现有认证", exOrg.name, cers[o.typ])._LI();
                            if (exOrg.id != org.id)
                            {
                                h.LI_("uk-flex-center").SPAN("必须先撤销现有认证", css: "uk-text-danger")._LI();
                                yes = false;
                            }
                        }
                    }
                    else
                    {
                        h.LI_().FIELD("现有认证", "无")._LI();
                    }

                    if (yes)
                    {
                        h.LI_().SELECT("授予认证", nameof(o.typ), o.typ, cers, required: true)._LI();
                    }
                    h._FIELDSUL();
                    h.BOTTOMBAR_().BUTTON("确认", nameof(add), 2, disabled: !yes)._BOTTOMBAR();
                }
                h._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();

            int id = f[nameof(id)];
            typ = f[nameof(typ)];

            using var dc = NewDbContext();
            dc.Sql("UPDATE users SET orgid = @1, typ = @2, oked = @3, oker = @4 WHERE id = @5");
            await dc.ExecuteAsync(p => p.Set(org.id).Set(typ).Set(DateTime.Now).Set(prin.name).Set(id));

            wc.GivePane(200); // ok
        }
    }
}