using ChainFx;
using ChainFx.Fabric;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("平台管理")]
    public class AdmlyWork : WebWork
    {
        protected override void OnCreate()
        {
            CreateWork<AdmlyRegWork>("reg");

            CreateWork<AdmlyOrgWork>("org");

            CreateWork<AdmlyUserWork>("user");

            CreateWork<AdmlyBuyRptWork>("buyrpt");

            CreateWork<AdmlyBookRptWork>("bookrpt");


            CreateWork<AdmlyBuyClearWork>("buyclr");

            CreateWork<AdmlyBookClearWork>("bookclr");


            CreateWork<AdmlySetgWork>("setg");

            CreateWork<AdmlyAccessWork>("access");

            CreateWork<AdmlyDataWork>("data");

            CreateWork<AdmlyNodeWork>("node");
        }

        public void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            var o = Self;
            wc.GivePage(200, h =>
            {
                h.TOPBARXL_();
                h.PIC("/logo.webp", circle: true, css: "uk-width-small");
                h.DIV_("uk-width-expand uk-col uk-padding-small-left");
                h.H2(Self.name);
                h._DIV();
                h._TOPBARXL();

                h.TASKBOARD();
            }, false, 900);
        }
    }

    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("运行参数", "系统")]
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

                h.LI_().FIELD("外卖派送基本费", rtlbasic)._LI();
                h.LI_().FIELD("外卖每单打理费", rtlfee)._LI();
                h.LI_().FIELD("零售支付扣点", rtlpayrate)._LI();
                h.LI_().FIELD("供应链支付扣点", suppayrate)._LI();
                h._UL();

                h.TOOLBAR(bottom: true);
            });
        }
    }

    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("数据维护任务", "系统")]
    public class AdmlyDataWork : WebWork
    {
    }

    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("联盟网络管理", "系统", icon: "social")]
    public class AdmlyNodeWork : NodeWork
    {
    }
}