using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainMart.User;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class UserWork : WebWork
    {
    }

    [Ui("账号信息", "功能")]
    public class MyInfoWork : UserWork
    {
        [Ui("账号信息"), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            int uid = wc[-1];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM users WHERE id = @1");
            var o = await dc.QueryTopAsync<User>(p => p.Set(uid));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("姓名", o.name)._LI();
                h.LI_().FIELD("类别", User.Typs[o.typ])._LI();
                h.LI_().FIELD("简述", o.tip)._LI();
                h.LI_().FIELD("电话", o.tel)._LI();
                h.LI_().FIELD("地址", o.addr)._LI();
                h._UL();

                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD2("创建", o.creator, o.created)._LI();
                h.LI_().FIELD2("更改", o.adapter, o.adapted)._LI();
                h._UL();

                // admly & orgly
                if (o.IsOrgly)
                {
                    var org = GrabObject<int, Org>(o.orgid);
                    h.SECTION_("uk-card uk-card-primary uk-card-body");
                    h.SPAN(org.name, "uk-width-1-2");
                    h.SPAN(User.Orgly[o.orgly], "uk-width-1-2");
                    h._SECTION();
                }
                // spr and rvr
            }, false, 6);

            // resend token cookie
            wc.SetTokenCookie(o);
        }

        [Ui("设置", icon: "cog"), Tool(ButtonOpen)]
        public async Task setg(WebContext wc)
        {
            const string PASSMASK = "t#0^0z4R4pX7";
            string name;
            string tel;
            string password;
            var prin = (User) wc.Principal;
            if (wc.IsGet)
            {
                name = prin.name;
                tel = prin.tel;
                password = string.IsNullOrEmpty(prin.credential) ? null : PASSMASK;
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");
                    h.LI_().TEXT("姓名", nameof(name), name, max: 8, min: 2, required: true)._LI();
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h._FIELDSUL().FIELDSUL_("操作密码（可选）");
                    h.LI_().PASSWORD("密码", nameof(password), password, max: 12, min: 3)._LI();
                    h._FIELDSUL()._FORM();

                    h.BOTTOM_BUTTON("确定", nameof(setg));
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                name = f[nameof(name)];
                tel = f[nameof(tel)];
                password = f[nameof(password)];
                string credential = string.IsNullOrEmpty(password) ? null :
                    password == PASSMASK ? prin.credential : ChainMartUtility.ComputeCredential(tel, password);

                using var dc = NewDbContext();
                dc.Sql("UPDATE users SET name = CASE WHEN @1 IS NULL THEN name ELSE @1 END , tel = @2, credential = @3 WHERE id = @4 RETURNING ").collst(User.Empty);
                prin = await dc.QueryTopAsync<User>(p => p.Set(name).Set(tel).Set(credential).Set(prin.id));
                // refresh cookie
                wc.SetTokenCookie(prin);
                wc.GivePane(200); // close
            }
        }
    }

    [Ui("人员权限", "系统")]
    public class AdmlyAccessWork : UserWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<AdmlyAccessVarWork>();
        }

        public void @default(WebContext wc)
        {
            short orgid = wc[-1];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM users WHERE orgid = @1 AND orgly > 0");
            var arr = dc.Query<User>(p => p.Set(orgid));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                    {
                        h.TD_().T(o.name).SP().SUB(o.tel)._TD();
                        h.TD_().T(Orgly[o.orgly])._TD();
                        h.TDFORM(() => h.VARTOOLSET(o.Key));
                    }
                );
            }, false, 3);
        }

        [UserAuthorize(orgly: 3)]
        [Ui("添加", icon: "plus"), Tool(ButtonOpen)]
        public async Task add(WebContext wc, int cmd)
        {
            short orgly = 0;
            if (wc.IsGet)
            {
                string tel = wc.Query[nameof(tel)];
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("授权给指定用户");
                    h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(add), 1, post: false)._LI();
                    h._FIELDSUL();
                    if (cmd == 1) // search user
                    {
                        using var dc = NewDbContext();
                        dc.Sql("SELECT ").collst(Empty).T(" FROM users WHERE tel = @1");
                        var o = dc.QueryTop<User>(p => p.Set(tel));
                        if (o != null)
                        {
                            h.FIELDSUL_();
                            h.HIDDEN(nameof(o.id), o.id);
                            h.LI_().FIELD("用户姓名", o.name)._LI();
                            h.LI_().SELECT("权限", nameof(orgly), orgly, Orgly, filter: (k, v) => k > 0)._LI();
                            h._FIELDSUL();
                            h.BOTTOMBAR_().BUTTON("确认", nameof(add), 2)._BOTTOMBAR();
                        }
                    }
                    h._FORM();
                });
            }
            else // POST
            {
                short orgid = wc[-1];
                var f = await wc.ReadAsync<Form>();
                int id = f[nameof(id)];
                orgly = f[nameof(orgly)];
                using var dc = NewDbContext();
                dc.Execute("UPDATE users SET orgid = @1, orgly = @2 WHERE id = @3", p => p.Set(orgid).Set(orgly).Set(id));
                wc.GivePane(200); // ok
            }
        }
    }


    [UserAuthorize(admly: ADMLY_MGT)]
    [Ui("用户管理", "业务")]
    public class AdmlyUserWork : UserWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<AdmlyUserVarWork>();
        }

        [Ui("浏览", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM users ORDER BY name LIMIT 30 OFFSET 30 * @1");
            var arr = dc.Query<User>(p => p.Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD(o.name);
                    h.TD(o.tel);
                    h.TD_().T(Typs[o.typ])._TD();
                    h.TD_("uk-visible@s");
                    if (o.orgid > 0)
                    {
                        // h.T(orgs[o.orgid].name).SP().T(Orgly[o.orgly]);
                    }
                    h._TD();
                    h.TD("⊘", @if: o.IsDisabled);
                });
                h.PAGINATION(arr?.Length == 30);
            });
        }

        [Ui("查询"), Tool(AnchorPrompt)]
        public void search(WebContext wc)
        {
            bool inner = wc.Query[nameof(inner)];
            string tel = null;
            if (inner)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("查找用户");
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h._FIELDSUL()._FORM();
                });
            }
            else // OUTER
            {
                tel = wc.Query[nameof(tel)];
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Empty).T(" FROM users WHERE tel = @1");
                var arr = dc.Query<User>(p => p.Set(tel));
                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    h.TABLE(arr, o =>
                    {
                        h.TDCHECK(o.id);
                        h.TD(o.name);
                        h.TD(o.tel);
                        h.TD_().T(Typs[o.typ])._TD();
                        h.TD_("uk-visible@s");
                        if (o.orgid > 0)
                        {
                            // h.T(orgs[o.orgid].name).SP().T(Orgly[o.orgly]);
                        }
                        h._TD();
                        h.TD("⊘", @if: o.IsDisabled);
                    });
                }, false, 3);
            }
        }
    }

    [Ui("人员权限", "基础")]
    public class OrglyAccessWork : UserWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<OrglyAccessVarWork>();
        }

        public void @default(WebContext wc)
        {
            short orgid = wc[-1];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM users WHERE orgid = @1 AND orgly > 0");
            var arr = dc.Query<User>(p => p.Set(orgid));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                    {
                        h.TD_().T(o.name).SP().SUB(o.tel)._TD();
                        h.TD_().T(Orgly[o.orgly])._TD();
                        h.TDFORM(() => h.VARTOOLSET(o.Key));
                    }
                );
            }, false, 3);
        }

        [UserAuthorize(orgly: 3)]
        [Ui("添加", icon: "plus"), Tool(ButtonOpen)]
        public async Task add(WebContext wc, int cmd)
        {
            short orgly = 0;
            if (wc.IsGet)
            {
                string tel = wc.Query[nameof(tel)];
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("授权给指定用户");
                    h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(add), 1, post: false)._LI();
                    h._FIELDSUL();
                    if (cmd == 1) // search user
                    {
                        using var dc = NewDbContext();
                        dc.Sql("SELECT ").collst(Empty).T(" FROM users WHERE tel = @1");
                        var o = dc.QueryTop<User>(p => p.Set(tel));
                        if (o != null)
                        {
                            h.FIELDSUL_();
                            h.HIDDEN(nameof(o.id), o.id);
                            h.LI_().FIELD("用户姓名", o.name)._LI();
                            h.LI_().SELECT("权限", nameof(orgly), orgly, Orgly, filter: (k, v) => k > 0)._LI();
                            h._FIELDSUL();
                            h.BOTTOMBAR_().BUTTON("确认", nameof(add), 2)._BOTTOMBAR();
                        }
                    }
                    h._FORM();
                });
            }
            else // POST
            {
                short orgid = wc[-1];
                var f = await wc.ReadAsync<Form>();
                int id = f[nameof(id)];
                orgly = f[nameof(orgly)];
                using var dc = NewDbContext();
                dc.Execute("UPDATE users SET orgid = @1, orgly = @2 WHERE id = @3", p => p.Set(orgid).Set(orgly).Set(id));

                wc.GivePane(200); // ok
            }
        }
    }

    [UserAuthorize(Org.TYP_MKT, 1)]
#if ZHNT
    [Ui("消费者管理", "市场")]
#else
    [Ui("消费者管理", "驿站")]
#endif
    public class MktlyCustWork : UserWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<MktlyCustVarWork>();
        }

        [Ui("全部消费者", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            int mrtid = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT DISTINCT ON (id) ").collst(User.Empty, alias: "u").T(" FROM users u, buys b WHERE u.id = b.uid AND b.mktid = @1 AND state > 2 LIMIT 30 OFFSET 30 * @2");
            var arr = dc.Query<User>(p => p.Set(mrtid).Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD(o.name);
                    h.TD(o.tel);
                    h.TD_("uk-width-tiny").T(Typs[o.typ])._TD();
                    h.TD_("uk-width-medium uk-visible@s");
                    if (o.orgid > 0)
                    {
                        // h.T(orgs[o.orgid].name).SP().T(Orgly[o.orgly]);
                    }
                    h._TD();
                    h.TD("⊘", @if: o.IsDisabled);
                });
                h.PAGINATION(arr?.Length == 30);
            });
        }

        [Ui(tip: "查询", icon: "search"), Tool(AnchorPrompt)]
        public void search(WebContext wc)
        {
            bool inner = wc.Query[nameof(inner)];
            string tel = null;
            if (inner)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("在本市场消费过的用户");
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h._FIELDSUL()._FORM();
                });
            }
            else // OUTER
            {
                tel = wc.Query[nameof(tel)];
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Empty).T(" FROM users WHERE tel = @1");
                var arr = dc.Query<User>(p => p.Set(tel));
                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    h.TABLE(arr, o =>
                    {
                        h.TDCHECK(o.id);
                        h.TD(o.name);
                        h.TD(o.tel);
                        h.TD_("uk-width-tiny").T(Typs[o.typ])._TD();
                        h.TD_("uk-width-medium uk-visible@s");
                        if (o.orgid > 0)
                        {
                            // h.T(orgs[o.orgid].name).SP().T(Orgly[o.orgly]);
                        }
                        h._TD();
                        h.TD("⊘", @if: o.IsDisabled);
                        h.TDFORM(() => h.VARTOOLSET(o.Key));
                    });
                }, false, 3);
            }
        }
    }
}