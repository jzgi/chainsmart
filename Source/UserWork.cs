using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

public abstract class UserWork<V> : WebWork where V : UserVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>(state: State);
    }

    protected static void MainGrid(HtmlBuilder h, User[] arr, bool? rtl)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

            if (o.icon)
            {
                h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/user/").T(o.id).T("/icon")._PIC();
            }
            else
                h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();

            h.HEADER_().H4(o.name);
            var role = rtl.HasValue ? rtl.Value ? User.Orgly[o.rtlly] : User.Orgly[o.suply] : User.Admly[o.admly];
            h.SPAN(role, "uk-badge");

            h._HEADER();

            h.Q(o.tel, "uk-width-expand");
            h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

[Ui("人员权限", "常规")]
public class AdmlyAccessWork : UserWork<AdmlyAccessVarWork>
{
    [Ui("人员权限"), Tool(Anchor)]
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

    [AdmlyAuthorize(User.ROL_MGT)]
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

                h.FIELDSUL_("授权给指定用户");
                h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(add), 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();
                h._FIELDSUL();

                if (cmd == 1) // search user
                {
                    using var dc = NewDbContext();
                    dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                    var o = dc.QueryTop<User>(p => p.Set(tel));
                    if (o != null)
                    {
                        h.FIELDSUL_();
                        h.HIDDEN(nameof(o.id), o.id);
                        h.LI_().FIELD("用户姓名", o.name)._LI();
                        if (o.supid > 0)
                        {
                            var org = GrabRow<int, Org>(o.supid);
                            h.LI_().FIELD2("现有权限", org.name, User.Orgly[o.suply])._LI();
                        }
                        else
                        {
                            h.LI_().FIELD("现有权限", "无")._LI();
                        }

                        h.LI_().SELECT("权限", nameof(admly), admly, User.Orgly, filter: (k, _) => k > 0)._LI();
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


[AdmlyAuthorize(User.ROL_OPN)]
[Ui("用户管理", "业务")]
public class AdmlyUserWork : UserWork<AdmlyUserVarWork>
{
    [Ui("用户", group: 1), Tool(Anchor)]
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

    [Ui(tip: "查询用户", icon: "search", group: 2), Tool(AnchorPrompt)]
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

[Ui("人员权限", "常规")]
public class OrglyAccessWork : UserWork<OrglyAccessVarWork>
{
    bool IsShop => (bool)State;


    [Ui("人员权限"), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users_vw WHERE ").T(IsShop ? "rtlid" : "supid").T(" = @1 AND ").T(IsShop ? "rtlly" : "suply").T(" > 0");
        var arr = await dc.QueryAsync<User>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无人员权限");
                return;
            }

            MainGrid(h, arr, IsShop);
        }, false, 6);
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("添加", tip: "添加人员权限", icon: "plus"), Tool(ButtonOpen)]
    public async Task add(WebContext wc, int cmd)
    {
        var org = wc[-1].As<Org>();

        var rtl = (bool)State;

        string password = null;

        short orgly = 0;
        if (wc.IsGet)
        {
            string tel = wc.Query[nameof(tel)];

            wc.GivePane(200, h =>
            {
                h.FORM_();

                h.FIELDSUL_("指定用户");
                h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(add), 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();
                h._FIELDSUL();

                if (cmd == 1) // search user
                {
                    using var dc = NewDbContext();
                    dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                    var o = dc.QueryTop<User>(p => p.Set(tel));

                    if (o != null)
                    {
                        h.FIELDSUL_();

                        h.HIDDEN(nameof(o.id), o.id);
                        h.HIDDEN(nameof(o.tel), o.tel);

                        h.LI_().FIELD("用户姓名", o.name)._LI();
                        var yes = true;
                        if (o.supid > 0)
                        {
                            var exOrg = GrabRow<int, Org>(rtl ? o.rtlid : o.supid);
                            if (exOrg != null)
                            {
                                h.LI_().FIELD2("现有权限", exOrg.name, User.Orgly[rtl ? o.rtlly : o.suply])._LI();
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
                            h.LI_().SELECT("授予权限", nameof(orgly), orgly, User.Orgly, filter: (k, _) => k > 1 && k <= User.ROL_MGT, required: true)._LI();
                            h.LI_().PASSWORD("操作密码", nameof(password), password, tip: "四到八位数", min: 4, max: 8)._LI();
                        }

                        h._FIELDSUL();
                        h.BOTTOMBAR_().BUTTON("确认", nameof(add), 2, disabled: !yes)._BOTTOMBAR();
                    }
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
            dc.Sql("UPDATE users SET ").T(rtl ? "rtlid" : "supid").T(" = @1, ").T(rtl ? "rtlly" : "suply").T(" = @2, credential = @3 WHERE id = @4");
            await dc.ExecuteAsync(p => p.Set(org.id).Set(orgly).Set(credential).Set(id));

            wc.GivePane(200); // ok
        }
    }
}


[Ui("大客户", "商户")]
public class RtllyVipWork : UserWork<RtllyVipVarWork>
{
    [Ui("大客户", group: 1), Tool(Anchor)]
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

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("添加", "添加大客户", icon: "plus", group: 1), Tool(ButtonOpen)]
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
                            h.ALERT("该用户已经是大客户");
                        }
                        else if (o.HasVipMAx)
                        {
                            h.ALERT("该用户已经是４个商户的大客户");
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