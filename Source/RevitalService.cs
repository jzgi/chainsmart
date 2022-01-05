using System;
using System.Threading.Tasks;
using System.Web;
using SkyChain;
using SkyChain.Db;
using SkyChain.Web;
using static Revital.WeChatUtility;

namespace Revital
{
    [UserAuthenticate]
    public class RevitalService : ChainService
    {
        protected override void OnMake()
        {
            MakeVarWork<RevitalVarWork>(); // org's home page

            MakeWork<PublyPostWork>("post");

            MakeWork<PublyItemWork>("item");

            MakeWork<PublySrcWork>("code");

            // management

            MakeWork<AdmlyWork>("admly"); // platform admin

            MakeWork<PrvlyWork>("prvly"); // for source and oroducer

            MakeWork<CtrlyWork>("ctrly"); // for center

            MakeWork<MrtlyWork>("mrtly"); // for market and biz

            MakeWork<MyWork>("my");
        }

        public void @default(WebContext wc, int cmd)
        {
            var orgs = ObtainMap<int, Org>();
            var regs = ObtainMap<short, Reg>();
            int mrtid = wc.Query[nameof(mrtid)];
            if (cmd == 0)
            {
                mrtid = wc.Cookies[nameof(mrtid)].ToInt();
                if (mrtid == 0)
                {
                    mrtid = wc.Cookies[nameof(mrtid)].ToShort();
                }
                wc.GivePane(200, h =>
                {
                    h.FORM_();
                    if (mrtid > 0)
                    {
                        var o = orgs[mrtid];
                        h.FIELDSUL_("我默认的市场").LI_();
                        h.SPAN_("uk-width-expand").RADIO(nameof(mrtid), o.id, o.name, true, tip: o.addr, required: true)._SPAN();
                        h.SPAN_("uk-badge").A_POI(o.x, o.y, o.name, o.addr)._SPAN();
                        h._LI()._FIELDSUL();
                    }
                    h.FIELDSUL_("选择就近的市场");
                    bool exist = false;
                    for (int i = 0; i < orgs.Count; i++)
                    {
                        var o = orgs.ValueAt(i);
                        exist = true;
                        h.LI_("uk-flex");
                        h.SPAN_("uk-width-expand").RADIO(nameof(mrtid), o.id, o.name, false, tip: o.addr, required: true).SP().SP().A_TEL("☏", o.Tel, css: "uk-small")._SPAN();
                        // h.A_TEL_ICON(o.name, o.Tel);
                        h.SPAN_("uk-margin-auto-left");
                        if (o.x > 0 && o.y > 0)
                        {
                            h.A_POI(o.x, o.y, o.name, o.addr, o.Tel);
                        }
                        else
                        {
                            h.T("<span class=\"uk-icon-link uk-inactive\" uk-icon=\"location\"></span>");
                        }
                        h._SPAN();
                        h._LI();
                    }
                    if (!exist)
                    {
                        h.LI_().T("（暂无市场）")._LI();
                    }
                    h._FIELDSUL();
                    h.BOTTOMBAR_().BUTTON("确定", string.Empty, subscript: 1, post: false)._BOTTOMBAR();
                    h._FORM();
                }, false, 15);
            }
            else if (cmd == 1) // agreement
            {
                const bool agree = true;
                var u = mrtid.ToString();
                wc.SetCookie(nameof(mrtid), mrtid.ToString(), maxage: 3600 * 300);
                wc.GiveRedirect("/" + u + "/");
            }
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
            string url;
            if (wc.IsGet) // GET
            {
                wc.GivePage(200, h =>
                {
                    url = wc.Query[nameof(url)];
                    url = HttpUtility.UrlDecode(url);

                    h.FORM_().FIELDSUL_("注册信息");
                    h.LI_().TEXT("姓名", nameof(name), name, max: 10, min: 2, required: true);
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h._FIELDSUL();
                    h.HIDDEN(nameof(url), url);

                    h.SECTION_("uk-section uk-col uk-flex-middle");
                    h.P("如需接受消息通知清关注");
                    h.PIC("/qrcode.jpg", css: "uk-width-medium");
                    h._SECTION();

                    h.BOTTOMBAR_().BUTTON("注册", nameof(signup))._BOTTOMBAR();
                    h._FORM();
                }, title: "用户注册");
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                url = f[nameof(url)];
                var o = new User
                {
                    status = _Info.STA_ENABLED,
                    name = f[nameof(name)],
                    tel = f[nameof(tel)],
                    im = openid,
                    created = DateTime.Now,
                };
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO users ").colset(o, User.INSERT)._VALUES_(o, User.INSERT).T(" RETURNING ").collst(User.Empty);
                o = await dc.QueryTopAsync<User>(p => o.Write(p));

                // refresh cookie
                wc.Principal = o;
                wc.SetTokenCookie(o);
                wc.GiveRedirect(url);
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
                dc.Sql("SELECT price FROM orders WHERE id = @1 AND status = ").T(_Deal.STA_CREATED);
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

        /// <summary>
        /// A buying payment notification.
        /// </summary>
        public async Task onbuy(WebContext wc)
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
                dc.Sql("SELECT price FROM orders WHERE id = @1 AND status = ").T(_Deal.STA_CREATED);
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