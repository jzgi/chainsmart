using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;
using static Rev.Supply.User_;

namespace Rev.Supply
{
    [UserAuthorize(admly: ADMLY_SUPLLY_MGT)]
    [Ui("平台用户")]
    public class AdmlyUserWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyUserVarWork>();
        }

        [Ui("浏览", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM users ORDER BY name LIMIT 30 OFFSET 30 * @1");
            var arr = dc.Query<User_>(p => p.Set(page));
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
                    h.TD("⊘", when: o.IsDisabled);
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
                var arr = dc.Query<User_>(p => p.Set(tel));
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
                        h.TD("⊘", when: o.IsDisabled);
                        h.TDFORM(() => h.VARTOOLS(o.Key));
                    });
                }, false, 3);
            }
        }
    }

    [Ui("人员权限", "users")]
    public class AdmlyAccessWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyAccessVarWork>();
        }

        public void @default(WebContext wc)
        {
            short commid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM users WHERE admly > 0");
            var arr = dc.Query<User_>(p => p.Set(commid));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                    {
                        h.TDCHECK(o.id);
                        h.TD(o.tel);
                        h.TD(o.name)._TD();
                        h.TD(Admly[o.admly]);
                        h.TDFORM(() => h.VARTOOLS(o.Key));
                    }
                );
            }, false, 3);
        }

        [UserAuthorize(admly: ADMLY_SUPLLY_MGT)]
        [Ui("添加", "添加人员权限"), Tool(ButtonOpen)]
        public async Task add(WebContext wc, int cmd)
        {
            short admly = 0;
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
                        var o = dc.QueryTop<User_>(p => p.Set(tel));
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

    [Ui("人员权限", "users")]
    public class OrglyAccessWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<OrglyAccessVarWork>();
        }

        public void @default(WebContext wc)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM users WHERE orgid = @1 AND orgly > 0");
            var arr = dc.Query<User_>(p => p.Set(orgid));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                    {
                        h.TDCHECK(o.id);
                        h.TD_().T(o.name).SP().SUB(o.tel)._TD();
                        h.TD(Orgly[o.orgly]);
                        h.TDFORM(() => h.VARTOOLS(o.Key));
                    }
                );
            }, false, 3);
        }

        [UserAuthorize(orgly: 3)]
        [Ui("添加", "添加人员权限"), Tool(ButtonOpen, Appear.Small)]
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
                        var o = dc.QueryTop<User_>(p => p.Set(tel));
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

    [Ui("消费者")]
    public class MrtlyUserWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MrtlyUserVarWork>();
        }

        [Ui("浏览", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM users ORDER BY name LIMIT 30 OFFSET 30 * @1");
            var arr = dc.Query<User_>(p => p.Set(page));
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
                    h.TD("⊘", when: o.IsDisabled);
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
                var arr = dc.Query<User_>(p => p.Set(tel));
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
                        h.TD("⊘", when: o.IsDisabled);
                        h.TDFORM(() => h.VARTOOLS(o.Key));
                    });
                }, false, 3);
            }
        }
    }
}