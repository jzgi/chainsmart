using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Application;
using static ChainMart.WeixinUtility;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
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
                    if (!o.EqMarket)
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
                    h.T("<a class=\"uk-width-expand\" href=\"").T(o.id).T("/\" id=\"").T(o.id).T("\" onclick=\"return markAndGo('mktid', this);\" cookie=\"mktid\" onfix=\"setActive(event, this)\">").T(o.name)._A();
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
            }, true, 900, title: Self.Name, onload: "fixAll();");
        }


        public async Task onpay(WebContext wc)
        {
            var xe = await wc.ReadAsync<XElem>();

            // For DEBUG
            // var xe = DataUtility.FileTo<XElem>("./Docs/test.xml");
            //  

            if (!OnNotified(sc: false, xe, out var trade_no, out var cash))
            {
                wc.Give(400);
                return;
            }

            // War("trade_no " + trade_no);
            // War("cash " + cash);

            int pos = 0;
            var buyid = trade_no.ParseInt(ref pos);

            // War("buyid " + buyid);

            try
            {
                // NOTE: WCPay may send notification more than once
                using var dc = NewDbContext();
                // verify that the ammount is correct
                if (await dc.QueryTopAsync("SELECT topay FROM buys WHERE id = @1 AND status = 0", p => p.Set(buyid)))
                {
                    dc.Let(out decimal topay);

                    // War("topay " + topay);

                    if (topay == cash) // update data
                    {
                        // the book and the lot updates
                        dc.Sql("UPDATE buys SET status = 1, pay = @1 WHERE id = @2 AND status = 0");
                        await dc.ExecuteAsync(p => p.Set(cash).Set(buyid));

                        // War("update ok!");
                    }
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