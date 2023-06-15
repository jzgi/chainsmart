using ChainFx;
using ChainFx.Web;

namespace ChainSmart;

[UserAuthenticate, AdmlyAuthorize]
[Ui("平台管理")]
public class AdmlyWork : WebWork
{
    protected override void OnCreate()
    {
        // twins

        CreateWork<AdmlyTwinWork>("twin");

        // basic 

        CreateWork<AdmlySetgWork>("setg", header: "常规");

        CreateWork<AdmlyAccessWork>("access");

        CreateWork<OrglyBuyClearWork>("pbuyclr", state: false);

        CreateWork<OrglyPurClearWork>("ppurclr", state: false);

        // biz

        CreateWork<AdmlyRegWork>("reg", header: "业务");

        CreateWork<AdmlyUserWork>("user");

        CreateWork<AdmlyOrgWork>("org");

        // fin

        CreateWork<AdmlyBuyAggWork>("buyagg", header: "财务");

        CreateWork<AdmlyPurAggWork>("puragg");

        CreateWork<AdmlyBuyClearWork>("buyclr");

        CreateWork<AdmlyPurClearWork>("purclr");
    }

    public void @default(WebContext wc)
    {
        var prin = (User)wc.Principal;

        wc.GivePage(200, h =>
        {
            h.TOPBARXL_();

            h.HEADER_("uk-width-expand uk-col uk-padding-left");
            h.H1(Application.Name);
            h.P2(prin.name, User.Orgly[wc.Role], brace: true);
            h._HEADER();

            h.PIC("/logo.webp", circle: true, css: "uk-width-small");

            h._TOPBARXL();

            h.WORKBOARD();

            h.TOOLBAR(bottom: true);
        }, false, 900);
    }
}

[Ui("基本信息和参数")]
public class AdmlySetgWork : WebWork
{
    public static readonly decimal
        rtlbasic,
        rtlfee,
        rtlpayrate,
        suppayrate;

    static AdmlySetgWork()
    {
        var jo = Application.Prog;

        jo.Get(nameof(rtlbasic), ref rtlbasic);
        jo.Get(nameof(rtlfee), ref rtlfee);
        jo.Get(nameof(rtlpayrate), ref rtlpayrate);
        jo.Get(nameof(suppayrate), ref suppayrate);
    }

    public void @default(WebContext wc)
    {
        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider uk-large");

            h.LI_().FIELD("消费派送基本费", rtlbasic)._LI();
            h.LI_().FIELD("消费每单打理费", rtlfee)._LI();
            h.LI_().FIELD("消费支付扣点", rtlpayrate)._LI();
            h.LI_().FIELD("供应支付扣点", suppayrate)._LI();
            h._UL();

            h.TOOLBAR(bottom: true);
        });
    }
}