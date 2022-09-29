using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class OrglyVarWork : WebWork
    {
        [Ui("运行设置"), Tool(Modal.ButtonOpen)]
        public async Task setg(WebContext wc)
        {
            var org = wc[0].As<Org>();
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("修改基本设置");
                    h.LI_().TEXT("标语", nameof(org.tip), org.tip, max: 16)._LI();
                    h.LI_().TEXT("地址", nameof(org.addr), org.addr, max: 16)._LI();
                    h.LI_().SELECT("状态", nameof(org.status), org.status, Entity.Statuses, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else
            {
                var o = await wc.ReadObjectAsync(instance: org); // use existing object
                using var dc = NewDbContext();
                // update the db record
                await dc.ExecuteAsync("UPDATE orgs SET tip = @1, cttid = CASE WHEN @2 = 0 THEN NULL ELSE @2 END, date = @3 WHERE id = @4",
                    p => p.Set(o.tip).Set(o.status).Set(org.id));

                wc.GivePane(200);
            }
        }
    }


    [UserAuthorize(Org.TYP_SRC, 1)]
    [Ui("供区产源操作")]
    public class ZonlyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            CreateWork<ZonlyOrgWork>("zorg");

            CreateWork<ZonlyRptWork>("zrpt");


            CreateWork<SrclyItemWork>("sitem");

            CreateWork<SrclyLotWork>("slot");

            CreateWork<SrclyBookWork>("sbook");

            CreateWork<SrclyRptWork>("srpt");


            CreateWork<CtrlyLotWork>("clot");

            CreateWork<CtrlyBookWork>("cbook");

            CreateWork<CtrlyDistrWork>("cdistr");

            CreateWork<CtrlyRptWork>("crpt");


            CreateWork<OrglyClearWork>("clear");

            CreateWork<OrglyAccessWork>("access");
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
                    h.PIC("/zon.webp", circle: true, css: "uk-width-small");
                }
                h.DIV_("uk-width-expand uk-col uk-padding-small-left");
                h.H2(org.name);
                h.SPAN(org.tel);
                h._DIV();
                h._TOPBARXL();

                h.TASKLIST();
            }, false, 3);
        }
    }


    [UserAuthorize(Org.TYP_SHP, 1)]
#if ZHNT
    [Ui("市场商户操作")]
#else
    [Ui("驿站商户操作")]
#endif
    public class MktlyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            CreateWork<MktlyOrgWork>("morg");

            CreateWork<MktlyCustWork>("mcust");

            CreateWork<MktlyBookWork>("mbook");

            CreateWork<MktlyBuyWork>("mbuy");


            CreateWork<ShplyBookWork>("sbook");

            CreateWork<ShplyBuyWork>("sbuy");

            CreateWork<ShplyStockWork>("sware");


            CreateWork<OrglyClearWork>("clear");

            CreateWork<OrglyAccessWork>("access");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                var role = prin.orgid != org.id ? "代办" : User.Orgly[prin.orgly];
                // h.TOOLBAR(tip: prin.name + "（" + role + "）");

                h.TOPBARXL_();
                if (prin.icon)
                {
                    h.PIC("/org/", prin.id, "/icon/", circle: true, css: "uk-width-medium");
                }
                else
                {
                    h.PIC("/mkt.webp", circle: true, css: "uk-width-small");
                }

                h.DIV_("uk-width-expand uk-col uk-padding-small-left");
                h.H2(org.name);
                h.SPAN(org.tel);
                h._DIV();
                h._TOPBARXL();

                h.TASKLIST();
            }, false, 3);
        }
    }
}