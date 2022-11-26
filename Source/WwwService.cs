using System;
using System.Threading.Tasks;
using System.Web;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainMart.WeixinUtility;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    [UserAuthenticate]
    public class WwwService : MainService
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyVarWork>(); // home for market

            CreateWork<PublyTagWork>("tag");

            CreateWork<PublyLotWork>("lot");

            CreateWork<PublyOrgWork>("org");

            CreateWork<PublyItemWork>("item");

            CreateWork<MyWork>("my");
        }


        /// <summary>
        /// To show the market list.
        /// </summary>
        public void @default(WebContext wc)
        {
            var topOrgs = Grab<int, Org>();
            var regs = Grab<short, Reg>();

            wc.GivePage(200, h =>
            {
                h.FORM_();

                bool exist = false;
                var last = 0;

                for (int i = 0; i < topOrgs.Count; i++)
                {
                    var o = topOrgs.ValueAt(i);
                    if (!o.IsMarket)
                    {
                        continue;
                    }

                    if (o.regid != last)
                    {
                        h._LI();
                        if (last != 0)
                        {
                            h._FIELDSUL();
                        }
                        h.FIELDSUL_(regs[o.regid]?.name);
                    }

                    h.LI_("uk-flex");
                    h.T("<a class=\"uk-width-expand\" href=\"").T(o.id).T("/\" id=\"").T(o.id).T("\" onclick=\"return markgo('mktid', this);\" cookie=\"mktid\" onenhance=\"setactive(event, this)\">").T(o.name)._A();
                    h.SPAN_("uk-margin-auto-left");
                    h.SPAN(o.addr, css: "uk-width-auto uk-text-small uk-padding-small-right");
                    h.A_POI(o.x, o.y, o.name, o.addr, o.Tel, o.x > 0 && o.y > 0)._SPAN();
                    h._LI();

                    exist = true;
                    last = o.regid;
                }
                h._FIELDSUL();
                if (!exist)
                {
                    h.LI_().T("（暂无市场）")._LI();
                }
                h._FORM();
            }, true, 900, title: Self.Name, onload: "enhanceAll()");
        }

        public void @catch(WebContext wc)
        {
            var e = wc.Error;
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
                dc.Sql("SELECT price FROM orders WHERE id = @1 AND status = ").T(STA_VOID);
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
                var x = new XmlBuilder(true, 1024);
                x.ELEM("xml", null, () =>
                {
                    x.ELEM("return_code", "SUCCESS");
                    x.ELEM("return_msg", "OK");
                });
                wc.Give(200, x);
            }
        }


        public async Task onpay(WebContext wc)
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
                dc.Sql("SELECT price FROM orders WHERE id = @1 AND status = ").T(STA_VOID);
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
                var x = new XmlBuilder(true, 1024);
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