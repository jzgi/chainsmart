using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainSmart.CloudUtility;
using static ChainFX.Nodal.Storage;

namespace ChainSmart;

public class WwwService : MainService
{
    protected override void OnCreate()
    {
        CreateVarWork<PublyVarWork>();

        CreateWork<PublyCatWork>("cat");

        CreateWork<PublyTagWork>("tag");

        CreateWork<PublySymWork>("sym");

        CreateWork<PublyOrgWork>("org");

        CreateWork<PublyItemWork>("item");

        CreateWork<PublyBatWork>("bat");

        CreateWork<MyWork>("my");
    }


    /// <summary>
    /// To show a list of all market, or a brief intro of one specified market.
    /// </summary>
    /// <param name="wc"></param>
    /// <param name="mktid">the market to show if greater than 0</param>
    public void @default(WebContext wc, int mktid)
    {
        var regs = Grab<short, Reg>();

        if (mktid > 0) // to show an intermediate dialog
        {
            var mkt = GrabTwin<int, Org>(mktid);
            bool inner = wc.Query[nameof(inner)]; // whether in an intermediate dialog or a full page

            wc.GivePage(200, h =>
            {
                lock (mkt)
                {
                    // brief of market
                    //
                    h.ARTICLE_("uk-card uk-card-primary");
                    h.HEADER_("uk-card-header").H3(mkt.whole).SPAN_("uk-badge").IMG("/logo.jpg", css: "uk-width-micro")._SPAN()._HEADER();

                    h.SECTION_("uk-card-body");
                    h.PIC_("/org/", mkt.id, "/img");
                    h._PIC();
                    h._SECTION();

                    h.FOOTER_("uk-card-footer").T(mkt.wholetip)._FOOTER();
                    h._ARTICLE();

                    // geolocation map

                    h.ARTICLE_("uk-card uk-card-primary");
                    h.HEADER_("uk-card-header").H3("地图定位").SPAN_("uk-badge").IMG("/logo.jpg", css: "uk-width-micro")._SPAN()._HEADER();

                    h.SECTION_("uk-card-body");
                    h.T("<iframe class=\"uk-card-body uk-height-large\" src=\"http://apis.map.qq.com/uri/v1/marker?marker=coord:").T(mkt.y).T(',').T(mkt.x).T(";title:").T(mkt.whole).T(";addr:").T(mkt.addr).T("\">");
                    h.T("</iframe>");
                    h._SECTION();

                    h._ARTICLE();

                    h.ARTICLE_("uk-card uk-card-primary");
                    h.HEADER_("uk-card-header").H3("关注公众号").SPAN_("uk-badge").IMG("/logo.jpg", css: "uk-width-micro")._SPAN()._HEADER();

                    h.SECTION_("uk-section uk-card-body uk-background-muted ");
                    h.DIV_("uk-width-expand uk-col uk-flex-center").SPAN("扫码关注「中惠农通」公众号").SPAN("更多农贸市场，更多农副产品").SPAN("幸福生活从健康餐桌开始！")._DIV();
                    h.ICON("triangle-right", 2, css: "uk-width-auto");
                    h.PIC("qrcode.jpg", css: "uk-width-medium");
                    h._SECTION();
                    h.FOOTER_("uk-card-footer")._FOOTER();

                    h._ARTICLE();


                    h.BOTTOMBAR_().A_(mkt.id, "/h", parent: inner, css: "uk-button uk-button-default").T("进入市场")._A()._BOTTOMBAR();
                }
            }, true, 720, Application.Nodal.name, dark: false);
        }
        else // to show the full market list
        {
            var mkts = GrabTwinArray<int, Org>(0, x => x.IsRtlEst && x.IsOked);

            wc.GivePage(200, h =>
            {
                bool found = false;
                var last = 0;

                string tel = Application.CustomConfig[nameof(tel)];

                h.TOPBAR_();
                h.HEADER_("uk-flex-center").H2_().T("全省热线&nbsp;").A_TEL(tel, tel, css: "uk-button-link")._H2()._HEADER();
                h._TOPBAR();

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

                        h.ARTICLE_("uk-card uk-card-primary");
                        h.H3(regs[o.regid]?.name, "uk-card-header");
                        h.UL_("uk-card-body uk-list uk-list-divider");
                    }

                    h.LI_("uk-flex");
                    h.T("<a class=\"uk-width-expand uk-link\" href=\"").T(o.id).T("\" id=\"").T(o.id).T("\" onclick=\"markAndGo('mktid', this); return dialog(this,16,false,'").T(o.whole).T("');\" cookie=\"mktid\" onfix=\"setActive(event, this)\">");
                    h.SPAN(o.Full);
                    h.P(o.addr, css: "uk-margin-auto-left");
                    h.ICON("chevron-right");
                    h._A();
                    h._LI();

                    found = true;
                    last = o.regid;
                }

                h._UL();

                if (!found)
                {
                    h.LI_().T("（暂无市场）")._LI();
                }

                h._ARTICLE();

                h.BOTTOMBAR_(css: "uk-background-muted uk-padding");
                h.A_(href: "https://beian.miit.gov.cn/", css: "uk-text-small").T("赣ICP备2022006974号-1")._A();
                h._BOTTOMBAR();
            }, true, 720, title: Application.Nodal.name, onload: "fixAll();");
        }
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

            if (await dc.QueryTopAsync("SELECT orgid, topay FROM buys WHERE id = @1 AND status = -1", p => p.Set(buyid)))
            {
                dc.Let(out int orgid);
                dc.Let(out decimal topay);

                if (topay == cash) // verify that the ammount is correct
                {
                    dc.Sql("UPDATE buys SET status = 1, pay = @1 WHERE id = @2 AND status = -1 RETURNING *");
                    var o = await dc.QueryTopAsync<Buy>(p => p.Set(cash).Set(buyid));

                    // add to watch set and buy set
                    //
                    var shp = GrabTwin<int, Org>(orgid);
                    var mkt = GrabTwin<int, Org>(shp.MktId);
                    shp.WatchSet.Put(OrgWatchAttribute.BUY_CREATED, 1, cash);
                    (shp.trust ? mkt : shp).BuySet.Add(o);
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