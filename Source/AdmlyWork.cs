using System.Threading.Tasks;
using SkyChain;
using SkyChain.Db;
using SkyChain.Web;

namespace Revital
{
    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("平台管理")]
    public class AdmlyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeWork<AdmlyRegWork>("reg");

            MakeWork<AdmlyOrgWork>("org");

            MakeWork<AdmlyItemWork>("item");

            MakeWork<AdmlyClearWork>("clear");

            MakeWork<AdmlyReportWork>("rpt");

            MakeWork<AdmlyUserWork>("user");

            MakeWork<ChainWork>("chain", authorize: new UserAuthorizeAttribute(admly: User.ADMLY_MGT));
        }

        public void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            var o = Chain.Info;
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: prin.name + "（" + User.Admly[prin.admly] + "）");

                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body uk-list uk-list-divider");
                if (o != null)
                {
                    h.LI_().FIELD("平台名称", o.Name)._LI();
                    h.LI_().FIELD("描述", o.Tip)._LI();
                    h.LI_().FIELD("连接地址", o.Uri)._LI();
                }
                h._UL();
                h._FORM();

                h.TASKLIST();
            });
        }

        [UserAuthorize(admly: User.ADMLY_OP)]
        [Ui("操作权限"), Tool(Modal.ButtonOpen)]
        public async Task access(WebContext wc, int cmd)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE admly > 0");
            var arr = dc.Query<User>();
            short admly = 0;

            if (wc.IsGet)
            {
                string tel = wc.Query[nameof(tel)];

                wc.GivePage(200, h =>
                {
                    h.TABLE(arr, o =>
                        {
                            h.TD_().T(o.name).SP().SUB(o.tel)._TD();
                            h.TD(User.Admly[o.admly]);
                            h.TDFORM(() => h.TOOL(nameof(access), caption: "✕", subscript: 2));
                        },
                        caption: "现有操作权限"
                    );
                    h.FORM_().FIELDSUL_("指定用户");
                    if (cmd == 0)
                    {
                        h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(access), 1, post: false)._LI();
                    }
                    else if (cmd == 1) // search user
                    {
                        // using var dc = NewDbContext();
                        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                        var o = dc.QueryTop<User>(p => p.Set(tel));
                        if (o != null)
                        {
                            h.HIDDEN(nameof(o.id), o.id);
                            h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(access), 1, post: false)._LI();
                            h.LI_().FIELD("用户姓名", o.name)._LI();
                            h.LI_().SELECT("权限", nameof(admly), admly, User.Admly, filter: (k, v) => k > 0)._LI();
                            h.BUTTON("确认", nameof(access), 2)._BOTTOMBAR();
                        }
                    }
                    else
                    {
                    }
                    h._FIELDSUL()._FORM();
                }, false, 3);
            }
            else
            {
                var f = await wc.ReadAsync<Form>();
                int id = f[nameof(id)];
                admly = f[nameof(admly)];
                // using var dc = NewDbContext();
                dc.Execute("UPDATE users SET admly = @1 WHERE id = @2", p => p.Set(admly).Set(id));
                wc.GivePane(200); // ok
            }
        }
    }
}