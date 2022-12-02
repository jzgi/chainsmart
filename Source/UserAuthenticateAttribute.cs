using System;
using System.Threading.Tasks;
using ChainFx.Fabric;
using ChainFx.Web;

namespace ChainMart
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
                var o = FromToken<User>(token);
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
                    if (openid != null)
                    {
                        return true;
                    }
                }
            }

            // wechat authenticate
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

                    (_, string openid) = await WeixinUtility.GetAccessorAsync(code);
                    if (openid == null)
                    {
                        return false;
                    }

                    // create principal
                    using var dc = Nodality.NewDbContext();
                    dc.Sql("SELECT ").collst(User.Empty).T(" FROM users_vw WHERE im = @1");
                    if (dc.QueryTop(p => p.Set(openid)))
                    {
                        var prin = dc.ToObject<User>();
                        wc.Principal = prin; // set principal for afterwrads
                        wc.SetUserCookie(prin);
                    }
                    else // keep the acquired openid thru cookie
                    {
                        wc.SetCookie(nameof(openid), openid);
                    }
                }
                else // redirect to WeiXin auth
                {
                    WeixinUtility.GiveRedirectWeiXinAuthorize(wc, MainApp.WwwUrl, false);
                    return false;
                }
            }

            return true;
        }
    }
}