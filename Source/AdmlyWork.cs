using System.Threading.Tasks;
using SkyChain;
using SkyChain.Chain;
using SkyChain.Web;

namespace Revital
{
    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("供应平台管理")]
    public class AdmlyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeWork<AdmlyRegWork>("reg");

            MakeWork<AdmlyOrgWork>("org");

            MakeWork<AdmlyItemWork>("item");

            MakeWork<AdmlyClearWork>("clear");

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
                h.UL_("uk-card-body");
                if (o != null)
                {
                    h.LI_().FIELD("系统名称", o.Name)._LI();
                    h.LI_().FIELD("描述", o.Tip)._LI();
                    h.LI_().FIELD("连接地址", o.Uri)._LI();
                }
                h._UL();
                h._FORM();

                h.TASKUL();
            });
        }

        [Ui("运行设置"), Tool(Modal.ButtonShow)]
        public async Task setg(WebContext wc)
        {
            var org = wc[0].As<Org>();
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("修改基本设置");
                    h.LI_().TEXT("标语", nameof(org.tip), org.tip, max: 16)._LI();
                    h.LI_().TEXT("地址", nameof(org.addr), org.addr, max: 16)._LI();
                    h.LI_().SELECT("状态", nameof(org.status), org.status, _Article.Statuses, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else
            {
                var o = await wc.ReadObjectAsync(inst: org); // use existing object
                using var dc = NewDbContext();
                // update the db record
                await dc.ExecuteAsync("UPDATE orgs SET tip = @1, cttid = CASE WHEN @2 = 0 THEN NULL ELSE @2 END, status = @3 WHERE id = @4",
                    p => p.Set(o.tip).Set(o.status).Set(org.id));

                wc.GivePane(200);
            }
        }


        [UserAuthorize(admly: User.ADMLY_OP)]
        [Ui("操作权限"), Tool(Modal.ButtonOpen)]
        public async Task access(WebContext wc, int cmd)
        {
            short commid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE admly > 0");
            var arr = dc.Query<User>(p => p.Set(commid));
            short admly = 0;

            if (wc.IsGet)
            {
                string tel = wc.Query[nameof(tel)];

                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    h.TABLE(arr, o =>
                        {
                            h.TDCHECK(o.id);
                            h.TD(o.tel);
                            h.TD(o.name)._TD();
                            h.TD(User.Admly[o.admly]);
                            h.TDFORM(() => h.VARTOOLS(o.Key));
                        }
                    );
                    if (cmd == 1)
                    {
                        h.FORM_().FIELDSUL_("授权给指定用户");
                        h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(access), 1, post: false)._LI();
                        h._FIELDSUL();
                        if (cmd == 1) // search user
                        {
                            // using var dc = NewDbContext();
                            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                            var o = dc.QueryTop<User>(p => p.Set(tel));
                            if (o != null)
                            {
                                h.FIELDSUL_();
                                h.HIDDEN(nameof(o.id), o.id);
                                h.LI_().FIELD("用户姓名", o.name)._LI();
                                h.LI_().SELECT("权限", nameof(admly), admly, User.Admly, filter: (k, v) => k > 0)._LI();
                                h._FIELDSUL();
                                h.BOTTOMBAR_().BUTTON("确认", nameof(access), 2)._BOTTOMBAR();
                            }
                        }
                        h._FORM();
                    }
                    else
                    {
                    }
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