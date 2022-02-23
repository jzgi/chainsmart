using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Store;
using SkyChain.Web;
using static Revital.WeChatUtility;

namespace Revital.Main
{
    [UserAuthenticate]
    public class MainService : RevitalService
    {
        protected override void OnCreate()
        {
            CreateVarWork<MainVarWork>(); // market home page

            CreateWork<PublyItemWork>("item");

            CreateWork<PublyBookWork>("code");

            CreateWork<AdmlyWork>("admly"); // platform admin

            CreateWork<MyWork>("my");
        }

        public void @default(WebContext wc, int cmd)
        {
            var orgs = Grab<int, Org>();
            var regs = Grab<short, Reg>();

            short regid = wc.Query[nameof(regid)];
            int mrtid = wc.Query[nameof(mrtid)];

            if (cmd == 0)
            {
                if (mrtid == 0)
                {
                    mrtid = wc.Cookies[nameof(mrtid)].ToInt();
                }

                wc.GivePane(200, h =>
                {
                    h.FORM_();

                    // show last-time selected
                    //
                    h.TOPBAR_();
                    if (mrtid > 0)
                    {
                        var o = orgs[mrtid];
                        PutMrt(h, o, true);
                    }
                    h._TOPBAR();

                    // output for selection
                    //
                    h.FIELDSUL_();
                    if (regid == 0)
                    {
                        regid = regs.First(v => v.IsDist).id;
                    }
                    h.LI_().SELECT(mrtid > 0 ? "其它市场" : "就近市场", nameof(regid), regid, regs, filter: (k, v) => v.IsDist, required: true, refresh: true)._LI();
                    bool exist = false;
                    for (int i = 0; i < orgs.Count; i++)
                    {
                        var o = orgs.ValueAt(i);
                        if (!o.IsMrt || o.regid != regid)
                        {
                            continue;
                        }
                        h.LI_("uk-flex");
                        PutMrt(h, o);
                        h._LI();
                        exist = true;
                    }
                    if (!exist)
                    {
                        h.LI_().T("（暂无市场）")._LI();
                    }
                    h._FIELDSUL();

                    h.BOTTOMBAR_().BUTTON("确定", string.Empty, subscript: 1, post: false)._BOTTOMBAR();
                    h._FORM();
                }, false, 15, title: Home.Info.Name);
            }
            else if (cmd == 1) // agreement
            {
                wc.SetCookie(nameof(mrtid), mrtid.ToString(), maxage: 3600 * 300);

                wc.GiveRedirect(mrtid + "/");
            }

            void PutMrt(HtmlContent h, Org o, bool selected = false)
            {
                h.SPAN_("uk-width-expand").RADIO(nameof(mrtid), o.id, o.name, selected, required: true)._SPAN();
                h.SPAN_("uk-margin-auto-left");
                h.SPAN(o.addr, css: "uk-width-auto uk-text-small uk-padding-small-right");
                h.A_POI(o.x, o.y, o.name, o.addr, o.Tel, o.x > 0 && o.y > 0)._SPAN();
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

        /// <summary>
        /// A buying payment notification.
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