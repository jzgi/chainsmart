using System;
using System.Threading.Tasks;
using SkyChain.Db;
using SkyChain.Web;

namespace Revital
{
    /// <summary>
    /// To establish principal identity. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class UserAuthenticateAttribute : AuthenticateAttribute
    {
        const string WXAUTH = "wxauth";

        public override bool Do(WebContext wc) => throw new NotImplementedException();

        public UserAuthenticateAttribute() : base(true)
        {
        }

        public override async Task<bool> DoAsync(WebContext wc)
        {
            // try to restore principal from cookie token
            string token;
            if (wc.Cookies.TryGetValue(nameof(token), out token))
            {
                var o = DecryptPrincipal<User>(token);
                if (o != null)
                {
                    wc.Principal = o;
                    return true;
                }
            }
            else
            {
                string openid;
                if (wc.Cookies.TryGetValue(nameof(openid), out openid))
                {
                    if (openid != null) return true;
                }
            }

            // wechat authenticate
            User prin;
            if (wc.IsWeChat) // weixin
            {
                string state = wc.Query[nameof(state)];
                if (WXAUTH.Equals(state)) // if weixin auth
                {
                    string code = wc.Query[nameof(code)];
                    if (code == null)
                    {
                        return false;
                    }

                    (_, string openid) = await WeChatUtility.GetAccessorAsync(code);
                    if (openid == null)
                    {
                        return false;
                    }

                    // create principal
                    using var dc = Chain.NewDbContext();
                    dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE im = @1");
                    if (dc.QueryTop(p => p.Set(openid)))
                    {
                        prin = dc.ToObject<User>();
                        wc.Principal = prin; // set principal for afterwrads
                        wc.SetTokenCookie(prin);
                    }
                    else // keep the acquired openid thru cookie
                    {
                        wc.SetCookie(nameof(openid), openid);
                    }
                }
                else // redirect to WeiXin auth
                {
                    WeChatUtility.GiveRedirectWeiXinAuthorize(wc, WeChatUtility.url, false);
                    return false;
                }
            }

            return true;
        }
    }
}