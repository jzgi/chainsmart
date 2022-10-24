using System;
using System.Threading.Tasks;
using System.Web;
using ChainFx;
using ChainFx.Fabric;
using ChainFx.Web;
using static ChainFx.Entity;

namespace ChainMart
{
    public abstract class ChainMartService : WebService
    {
        public async Task signin(WebContext wc)
        {
            string tel = null;
            string password = null;
            string url;
            if (wc.IsGet)
            {
                wc.GivePage(200, h =>
                {
                    url = wc.Query[nameof(url)];
                    url = HttpUtility.UrlDecode(url);

                    h.FORM_("uk-card uk-card-primary uk-card-body");

                    h.FIELDSUL_("通过预留密码登录");
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h.LI_().PASSWORD("密码", nameof(password), password, max: 12, min: 3)._LI();
                    h._FIELDSUL();

                    h.HIDDEN(nameof(url), url);
                    h.BOTTOMBAR_().BUTTON("登录", nameof(signin))._BOTTOMBAR();
                    h._FORM();
                }, title: "用户登录");
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                tel = f[nameof(tel)];
                password = f[nameof(password)];
                url = f[nameof(url)];

                using var dc = Nodality.NewDbContext();
                var credential = ChainMartUtility.ComputeCredential(tel, password);
                dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                var prin = dc.QueryTop<User>(p => p.Set(tel));
                if (prin == null || !credential.Equals(prin.credential))
                {
                    wc.GiveRedirect(nameof(signin));
                    return;
                }

                // successfully signed in
                wc.Principal = prin;
                wc.SetTokenCookie(prin);
                wc.GiveRedirect(url ?? "/");
            }
        }

        public async Task signup(WebContext wc)
        {
            // must have openid
            string openid = wc.Cookies[nameof(openid)];
            if (openid == null)
            {
                wc.Give(400); // bad request
                return;
            }

            string name = null;
            string tel = null;
            string vcode = null; // verification code
            string url;
            if (wc.IsGet) // GET
            {
                wc.GivePage(200, h =>
                {
                    url = wc.Query[nameof(url)];
                    url = HttpUtility.UrlDecode(url);

                    h.FORM_("uk-card uk-card-primary uk-card-body");

                    h.FIELDSUL_("填写新账号信息");
                    h.LI_().TEXT("姓名", nameof(name), name, max: 10, min: 2, required: true)._LI();
                    h.LI_().LABEL("手机号").TEXT(null, nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("获取验证码", onclick: "", css: "uk-button-small")._LI();
                    h.LI_().TEXT("验证码", nameof(vcode), vcode, tip: "填写收到的验证码", pattern: "[0-9]+", max: 4, min: 4, required: true)._LI();
                    h._FIELDSUL();

                    h.HIDDEN(nameof(url), url);

                    h.BOTTOMBAR_().BUTTON("注册", nameof(signup))._BOTTOMBAR();
                    h._FORM();
                }, title: "注册新用户");
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                url = f[nameof(url)];
                vcode = f[nameof(vcode)];
                var m = new User
                {
                    typ = 0,
                    status = STU_NORMAL,
                    name = f[nameof(name)],
                    tel = f[nameof(tel)],
                    im = openid,
                    created = DateTime.Now,
                    creator = f[nameof(name)]
                };

                // check
                if (vcode != CryptoUtility.GetVCode(m.tel, Application.Secret))
                {
                    wc.GivePage(200, h =>
                    {
                        url = wc.Query[nameof(url)];
                        url = HttpUtility.UrlDecode(url);

                        h.FORM_("uk-card uk-card-primary uk-card-body");

                        h.FIELDSUL_("填写新账号信息");
                        h.LI_().TEXT("姓名", nameof(name), name, max: 10, min: 2, required: true)._LI();
                        h.LI_().LABEL("手机号").TEXT(null, nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("获取验证码", onclick: "", css: "uk-button-small")._LI();
                        h.LI_().TEXT("验证码", nameof(vcode), vcode, tip: "填写收到的验证码", pattern: "[0-9]+", max: 4, min: 4, required: true)._LI();
                        h._FIELDSUL();

                        h.HIDDEN(nameof(url), url);

                        h.BOTTOMBAR_().BUTTON("注册", nameof(signup))._BOTTOMBAR();
                        h._FORM();
                    }, title: "注册新用户");
                }
                else
                {
                    // insert or update
                    using var dc = Nodality.NewDbContext();
                    const short msk = MSK_BORN | MSK_EDIT;
                    dc.Sql("INSERT INTO users ").colset(m, msk)._VALUES_(m, msk).T(" ON CONFLICT (tel) DO UPDATE SET im = @1 RETURNING ").collst(User.Empty);
                    m = await dc.QueryTopAsync<User>(p =>
                    {
                        m.Write(p);
                        p.Set(openid);
                    });

                    // refresh cookie
                    wc.Principal = m;
                    wc.SetTokenCookie(m);
                    wc.GiveRedirect(url);
                }
            }
        }
    }
}