using ChainFx;
using ChainFx.Fabric;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    [UserAuthenticate, AdmlyAuthorize(1)]
    [Ui("平台管理")]
    public class AdmlyWork : WebWork
    {
        protected override void OnCreate()
        {
            CreateWork<AdmlySetgWork>("setg");

            CreateWork<AdmlyAccessWork>("access");

            CreateWork<AdmlyNodeWork>("node");


            CreateWork<AdmlyRegWork>("reg");

            CreateWork<AdmlyUserWork>("user");

            CreateWork<AdmlyOrgWork>("org");

            CreateWork<AdmlyBuyRptWork>("buyrpt");

            CreateWork<AdmlyBookRptWork>("bookrpt");


            CreateWork<AdmlyBuyClearWork>("buyclr");

            CreateWork<AdmlyBookClearWork>("bookclr");
        }

        public void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;

            wc.GivePage(200, h =>
            {
                h.TOPBARXL_();

                h.HEADER_("uk-width-expand uk-col uk-padding-small-left");
                h.H2(Self.name);
                h.P2(prin.name, User.Admly[wc.Role], brace: true);
                h._HEADER();

                h.PIC("/logo.webp", circle: true, css: "uk-width-small");

                h._TOPBARXL();

                h.WORKBOARD();
            }, false, 900);
        }
    }

    [AdmlyAuthorize(1)]
    [Ui("基本信息和参数", "常规")]
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
                h.LI_().FIELD("供应链支付扣点", suppayrate)._LI();
                h._UL();

                h.TOOLBAR(bottom: true);
            });
        }
    }

    [AdmlyAuthorize(1)]
    [Ui("联盟网络管理", "常规", icon: "social")]
    public class AdmlyNodeWork : NodeWork
    {
    }
}