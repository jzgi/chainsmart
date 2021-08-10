using System.Threading.Tasks;
using SkyChain;
using SkyChain.Db;
using SkyChain.Web;
using Zhnt.Market;
using static System.String;
using static SkyChain.Web.Modal;
using static Zhnt.ZhntUtility;

namespace Zhnt
{
    [UserAuthorize]
    [Ui("账号信息")]
    public class MyVarWork : WebWork
    {
        protected override void OnMake()
        {
            MakeWork<MyRoWork>("order");

        }

        [UserAuthorize]
        public async Task @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE id = @1");
            var o = dc.QueryTop<User>(p => p.Set(prin.id));

            // refresh cookie
            wc.SetTokenCookie(o);

            var doing = await dc.SeekQueueAsync(prin.acct);
            var done = await dc.SeekArchiveAsync(1, prin.acct);

            wc.GivePage(200, h =>
            {
                // h.TOOLBAR(caption: "我的账号信息");

                h.FORM_("uk-card uk-card-default");
                h.HEADER_("uk-card-header").T("基本信息").SPAN("我的账号信息", "uk-badge uk-badge-secondary")._HEADER();
                h.UL_("uk-card-body");
                h.LI_().FIELD("姓名", o.name)._LI();
                h.LI_().FIELD("手机号码", o.tel)._LI();
                h.LI_().FIELD("专业验证", User.Typs[o.typ])._LI();
                h._UL();
                h.FOOTER_("uk-card-footer uk-flex-right").TOOL(nameof(setg), css: "uk-button-secondary")._FOOTER();
                h._FORM();

                h.FORM_("uk-card uk-card-default");
                h.HEADER_("uk-card-header").T("时间账号").SPAN_("uk-badge uk-badge-secondary");
                if (o.IsCertified)
                {
                    h.T(o.acct);
                }
                else
                {
                    h.TOOL(nameof(open), css: "uk-button-secondary");
                }
                h._SPAN()._HEADER();
                if (o.IsCertified)
                {
                    h.UL_("uk-card-body");
                    foreach (var o in doing)
                    {
                        h.LI_("uk-flex");
                        h.SPAN_("uk-width-1-4").T(o.Stamp, 3, 0)._SPAN();
                        h.SPAN(o.Remark, "uk-width-expand");
                        h.SPAN_("uk-width-1-4").T(o.Amt)._SPAN();
                        h._LI();
                    }
                    h._UL();

                    h.FOOTER_("uk-card-footer uk-flex-right").DIV_("uk-button-group");
                    h.TOOL(nameof(give), css: "uk-button-secondary");
                    h._DIV()._FOOTER();
                }
                h._FORM();

                h.SECTION_("uk-section");
                h.SPAN_("uk-flex uk-flex-center").TOOL(nameof(plat))._SPAN();
                h._SECTION();
            }, false, 3, title: "我的账号管理");
        }

        [Ui("设置", group: 1), Tool(ButtonShow)]
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
                password = IsNullOrEmpty(prin.credential) ? null : PASSMASK;
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");
                    if (!prin.IsCertified)
                    {
                        h.LI_().TEXT("姓名", nameof(name), name, max: 8, min: 2, required: true)._LI();
                    }
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h._FIELDSUL().FIELDSUL_("操作密码（可选）");
                    h.LI_().PASSWORD("密码", nameof(password), password, max: 12, min: 3)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                name = f[nameof(name)];
                tel = f[nameof(tel)];
                password = f[nameof(password)];
                string credential =
                    IsNullOrEmpty(password) ? null :
                    password == PASSMASK ? prin.credential :
                    ComputeCredential(tel, password);

                using var dc = NewDbContext();
                dc.Sql("UPDATE users SET name = CASE WHEN @1 IS NULL THEN name ELSE @1 END , tel = @2, credential = @3 WHERE id = @4 RETURNING ").collst(User.Empty);
                prin = await dc.QueryTopAsync<User>(p => p.Set(name).Set(tel).Set(credential).Set(prin.id));
                // refresh cookie
                wc.SetTokenCookie(prin);
                wc.GivePane(200); // close
            }
        }

        static readonly WebClient Api = new WebClient("https://idcert.market.alicloudapi.com");

        // id cert appcode
        static readonly string idcertcode;

        static MyVarWork()
        {
            ServerEnviron.extcfg.Get(nameof(idcertcode), ref idcertcode);
        }

        [Ui("开通", group: 1), Tool(ButtonOpen)]
        public async Task open(WebContext wc)
        {
            var prin = (User) wc.Principal;
            if (prin.IsCertified)
            {
                wc.Give(200);
                return;
            }
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_();
                    h.FIELDSUL_("时间银行要求真实信息");
                    h.LI_().TEXT("姓名", nameof(prin.name), prin.name, max: 12, min: 2, required: true)._LI();
                    h.LI_().TEXT("身份证号", nameof(prin.acct), prin.acct, pattern: "[0-9]+", max: 18, min: 18, required: true)._LI();
                    h._FIELDSUL();
                    h.BOTTOMBAR_().BUTTON("验证", nameof(open))._BOTTOMBAR();
                    h._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                string name = f[nameof(name)];
                string acct = f[nameof(acct)];
                var (_, mdl) = await Api.GetAsync<JObj>("/idcard?idCard=" + acct + "&name=" + name, "APPCODE " + idcertcode);
                if (mdl != null)
                {
                    string status = null;
                    mdl.Get(nameof(status), ref status);
                    if (status == "01") // success
                    {
                        using var dc = NewDbContext();
                        await dc.ExecuteAsync("UPDATE users SET name = @1, acct = @2 WHERE id = @3", p => p.Set(name).Set(acct).Set(prin.id));
                        wc.SetTokenCookie(prin);
                        wc.GivePane(200); // close dialog
                        return;
                    }
                }
                wc.GivePane(200, h => { h.ALERT("&#9940; 认证失败，请提供正确信息后重试"); });
            }
        }


        [Ui("转送"), Tool(ButtonOpen)]
        public async Task give(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;

            string acct = null;
            string name = null;
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();
                h.LI_().FIELD("我的账号", prin.acct)._LI();
                h.LI_().FIELD("姓名", prin.name)._LI();
                h.LI_().TEXT("转入账号", nameof(acct), acct, pattern: "[0-9]+", min: 18, max: 18, tip: "收方账号")._LI();
                h.LI_().TEXT("姓名", nameof(name), name, min: 2, max: 8, tip: "收方姓名")._LI();
                h._FIELDSUL()._FORM();
            }, false, 3);
        }

        [Ui("推荐穹苍家园", group: 1), Tool(AnchorOpen, Appear.Small)]
        public void plat(WebContext wc)
        {
            wc.GivePane(200, h =>
            {
                h.DIV_("uk-flex uk-flex-center");
                h.PIC("/qrcode.jpg");
                h._DIV();
            });
        }
    }
}