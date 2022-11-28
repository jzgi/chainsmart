using ChainFx.Web;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class OrglyVarWork : WebWork
    {
    }


    [UserAuthorize(Org.TYP_SRC, 1)]
    [Ui("供区和产源操作")]
    public class ZonlyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            // ctr

            CreateWork<CtrlyBookWork>("cbook");

            CreateWork<CtrlyRptWork>("crpt");

            // zon

            CreateWork<ZonlyOrgWork>("zorg");

            CreateWork<ZonlyTestWork>("ztest");

            CreateWork<ZonlyRptWork>("zrpt");

            // src

            CreateWork<SrclyItemWork>("sitem");

            CreateWork<SrclyLotWork>("slot");

            CreateWork<SrclyBookWork>("sbook");

            CreateWork<SrclyRptWork>("srpt");

            // org

            CreateWork<OrglySetgWork>("setg");

            CreateWork<OrglyAccessWork>("access");

            CreateWork<OrglyClearWork>("clear");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var topOrgs = Grab<int, Org>();
            var prin = (User) wc.Principal;

            wc.GivePage(200, h =>
            {
                h.TOPBARXL_();
                if (org.icon)
                {
                    h.PIC("/org/", org.id, "/icon/", circle: true, css: "uk-width-medium");
                }
                else
                {
                    h.PIC(org.IsZone ? "/zon.webp" : "/src.webp", circle: true, css: "uk-width-small");
                }
                h.DIV_("uk-width-expand uk-col uk-padding-small-left");
                h.H2(org.name);
                h.SPAN(org.tel);
                h._DIV();
                h._TOPBARXL();

                h.TASKBOARD();
            }, false, 3);
        }
    }


    [UserAuthorize(Org.TYP_SHP, 1)]
#if ZHNT
    [Ui("市场和商户操作")]
#else
    [Ui("驿站和商户操作")]
#endif
    public class MktlyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            // mkt

            CreateWork<MktlyOrgWork>("morg");

            CreateWork<MktlyTestWork>("mtest");

            CreateWork<MktlyBuyWork>("mbuy");

            CreateWork<MktlyBookWork>("mbook");

            // shp

            CreateWork<ShplyBuyWork>("sbuy");

            CreateWork<ShplyPosWork>("spos");

            CreateWork<ShplyWareWork>("sware");

            CreateWork<ShplyVipWork>("svip");

            CreateWork<ShplyBookWork>("sbook");

            // org

            CreateWork<OrglySetgWork>("setg");

            CreateWork<OrglyAccessWork>("access");

            CreateWork<OrglyClearWork>("clear");
        }

        public void @default(WebContext wc)
        {
            var mkt = wc[0].As<Org>();
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                var role = prin.orgid != mkt.id ? "代办" : User.Orgly[prin.orgly];
                // h.TOOLBAR(tip: prin.name + "（" + role + "）");

                h.TOPBARXL_();
                if (mkt.icon)
                {
                    h.PIC_("uk-width-1-5", circle: true).T(MainApp.WwwUrl).T("/org/").T(mkt.id).T("/icon")._PIC();
                }
                else
                {
                    h.PIC("/mkt.webp", circle: true, css: "uk-width-small");
                }
                h.DIV_("uk-width-expand uk-col uk-padding-small-left");
                h.H2(mkt.name);
                h.SPAN(mkt.tel);
                h._DIV();
                h._TOPBARXL();

                h.TASKBOARD();
            }, false, 3);
        }
    }
}