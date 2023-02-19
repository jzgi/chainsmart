using ChainFX;
using ChainFX.Nodal;
using ChainFX.Web;
using static ChainFX.Nodal.Nodality;

namespace ChainSMart
{
    [UserAuthenticate, AdmlyAuthorize(1)]
    [Ui("平台管理")]
    public class AdmlyWork : WebWork
    {
        protected override void OnCreate()
        {
            // 

            CreateWork<AdmlySetgWork>("setg");

            CreateWork<AdmlyAccessWork>("access");

            CreateWork<PtylyBuyClearWork>("pbuyclr", state: false);

            CreateWork<PtylyBookClearWork>("pbookclr", state: false);

            CreateWork<AdmlyNodeWork>("node");

            // biz

            CreateWork<AdmlyRegWork>("reg");

            CreateWork<AdmlyItemWork>("item");

            CreateWork<AdmlyUserWork>("user");

            CreateWork<AdmlyOrgWork>("org");

            // fin

            CreateWork<AdmlyBuyClearWork>("buyclr");

            CreateWork<AdmlyBookClearWork>("bookclr");

            CreateWork<AdmlyBuyAggWork>("buyagg");

            CreateWork<AdmlyBookAggWork>("bookagg");
        }

        public void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;

            wc.GivePage(200, h =>
            {
                h.TOPBARXL_();

                h.HEADER_("uk-width-expand uk-col uk-padding-left");
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
                h.LI_().FIELD("供应支付扣点", suppayrate)._LI();
                h._UL();

                h.TOOLBAR(bottom: true);
            });
        }
    }

    [AdmlyAuthorize(1)]
    [Ui("碳交易网络", "常规", icon: "social")]
    public class AdmlyNodeWork : NodeWork
    {
    }
}