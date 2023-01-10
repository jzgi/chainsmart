using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainMart.WeixinUtility;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public class MgtService : MainService
    {
        protected override void OnCreate()
        {
            CreateWork<AdmlyWork>("admly"); // for admin

            CreateWork<SrclyWork>("srcly"); // for zone / source / center

            CreateWork<ShplyWork>("shply"); // for markets and shops
        }

        public void @default(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.FORM_().FIELDSUL_("运营管理模块");
                h.LI_().A_("admly/").T("Ａ）平台管理")._A()._LI();
                h.LI_().A_("srcly//").T("Ｂ）供区产源操作")._A()._LI();
                h.LI_().A_("shply//").T("Ｃ）市场摊铺操作")._A()._LI();
                h._FIELDSUL()._FORM();
            }, true, 3600, title: "中惠农通运营管理");
        }

        public async Task onpay(WebContext wc)
        {
            var xe = await wc.ReadAsync<XElem>();

            // For DEBUG
            // var xe = DataUtility.FileTo<XElem>("./Docs/test.xml");
            //  

            if (!OnNotified(sc: true, xe, out var trade_no, out var cash))
            {
                wc.Give(400);
                return;
            }

            // War("trade_no " + trade_no);
            // War("cash " + cash);

            int pos = 0;
            var bookid = trade_no.ParseInt(ref pos);

            // War("bookid " + bookid);

            try
            {
                // NOTE: WCPay may send notification more than once
                using var dc = NewDbContext();
                // verify that the ammount is correct
                if (await dc.QueryTopAsync("SELECT srcid, lotid, qty, topay FROM books WHERE id = @1 AND status = 0", p => p.Set(bookid)))
                {
                    dc.Let(out int srcid);
                    dc.Let(out int lotid);
                    dc.Let(out short qty);
                    dc.Let(out decimal topay);

                    // War("lotid " + lotid);
                    // War("qty " + qty);
                    // War("topay " + topay);

                    if (topay == cash) // update data
                    {
                        // the book and the lot updates
                        dc.Sql("UPDATE books SET status = 1, pay = @1 WHERE id = @2 AND status = 0; UPDATE lots SET avail = avail - @3 WHERE id = @4");
                        await dc.ExecuteAsync(p => p.Set(cash).Set(bookid).Set(qty).Set(lotid));

                        // War("update ok!");

                        // put a notice to the accepter
                        NoticeBox.Put(srcid, Notice.BOOK_CREATED, 1, cash);
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