using ChainFx.Web;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class OrglyVarWork : WebWork
    {
    }


    [OrglyAuthorize(Org.TYP_SRC, 1)]
    [Ui("供区和产源操作")]
    public class ZonlyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            // org

            CreateWork<OrglySetgWork>("setg");

            CreateWork<OrglyAccessWork>("access");

            CreateWork<OrglyClearWork>("clear");

            // src

            CreateWork<SrclyItemWork>("sitem");

            CreateWork<SrclyLotWork>("slot");

            CreateWork<SrclyBookWork>("sbook");

            CreateWork<SrclyRptWork>("srpt");

            // zon

            CreateWork<ZonlyOrgWork>("zorg");

            CreateWork<ZonlyTestWork>("ztest");

            CreateWork<ZonlyRptWork>("zrpt");

            // ctr

            CreateWork<CtrlyBookWork>("cbook");

            CreateWork<CtrlyRptWork>("crpt");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var prin = (User) wc.Principal;

            wc.GivePage(200, h =>
            {
                h.TOPBARXL_();

                bool astack = wc.Query[nameof(astack)];
                if (astack)
                {
                    h.T("<a class=\"uk-icon-button\" href=\"javascript: window.parent.closeUp(false);\" uk-icon=\"icon: chevron-left; ratio: 1.75\"></a>");
                }

                string rol = wc.Dive ? "代" + User.Orgly[wc.Role] : User.Orgly[wc.Role];

                h.HEADER_("uk-width-expand uk-col uk-padding-small-left");
                h.H2(org.name).P2(prin.name, rol, brace: true);
                h._HEADER();

                if (org.icon)
                {
                    h.PIC_("uk-width-1-5", circle: true).T(MainApp.WwwUrl).T("/org/").T(org.id).T("/icon")._PIC();
                }
                else
                    h.PIC(org.IsZone ? "/zon.webp" : "/src.webp", circle: true, css: "uk-width-small");

                h._TOPBARXL();

                h.WORKBOARD();
            }, false, 720, title: org.name);
        }
    }


    [OrglyAuthorize(Org.TYP_SHP, 1)]
#if ZHNT
    [Ui("市场和商户操作")]
#else
    [Ui("驿站和商户操作")]
#endif
    public class MktlyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            // org

            CreateWork<OrglySetgWork>("setg");

            CreateWork<OrglyAccessWork>("access");

            CreateWork<OrglyClearWork>("clear");

            // shp

            CreateWork<ShplyBuyWork>("sbuy");

            CreateWork<ShplyPosWork>("spos");

            CreateWork<ShplyWareWork>("sware");

            CreateWork<ShplyVipWork>("svip");

            CreateWork<ShplyBookWork>("sbook");

            // mkt

            CreateWork<MktlyOrgWork>("morg");

            CreateWork<MktlyTestWork>("mtest");

            CreateWork<MktlyBuyWork>("mbuy");

            CreateWork<MktlyBookWork>("mbook");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                h.TOPBARXL_();

                bool astack = wc.Query[nameof(astack)];
                if (astack)
                {
                    h.T("<a class=\"uk-icon-button\" href=\"javascript: window.parent.closeUp(false);\" uk-icon=\"icon: chevron-left; ratio: 1.75\"></a>");
                }

                string rol = wc.Dive ? "代" + User.Orgly[wc.Role] : User.Orgly[wc.Role];

                h.HEADER_("uk-width-expand uk-col uk-padding-small-left");
                h.H2(org.name).P2(prin.name, rol, brace: true);
                h._HEADER();

                if (org.icon)
                {
                    h.PIC_("uk-width-1-5", circle: true).T(MainApp.WwwUrl).T("/org/").T(org.id).T("/icon")._PIC();
                }
                else
                    h.PIC("/mkt.webp", circle: true, css: "uk-width-small");

                h._TOPBARXL();

                h.WORKBOARD();
            }, false, 720, title: org.name);
        }
    }
}