using System;
using System.Threading.Tasks;
using System.Web;
using SkyChain;
using SkyChain.Web;

namespace Revital
{
    public abstract class RevitalService : WebService
    {
        public void @catch(WebContext wc)
        {
            var e = wc.Exception;
            if (e is WebException we)
            {
                if (we.Code == 401)
                {
                    if (wc.IsWeChat) // initiate signup
                    {
                        wc.GiveRedirect("/signup?url=" + HttpUtility.UrlEncode(wc.Url));
                    }
                    else // initiate form auth
                    {
                        wc.GiveRedirect("/signin?url=" + HttpUtility.UrlEncode(wc.Url));
                    }
                }
                else if (we.Code == 403)
                {
                    wc.GivePage(403, m => { m.ALERT("此功能需要系统授权后才能使用。", head: "⛔ 没有访问权限"); }, title: "权限保护");
                }
            }
            else
            {
                wc.Give(500, e.Message);
                Console.Write(e.StackTrace);
            }
        }

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

                    h.FORM_().FIELDSUL_("登录信息");
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

                using var dc = NewDbContext();
                var credential = RevitalUtility.ComputeCredential(tel, password);
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
    }
}