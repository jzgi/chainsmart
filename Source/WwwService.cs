using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainSmart.WeChatUtility;
using static ChainFX.Nodal.Storage;

namespace ChainSmart;

public class WwwService : MainService
{
    protected override void OnCreate()
    {
        CreateVarWork<PublyVarWork>();

        CreateWork<PublyCatWork>("cat");

        CreateWork<PublyEnvWork>("env");

        CreateWork<PublyTagWork>("tag");

        CreateWork<PublySymWork>("sym");

        CreateWork<PublyOrgWork>("org");

        CreateWork<PublyItemWork>("item");

        CreateWork<PublyBatWork>("bat");

        CreateWork<MyWork>("my");
    }


    /// <summary>
    /// To show the market list.
    /// </summary>
    public void @default(WebContext wc)
    {
        var regs = Grab<short, Reg>();

        var mkts = GrabTwinArray<int, Org>(0, x => x.IsMkt && x.IsOked);

        wc.GivePage(200, h =>
        {
            bool found = false;
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

                    h.ARTICLE_("uk-card uk-card-primary");
                    h.H3(regs[o.regid]?.name, "uk-card-header");
                    h.UL_("uk-card-body uk-list uk-list-divider");
                }

                h.LI_("uk-flex");
                h.T("<a class=\"uk-width-expand uk-link\" href=\"").T(o.id).T("/h\" id=\"").T(o.id).T("\" onclick=\"markAndGo('mktid', this);\" cookie=\"mktid\" onfix=\"setActive(event, this)\">");
                h.SPAN(o.Whole);
                h.P(o.addr, css: "uk-margin-auto-left");
                h.ICON("chevron-right");
                h._A();
                h.A_POI(o.x, o.y, o.Whole, o.addr, o.Tel, o.x > 0 && o.y > 0)._SPAN();
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

            string tel = Application.CustomConfig[nameof(tel)];
            h.BOTTOMBAR_(large: true, css: "uk-col uk-flex-center uk-flex-middle");
            h.SPAN_(css: "uk-label").ICON("receiver").SP().SPAN(tel)._SPAN().A_(href: "https://beian.miit.gov.cn/", css: "uk-text-small").T("赣ICP备2022006974号-1")._A();
            h._BOTTOMBAR();
        }, true, 720, title: Application.Nodal.name + "市场", onload: "fixAll();");
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
                    dc.Sql("UPDATE buys SET status = 1, pay = @1 WHERE id = @2 AND status = -1");
                    await dc.ExecuteAsync(p => p.Set(cash).Set(buyid));

                    // put a notice
                    var shp = GrabTwin<int, Org>(orgid);
                    var mkt = GrabTwin<int, Org>(shp.MktId);

                    // notice and event
                    //
                    shp.NoticePack.Put(OrgNoticePack.BUY_CREATED, 1, cash);
                    mkt.EventPack.AddNew(shp.name);
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