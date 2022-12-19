using System.Threading.Tasks;
using System.Web;
using ChainFx;
using ChainFx.Web;
using static ChainMart.WeixinUtility;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    [UserAuthenticate]
    public class MgtService : MainService
    {
        protected override void OnCreate()
        {
            CreateWork<AdmlyWork>("admly"); // for admin

            CreateWork<ZonlyWork>("zonly"); // for zone / source / center

            CreateWork<MktlyWork>("mktly"); // for markets and shops
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
                    wc.GivePage(403, m => { m.ALERT(head: "⛔ 没有访问权限", p: "此功能需要系统授权后才能使用。"); }, title: "权限保护");
                }
            }
            else
            {
                wc.GiveMsg(500, e.Message, e.StackTrace);
            }
        }

        public async Task onpay(WebContext wc)
        {
            var xe = await wc.ReadAsync<XElem>();
            if (!OnNotified(SC: true, xe, out var trade_no, out var cash))
            {
                wc.Give(400);
                return;
            }
            int pos = 0;
            var bookid = trade_no.ParseInt(ref pos);
            try
            {
                // NOTE: WCPay may send notification more than once
                using var dc = NewDbContext();
                // verify that the ammount is correct
                var topay = (decimal) dc.Scalar("SELECT topay FROM books WHERE id = @1 AND status = 0", p => p.Set(bookid));
                if (topay == cash) // update data
                {
                    dc.Sql("UPDATE books SET status = 1, pay = @1 WHERE id = @2 AND status = 0");
                    await dc.ExecuteAsync(p => p.Set(cash).Set(bookid));
                }
                else // try to refund this payment
                {
                    await PostRefundAsync(SC: true, trade_no, cash, cash, trade_no);
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