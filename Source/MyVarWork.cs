using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static System.String;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    [UserAuthorize]
    [Ui("账号信息")]
    public class MyVarWork : WebWork
    {
        protected override void OnCreate()
        {
            CreateWork<MyInfoWork>("info");

            CreateWork<MyBuyWork>("buy");

            CreateWork<MyCreditWork>("credit");
        }

        [UserAuthorize]
        public void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            wc.GivePage(200, h =>
            {
                h.TOPBARXL_();
                if (prin.icon)
                {
                    h.PIC("/user/", prin.id, "/icon/", circle: true, css: "uk-width-medium");
                }
                else
                {
                    h.PIC("/my.webp", circle: true, css: "uk-width-small");
                }
                h.DIV_("uk-width-expand uk-col uk-padding-small-left");
                h.H2(prin.name);
                h._DIV();
                h._TOPBARXL();

                h.TASKBOARD();
            }, false, 6, title: "个人账号");
        }
    }
}