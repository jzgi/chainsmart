using ChainFx.Web;

namespace ChainMart
{
    [UserAuthorize]
    [Ui("我的个人账号")]
    public class MyVarWork : WebWork
    {
        protected override void OnCreate()
        {
            CreateWork<MyInfoVarWork>("info");

            CreateWork<MyBuyWork>("buy");

            CreateWork<MyCreditWork>("credit");

            CreateWork<MyAccessVarWork>("access");
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

                h.DIV_("uk-margin-large-top uk-flex uk-flex-center").PIC("/qrcode.jpg", css: "uk-width-large")._DIV();
                h.H4_("uk-flex uk-flex-center").T("推荐中惠农通")._H4();
            }, false, 900);
        }
    }
}