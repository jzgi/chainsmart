using System.Threading.Tasks;
using SkyChain;
using SkyChain.Chain;
using SkyChain.Web;

namespace Revital
{
    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("平台管理")]
    public class AdmlyWork : WebWork
    {
        protected override void OnCreate()
        {
            CreateWork<AdmlyRegWork>("reg");

            CreateWork<AdmlyOrgWork>("org");

            CreateWork<AdmlyItemWork>("item");

            CreateWork<AdmlyClearWork>("clear");

            CreateWork<AdmlyDailyWork>("daily");

            CreateWork<AdmlyUserWork>("user");

            CreateWork<FedWork>("fed", authorize: new UserAuthorizeAttribute(admly: User.ADMLY_MGT));
        }

        public void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            var o = Home.Self;
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: prin.name + "（" + wc.Role + "）");

                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body uk-list uk-list-divider");
                if (o != null)
                {
                    h.LI_().FIELD("平台名称", o.Name)._LI();
                    h.LI_().FIELD("描述", o.Tip)._LI();
                    h.LI_().FIELD("连接地址", o.Domain)._LI();
                }
                h._UL();
                h._FORM();

                h.TASKLIST();
            });
        }

        [UserAuthorize(admly: User.ADMLY_OPN)]
        [Ui("操作权限"), Tool(Modal.ButtonOpen)]
        public async Task access(WebContext wc, int cmd)
        {
            using var dc = NewDbContext();

            // get all adm users
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE admly > 0");
            var arr = dc.Query<User>();
            short admly = 0;
            int id = 0;

            if (wc.IsGet)
            {
                string tel = wc.Query[nameof(tel)];

                wc.GivePage(200, h =>
                {
                    h.TABLE(arr, o =>
                        {
                            h.TD(o.name);
                            h.TD(o.tel);
                            h.TD(User.Admly[o.admly]);
                            h.TDFORM(() =>
                            {
                                h.HIDDEN(nameof(id), o.id);
                                h.TOOL(nameof(access), caption: "✕", subscript: 2, tool: ToolAttribute.BUTTON_CONFIRM, css: "uk-button-secondary");
                            });
                        },
                        caption: "现有操作权限"
                    );
                    h.FORM_().FIELDSUL_("授权目标用户");
                    if (cmd == 0)
                    {
                        h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(access), 1, post: false, css: "uk-button-secondary")._LI();
                    }
                    else if (cmd == 1) // search user
                    {
                        // get user by tel
                        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                        var o = dc.QueryTop<User>(p => p.Set(tel));
                        if (o != null)
                        {
                            h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(access), 1, post: false, css: "uk-button-secondary")._LI();
                            h.LI_().FIELD("用户姓名", o.name)._LI();
                            h.LI_().SELECT("权限", nameof(admly), admly, User.Admly, filter: (k, v) => k > 0, required: true)._LI();
                            h.LI_("uk-flex uk-flex-center").BUTTON("确认", nameof(access), 2)._LI();
                            h.HIDDEN(nameof(o.id), o.id);
                        }
                    }
                    h._FIELDSUL()._FORM();
                }, false, 3);
            }
            else
            {
                var f = await wc.ReadAsync<Form>();
                id = f[nameof(id)];
                admly = f[nameof(admly)]; // lead to removal when 0
                dc.Execute("UPDATE users SET admly = @1 WHERE id = @2", p => p.Set(admly).Set(id));
                wc.GiveRedirect(nameof(access));
            }
        }
    }
}