using System.Threading.Tasks;
using CoChain;
using CoChain.Nodal;
using CoChain.Web;
using static CoChain.Nodal.Store;

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

            CreateWork<AdmlySupplyDailyWork>("sdaily");

            CreateWork<AdmlyBuyDailyWork>("bdaily");

            CreateWork<AdmlySupplyClearWork>("sclear");

            CreateWork<AdmlyBuyClearWork>("bclear");

            CreateWork<AdmlyUserWork>("user");

            CreateWork<AdmlyNoteWork>("note");

            CreateWork<FedMgtWork>("fed", authorize: new UserAuthorizeAttribute(admly: User.ADMLY_MGT));
        }

        public void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            var o = Self;
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: prin.name + "（" + wc.Role + "）");

                h.FORM_("uk-card uk-card-default");
                h.UL_("uk-card-body uk-list uk-list-divider");
                if (o != null)
                {
                    h.LI_().FIELD("平台名称", o.Name)._LI();
                    h.LI_().FIELD("描述", o.Tip)._LI();
                }
                h._UL();
                h._FORM();

                h.TASKLIST();
            });
        }

        [UserAuthorize(admly: User.ADMLY_OPN)]
        [Ui("运行参数"), Tool(Modal.ButtonOpen)]
        public async Task setg(WebContext wc, int cmd)
        {
            var ext = Application.Prog;

            decimal rtlpct = ext[nameof(rtlpct)];
            decimal suppct = ext[nameof(suppct)];
            wc.GivePage(200, h =>
            {
                h.FORM_().FIELDSUL_("运维参数需运维技术员调整");
                h.LI_().FIELD("零售业务扣点", rtlpct)._LI();
                h.LI_().FIELD("供应业务扣点", suppct)._LI();
                h.LI_()._LI();
                h.LI_()._LI();
                h._FIELDSUL()._FORM();
            });
        }

        [UserAuthorize(admly: User.ADMLY_OPN)]
        [Ui("操作权限"), Tool(Modal.ButtonOpen)]
        public async Task acl(WebContext wc, int cmd)
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
                                h.TOOL(nameof(acl), caption: "✕", subscript: 2, tool: ToolAttribute.BUTTON_CONFIRM, css: "uk-button-secondary");
                            });
                        },
                        caption: "现有操作权限"
                    );
                    h.FORM_().FIELDSUL_("授权目标用户");
                    if (cmd == 0)
                    {
                        h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(acl), 1, post: false, css: "uk-button-secondary")._LI();
                    }
                    else if (cmd == 1) // search user
                    {
                        // get user by tel
                        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                        var o = dc.QueryTop<User>(p => p.Set(tel));
                        if (o != null)
                        {
                            h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(acl), 1, post: false, css: "uk-button-secondary")._LI();
                            h.LI_().FIELD("用户姓名", o.name)._LI();
                            h.LI_().SELECT("权限", nameof(admly), admly, User.Admly, filter: (k, v) => k > 0, required: true)._LI();
                            h.LI_("uk-flex uk-flex-center").BUTTON("确认", nameof(acl), 2)._LI();
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
                wc.GiveRedirect(nameof(acl));
            }
        }
    }
}