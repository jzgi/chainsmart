using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainSmart.WeixinUtility;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public class WwwService : MainService
{
    protected override void OnCreate()
    {
        CreateVarWork<PublyVarWork>(); // home for market

        CreateWork<PublyTagWork>("tag");

        CreateWork<PublyLotWork>("lot");

        CreateWork<PublyOrgWork>("org");

        CreateWork<PublySrcWork>("src");

        CreateWork<PublyItemWork>("item");

        CreateWork<MyWork>("my");
    }


    /// <summary>
    /// To show the market list.
    /// </summary>
    public void @default(WebContext wc)
    {
        var regs = Grab<short, Reg>();

        var mkts = GrabTwinSet<int, Org>(0, x => x.IsMarket);

        wc.GivePage(200, h =>
        {
            bool exist = false;
            var last = 0;

            foreach (var o in mkts)
            {
                if (o.regid != last)
                {
                    h._LI();
                    if (last != 0)
                    {
                        h._UL();
                        h._ARTICLE();
                    }

                    h.ARTICLE_("uk-card uk-card-default");
                    h.H3(regs[o.regid]?.name, "uk-card-header");
                    h.UL_("uk-card-body uk-list-divider");
                }

                h.LI_("uk-flex");
                h.T("<a class=\"uk-width-expand uk-link\" href=\"").T(o.id).T("/\" id=\"").T(o.id).T("\" onclick=\"markAndGo('mktid', this); return dialog(this,16,false);\" cookie=\"mktid\" onfix=\"setActive(event, this)\">");
                h.SPAN(o.Cover);
                h.P(o.addr, css: "uk-margin-auto-left");
                h.ICON("chevron-right");
                h._A();
                h.A_POI(o.x, o.y, o.Cover, o.addr, o.Tel, o.x > 0 && o.y > 0)._SPAN();
                h._LI();

                exist = true;
                last = o.regid;
            }

            h._UL();

            if (!exist)
            {
                h.LI_().T("（暂无市场）")._LI();
            }

            h._ARTICLE();

            string tel = Application.Program[nameof(tel)];
            h.BOTTOMBAR_("uk-flex-between uk-padding");
            h.SPAN("☎ " + tel).A_(href: "https://beian.miit.gov.cn/", css: "uk-text-small").T("赣ICP备2022006974号-1")._A();
            h._BOTTOMBAR();
        }, true, 720, title: Application.Name + "市场", onload: "fixAll();");
    }


    /**
     * The callback by the payment gateway.
     */
    public async Task onpay(WebContext wc)
    {
        var xe = await wc.ReadAsync<XElem>();

        if (!OnNotified(sup: false, xe, out var trade_no, out var cash))
        {
            wc.Give(400);
            return;
        }

        int pos = 0;
        var buyid = trade_no.ParseInt(ref pos);

        try
        {
            // NOTE: WCPay may send notification more than once
            using var dc = NewDbContext();

            if (await dc.QueryTopAsync("SELECT rtlid, topay FROM buys WHERE id = @1 AND status = -1", p => p.Set(buyid)))
            {
                dc.Let(out int rtlid);
                dc.Let(out decimal topay);

                if (topay == cash) // verify that the ammount is correct
                {
                    dc.Sql("UPDATE buys SET status = 1, pay = @1 WHERE id = @2 AND status = -1");
                    await dc.ExecuteAsync(p => p.Set(cash).Set(buyid));

                    // put a notice
                    var rtl = GrabTwin<int, Org>(rtlid);

                    rtl.Notices.Put(OrgNoticePack.BUY_CREATED, 1, cash);
                }
                else // the pay differs from the order
                {
                    // refund
                    await PostRefundAsync(sup: false, trade_no, cash, cash, trade_no, "支付金额与订单不符");
                }
            }
        }
        catch (Exception e)
        {
            Application.War(e.Message);
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