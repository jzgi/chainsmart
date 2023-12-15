﻿using System;
using System.Threading.Tasks;
using System.Web;
using ChainFX.Nodal;
using ChainFX.Web;

namespace ChainSmart;

/// <summary>
/// To establish principal identity. 
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class UserAuthenticateAttribute : AuthenticateAttribute
{
    const string WXAUTH = "wxauth";

    const int MAXAGE = 3600 * 24; // 3 days


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

        if (!wc.IsWeChat) // not from weixin
        {
            // redirect to custom form authentication
            wc.GiveRedirect("/login?url=" + HttpUtility.UrlEncode(wc.Url));
            return false;
        }

        // wechat authenticate
        string openid;
        if (wc.Cookies.TryGetValue(nameof(openid), out openid)) // a previously kept openid
        {
            goto HasGotOpenId;
        }

        string state = wc.Query[nameof(state)];
        if (!WXAUTH.Equals(state)) // if not weixin auth
        {
            // redirect to WeiXin auth
            WeixinUtility.GiveRedirectWeiXinAuthorize(wc, false);
            return false;
        }

        string code = wc.Query[nameof(code)];
        if (code == null)
        {
            return false;
        }

        (_, openid) = await WeixinUtility.GetAccessorAsync(code);
        if (openid == null)
        {
            return false;
        }

        HasGotOpenId:

        // create principal
        using var dc = Nodality.NewDbContext();
        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users_vw WHERE im = @1");
        if (await dc.QueryTopAsync(p => p.Set(openid)))
        {
            var prin = dc.ToObject<User>();

            wc.Principal = prin; // set principal for afterwrads
            wc.SetTokenCookies(prin);
        }
        else // keep the acquired openid and signup
        {
            wc.SetCookie(nameof(openid), openid, MAXAGE);

            // new account
            wc.GiveRedirect("/signup?url=" + HttpUtility.UrlEncode(wc.Url));
            return false;
        }

        return true;
    }
}