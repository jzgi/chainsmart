using System;
using System.Threading.Tasks;
using System.Web;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainMart.WeChatUtility;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    [UserAuthenticate]
    public class MgtService : WebService
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyCtrVarWork>(); // provisions related to present center

            CreateWork<AdmlyWork>("admly"); // for admin

            CreateWork<PrvlyWork>("prvly"); // for provision sector / source / center

            CreateWork<MrtlyWork>("mrtly"); // for markets and shops
        }

        public void @default(WebContext wc, int cmd)
        {
            var orgs = Grab<int, Org>();

            wc.GivePage(200, h =>
            {
                h.FORM_("uk-card uk-card-default");
                h.H4("批发市场", css: "uk-card-header");
                h.UL_("uk-card-body uk-child-width-1-2", grid: true);
                for (int i = 0; i < orgs.Count; i++)
                {
                    var org = orgs.ValueAt(i);
                    if (org.IsCenter)
                    {
                        h.LI_("uk-flex").A_(org.Key, "/", css: "uk-button-link").SPAN(org.tip).ICON("triangle-right")._A()._LI();
                    }
                }
                h._UL();
                h._FORM();

                h.FORM_("uk-card uk-card-default");
                h.H4("管理模块", css: "uk-card-header");
                h.UL_("uk-card-body uk-child-width-1-2", grid: true);
                for (int i = 0; i < SubWorks.Count; i++)
                {
                    var wrk = SubWorks.ValueAt(i);
                    h.LI_("uk-flex").A_(wrk.Key, wrk.HasVarWork ? "//" : "/", css: "uk-button uk-button-link").SPAN(wrk.Label).ICON("triangle-right")._A()._LI();
                }
                h._UL();
                h._FORM();
            }, true, 3600, title: Self.Name + "管理");
        }

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
                wc.GiveMsg(500, e.Message, e.StackTrace);
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

                    h.FORM_("uk-card uk-card-body");

                    h.FIELDSUL_("填写登录信息");
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h.LI_().PASSWORD("密码", nameof(password), password, max: 12, min: 3)._LI();
                    h._FIELDSUL();

                    h.HIDDEN(nameof(url), url);
                    h.BOTTOMBAR_().BUTTON("登录", nameof(signin))._BOTTOMBAR();
                    h._FORM();
                }, title: wc.Action.Label);
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                tel = f[nameof(tel)];
                password = f[nameof(password)];
                url = f[nameof(url)];

                using var dc = NewDbContext();
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

        public async Task signup(WebContext wc, int cmd)
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

                    h.FORM_("uk-card uk-card-body");

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
                    status = STA_ENABLED,
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

                        h.FORM_("uk-card uk-card-body");

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
                    using var dc = NewDbContext();
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

        /// <summary>
        /// A booking payment notification.
        /// </summary>
        public async Task onbook(WebContext wc)
        {
            var xe = await wc.ReadAsync<XElem>();
            if (!OnNotified(xe, out var trade_no, out var cash))
            {
                wc.Give(400);
                return;
            }
            int pos = 0;
            var orderid = trade_no.ParseInt(ref pos);
            try
            {
                // NOTE: WCPay may send notification more than once
                using var dc = NewDbContext();
                // verify that the ammount is correct
                var today = DateTime.Today;
                dc.Sql("SELECT price FROM orders WHERE id = @1 AND status = ").T(STA_DISABLED);
                var price = (decimal) dc.Scalar(p => p.Set(orderid));
                if (price == cash) // update order status and line states
                {
                    dc.Sql("UPDATE orders SET status = 1, pay = @1, issued = @2 WHERE id = @3 AND status = 0");
                    await dc.ExecuteAsync(p => p.Set(cash).Set(today).Set(orderid));
                }
                else // try to refund this payment
                {
                    await PostRefundAsync(trade_no, cash, cash, trade_no);
                }
            }
            finally
            {
                // return xml to WCPay server
                var x = new XmlContent(true, 1024);
                x.ELEM("xml", null, () =>
                {
                    x.ELEM("return_code", "SUCCESS");
                    x.ELEM("return_msg", "OK");
                });
                wc.Give(200, x);
            }
        }
    }
}