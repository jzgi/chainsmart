using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainMart.User;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainMart
{
    public abstract class UserWork<V> : WebWork where V : UserVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    [Ui("账号信息", "功能")]
    public class MyInfoWork : WebWork
    {
        public async Task @default(WebContext wc)
        {
            int uid = wc[-1];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM users WHERE id = @1");
            var o = await dc.QueryTopAsync<User>(p => p.Set(uid));

            wc.GivePage(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("姓名", o.name)._LI();
                h.LI_().FIELD("类别", Typs[o.typ])._LI();
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
                    h.SPAN(Orgly[o.orgly], "uk-width-1-2");
                    h._SECTION();
                }

                h.TOOLBAR(bottom: true);


                // spr and rvr
            }, false, 6);

            // resend token cookie
            wc.SetTokenCookie(o);
        }

        [Ui("设置", icon: "cog"), Tool(ButtonShow)]
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
                dc.Sql("UPDATE users SET name = CASE WHEN @1 IS NULL THEN name ELSE @1 END , tel = @2, credential = @3 WHERE id = @4 RETURNING ").collst(Empty);
                prin = await dc.QueryTopAsync<User>(p => p.Set(name).Set(tel).Set(credential).Set(prin.id));
                // refresh cookie
                wc.SetTokenCookie(prin);
                wc.GivePane(200); // close
            }
        }
    }

    [Ui("人员权限", "系统")]
    public class AdmlyAccessWork : UserWork<AdmlyAccessVarWork>
    {
        [Ui("人员权限"), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            short orgid = wc[-1];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM users_vw WHERE admly > 0");
            var arr = await dc.QueryAsync<User>(p => p.Set(orgid));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");
                    if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(ChainMartApp.WwwUrl).T("/user/").T(o.id).T("/icon")._PIC();
                    }
                    else
                    {
                        h.PIC("/void.webp", css: "uk-width-1-5");
                    }
                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name).SP().SUB(o.tel);
                    h.P(Orgly[o.orgly]);
                    h._DIV();
                    h._A();
                });
            }, false, 3);
        }

        [UserAuthorize(orgly: 3)]
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
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(add), 1, post: false, css: "uk-button-secondary")._LI();
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
                            h.LI_().SELECT("权限", nameof(admly), admly, Admly, filter: (k, v) => k > 0)._LI();
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
                dc.Execute("UPDATE users SET admly = @1 WHERE id = @2", p => p.Set(admly).Set(id));

                wc.GivePane(200); // ok
            }
        }
    }


    [UserAuthorize(admly: ADMLY_MGT)]
    [Ui("用户管理", "业务")]
    public class AdmlyUserWork : UserWork<AdmlyUserVarWork>
    {
        [Ui("浏览", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            using var dc = NewDbContext();
            var num = (int) (await dc.ScalarAsync("SELECT count(*)::int FROM users"));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.SECTION_("uk-card uk-card-primary");
                h.H4("总用户数", "uk-card-header");
                h.DIV_("uk-card-body").T(num)._DIV();
                h._SECTION();
            });
        }

        [Ui(tip: "查询", icon: "search"), Tool(AnchorPrompt)]
        public async Task search(WebContext wc)
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
                var arr = await dc.QueryAsync<User>(p => p.Set(tel));

                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    h.MAINGRID(arr, o =>
                    {
                        h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
                        if (o.icon)
                        {
                            h.PIC_("uk-width-1-5").T(ChainMartApp.WwwUrl).T("/user/").T(o.id).T("/icon")._PIC();
                        }
                        else
                        {
                            h.PIC("/void.webp", css: "uk-width-1-5");
                        }
                        h.DIV_("uk-width-expand uk-padding-left");
                        h.H5(o.name).SP().SUB(o.tel);
                        h.P(Orgly[o.orgly]);
                        h._DIV();
                        h._A();
                    });
                }, false, 30);
            }
        }
    }

    [Ui("人员权限", "基础")]
    public class OrglyAccessWork : UserWork<OrglyAccessVarWork>
    {
        [Ui("人员权限"), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM users_vw WHERE orgid = @1 AND orgly > 0");
            var arr = await dc.QueryAsync<User>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");
                    if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(ChainMartApp.WwwUrl).T("/user/").T(o.id).T("/icon")._PIC();
                    }
                    else
                    {
                        h.PIC("/void.webp", css: "uk-width-1-5");
                    }
                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name).SP().SUB(o.tel);
                    h.P(Orgly[o.orgly]);
                    h._DIV();
                    h._A();
                });
            }, false, 6);
        }

        [UserAuthorize(orgly: 3)]
        [Ui("添加", tip: "添加人员权限", icon: "plus"), Tool(ButtonOpen)]
        public async Task add(WebContext wc, int cmd)
        {
            short orgly = 0;
            if (wc.IsGet)
            {
                string tel = wc.Query[nameof(tel)];
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("指定用户");
                    h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(add), 1, post: false, css: "uk-button-secondary")._LI();
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
    public class MktlyCustWork : UserWork<MktlyCustVarWork>
    {
        [Ui("最近消费", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            int mrtid = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT DISTINCT ON (id) ").collst(Empty, alias: "u").T(" FROM users u, buys b WHERE u.id = b.uid AND b.mktid = @1 AND b.state > 2 LIMIT 20 OFFSET 20 * @2");
            var arr = dc.Query<User>(p => p.Set(mrtid).Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");
                    if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(ChainMartApp.WwwUrl).T("/user/").T(o.id).T("/icon")._PIC();
                    }
                    else
                    {
                        h.PIC("/void.webp", css: "uk-width-1-5");
                    }
                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name);
                    h.P(o.tip);
                    h._DIV();
                    h._A();
                });

                h.PAGINATION(arr?.Length == 20);
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