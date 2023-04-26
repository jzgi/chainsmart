using ChainFx.Web;

namespace ChainSmart;

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
                h.PIC(org.IsOfShop ? "/rtl.webp" : org.IsCenter ? "/ctr.webp" : "/sup.webp", circle: true, css: "uk-width-small");

            h._TOPBARXL();

            h.WORKBOARD(notice: org.id);
        }, false, 30, title: org.name);
    }
}

[OrglyAuthorize(Org.TYP_RTL)]
[Ui("市场操作")]
public class RtllyVarWork : OrglyVarWork
{
    protected override void OnCreate()
    {
        // org

        CreateWork<OrglySetgWork>("setg");

        CreateWork<OrglyAccessWork>("access", true); // true = shop

        CreateWork<OrglyBuyClearWork>("buyclr", state: true);

        CreateWork<OrglyCarbonWork>("credit");

        // retail shop

        CreateWork<RtllyItemWork>("ritem");

        CreateWork<RtllyPosWork>("rpos");

        CreateWork<RtllyBuyWork>("rbuy");

        CreateWork<RtllyOrdWork>("rord");

        CreateWork<RtllyBuyAggWork>("rbuyagg");

        CreateWork<RtllyOrdAggWork>("rordagg");

        CreateWork<RtllyVipWork>("rvip");

        // mkt

        CreateWork<MktlyOrgWork>("morg");

        CreateWork<MktlyCreditWork>("mcredit");

        CreateWork<MktlyBuyWork>("mbuy");

        CreateWork<MktlyOrdWork>("mord");

        CreateWork<MktlyBuyAggWork>("mbuyagg");

        CreateWork<MktlyOrdAggWork>("mordagg");
    }
}

[OrglyAuthorize(Org.TYP_SUP)]
[Ui("供应操作")]
public class SuplyVarWork : OrglyVarWork
{
    protected override void OnCreate()
    {
        // org

        CreateWork<OrglySetgWork>("setg");

        CreateWork<OrglyAccessWork>("access", false); // false = source

        CreateWork<OrglyOrdClearWork>("ordclr", state: true); // true = is org

        CreateWork<OrglyCarbonWork>("credit");

        // supply shop

        CreateWork<SuplyProdWork>("sprod");

        CreateWork<SuplyLotWork>("slot");

        CreateWork<SuplyOrdWork>("sordspot", state: Ord.TYP_SPOT, ui: new("销售订单-现货", "商户"));

        CreateWork<SuplyOrdWork>("sordlift", state: Ord.TYP_LIFT, ui: new("销售订单-助农", "商户"));

        CreateWork<SuplyOrdAggWork>("sordagg");

        // ctr

        CreateWork<CtrlyOrgWork>("corg");

        CreateWork<CtrlyCreditWork>("ceval");

        CreateWork<CtrlyOrdWork>("cord");

        CreateWork<CtrlyLotWork>("clot");

        CreateWork<CtrlyOrdAggWork>("cordagg");
    }
}