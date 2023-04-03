using ChainFx.Web;

namespace ChainSmart
{
    public abstract class OrglyVarWork : WebWork
    {
        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var prin = (User)wc.Principal;

            wc.GivePage(200, h =>
            {
                h.TOPBARXL_();

                bool astack = wc.Query[nameof(astack)];
                if (astack)
                {
                    h.T("<a class=\"uk-icon-button\" href=\"javascript: window.parent.closeUp(false);\" uk-icon=\"icon: chevron-left; ratio: 1.75\"></a>");
                }

                string rol = wc.Super ? "代" + User.Orgly[wc.Role] : User.Orgly[wc.Role];

                h.HEADER_("uk-width-expand uk-col uk-padding-left");
                h.H2(org.name);
                if (org.IsParent) h.H4(org.Ext);
                h.P2(prin.name, rol, brace: true);
                h._HEADER();

                if (org.icon)
                {
                    h.PIC(MainApp.WwwUrl, "/org/", org.id, "/icon", circle: true, css: "uk-width-small");
                }
                else
                    h.PIC(org.IsOfShop ? "/shp.webp" : org.IsCenter ? "/ctr.webp" : "/src.webp", circle: true, css: "uk-width-small");

                h._TOPBARXL();

                h.WORKBOARD(notice: org.id);
            }, false, 30, title: org.name);
        }
    }


    [OrglyAuthorize(Org.TYP_SHP)]
    [Ui("市场操作")]
    public class ShplyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            // org

            CreateWork<OrglySetgWork>("setg");

            CreateWork<OrglyAccessWork>("access", true); // true = shop

            CreateWork<OrglyBuyClearWork>("buyclr", state: true);

            CreateWork<OrglyCreditWork>("credit");

            // shp

            CreateWork<ShplyItemWork>("sitem");

            CreateWork<ShplyPosWork>("spos");

            CreateWork<ShplyBuyWork>("sbuy");

            CreateWork<ShplyBookWork>("sbook");

            CreateWork<ShplyBuyAggWork>("sbuyagg");

            CreateWork<ShplyBookAggWork>("sbookagg");

            CreateWork<ShplyVipWork>("svip");

            // mkt

            CreateWork<MktlyOrgWork>("morg");

            CreateWork<MktlyTestWork>("mtest");

            CreateWork<MktlyBuyWork>("mbuy");

            CreateWork<MktlyBookWork>("mbook");

            CreateWork<MktlyBuyAggWork>("mbuyagg");

            CreateWork<MktlyBookAggWork>("mbookagg");
        }
    }


    [OrglyAuthorize(Org.TYP_SRC)]
    [Ui("供应操作")]
    public class SrclyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            // org

            CreateWork<OrglySetgWork>("setg");

            CreateWork<OrglyAccessWork>("access", false); // false = source

            CreateWork<OrglyCreditWork>("credit");

            CreateWork<OrglyBookClearWork>("bookclr", state: true); // true = is org

            // src

            CreateWork<SrclyAssetWork>("sasset");

            CreateWork<SrclyLotWork>("slot");

            CreateWork<SrclyBookWork>("sbookspot", state: Book.TYP_SPOT, ui: new("销售订单-现货", "商户"));

            CreateWork<SrclyBookWork>("sbooklift", state: Book.TYP_LIFT, ui: new("销售订单-助农", "商户"));

            CreateWork<SrclyBookAggWork>("sbookagg");

            // ctr

            CreateWork<CtrlyOrgWork>("corg");

            CreateWork<CtrlyBookWork>("cbook");

            CreateWork<CtrlyLotWork>("clot");

            CreateWork<CtrlyBookAggWork>("cbookagg");
        }
    }
}