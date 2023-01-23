using ChainFx.Web;

namespace ChainMart
{
    [MyAuthorize]
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

        public void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            wc.GivePage(200, h =>
            {
                h.TOPBARXL_();

                h.HEADER_("uk-width-expand uk-col uk-padding-left");
                h.H2(prin.name);
                if (prin.typ > 0) h.P(User.Typs[prin.typ]);
                h._HEADER();

                if (prin.icon)
                {
                    h.PIC("/user/", prin.id, "/icon/", circle: true, css: "uk-width-medium");
                }
                else
                    h.PIC("/my.webp", circle: true, css: "uk-width-small");

                h._TOPBARXL();

                h.WORKBOARD();

                h.ARTICLE_("uk-card uk-card-primary");
                h.H3("服务约定", "uk-card-header");
                h.SECTION_("uk-card-body");
                h.P("「中惠农通」线上平台是实体市场的镜像，平台上所有商户都是在指定市场内有实体摊铺的合法经营户，受当地政府有关部门监管。");
                h.P("通过平台发生的线上和线下的商品交易行为，买卖双方应各自负有相应的责任。");
                h.P("按照平台的协议，市场内的「中惠农通体验中心」作为商盟的盟主，有权向盟友摊铺提出整改建议。");
                h.P("「中惠农通」是集体所有制平台，保障您的个人数据不作任何其它用途。");
                h._SECTION();
                h.FOOTER_("uk-card-footer uk-flex-center").PIC("/qrcode.jpg", css: "uk-width-small")._FOOTER();
                h._ARTICLE();
            }, false, 900);
        }
    }
}