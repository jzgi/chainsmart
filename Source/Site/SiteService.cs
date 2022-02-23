using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Store;
using SkyChain.Web;
using static Revital.WeChatUtility;

namespace Revital.Site
{
    [UserAuthenticate]
    public class SiteService : RevitalService
    {
        protected override void OnCreate()
        {
            CreateVarWork<SiteVarWork>();

            CreateWork<PublyPieceWork>("piece");

            CreateWork<AdmlyWork>("admly"); // platform admin

            CreateWork<MrtlyWork>("mrtly"); // for market and biz

            CreateWork<CtrlyWork>("ctrly"); // for center
        }

        public void @default(WebContext wc, int cmd)
        {
            var regs = Grab<short, Reg>();

            wc.GivePage(200, h =>
            {
                h.FORM_();
                h.FIELDSUL_("批发功能");
                for (int i = 0; i < regs.Count; i++)
                {
                    var r = regs.ValueAt(i);
                    if (!r.IsDist) continue;
                    h.LI_("uk-flex");
                    h.A_(r.Key, "/", end: true, css: "uk-button uk-button-link uk-flex-left").T(r.name)._A();
                    // h.P(wrk.Tip, "uk-padding uk-width-expand");
                    h._LI();
                }
                h._FIELDSUL();

                h.FIELDSUL_("管理功能");
                for (int i = 0; i < Works.Count; i++)
                {
                    var wrk = Works.ValueAt(i);
                    h.LI_("uk-flex");
                    h.A_(wrk.Key, wrk.HasVarWork ? "//" : "/", end: true, css: "uk-button uk-button-link uk-flex-left").T(wrk.Label)._A();
                    // h.P(wrk.Tip, "uk-padding uk-width-expand");
                    h._LI();
                }
                h._FIELDSUL();

                h._FORM();
            }, true, 3600, title: Home.Info.Name + "管理");
        }


        /// <summary>
        /// A booking payment notification.
        /// </summary>
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
                dc.Sql("SELECT price FROM orders WHERE id = @1 AND status = ").T(_Info.STA_DISABLED);
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