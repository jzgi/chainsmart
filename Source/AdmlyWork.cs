using System.Threading.Tasks;
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

            CreateWork<AdmlyPrvRptWork>("prpt");

            CreateWork<AdmlyBuyRptWork>("brpt");


            CreateWork<AdmlyPrvClearWork>("pclr");

            CreateWork<AdmlyBuyClearWork>("bclr");


            CreateWork<AdmlyAccessWork>("access");

            CreateWork<AdmlyDatWork>("dat");

            CreateWork<AdmlyNodWork>("chain");
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
            });
        }
    }

    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("数据维护", "系统")]
    public class AdmlyDatWork : WebWork
    {
    }

    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("联盟链网络", "系统")]
    public class AdmlyNodWork : NodeWork
    {
    }
}