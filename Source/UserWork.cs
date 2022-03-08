using SkyChain.Web;
using static SkyChain.Web.Modal;
using static Revital.User;

namespace Revital
{
    public abstract class UserWork : WebWork
    {
    }

    [UserAuthorize(admly: ADMLY_MGT)]
    [Ui("平台用户管理", "users")]
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
                    h.TD("⊘", @if: o.IsGone);
                });
                h.PAGINATION(arr?.Length == 30);
            });
        }

        [Ui("查询"), Tool(AnchorPrompt, Appear.Small)]
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
                        h.TD("⊘", @if: o.IsGone);
                    });
                }, false, 3);
            }
        }
    }

    [UserAuthorize(Org.TYP_MRT, 1)]
    [Ui("市场消费者管理", "users")]
    public class MrtlyUserWork : UserWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<MrtlyUserVarWork>();
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
                    h.TD_("uk-width-tiny").T(Typs[o.typ])._TD();
                    h.TD_("uk-width-medium uk-visible@s");
                    if (o.orgid > 0)
                    {
                        // h.T(orgs[o.orgid].name).SP().T(Orgly[o.orgly]);
                    }
                    h._TD();
                    h.TD("⊘", @if: o.IsGone);
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
                        h.TD_("uk-width-tiny").T(Typs[o.typ])._TD();
                        h.TD_("uk-width-medium uk-visible@s");
                        if (o.orgid > 0)
                        {
                            // h.T(orgs[o.orgid].name).SP().T(Orgly[o.orgly]);
                        }
                        h._TD();
                        h.TD("⊘", @if: o.IsGone);
                        h.TDFORM(() => h.TOOLGROUPVAR(o.Key));
                    });
                }, false, 3);
            }
        }
    }
}