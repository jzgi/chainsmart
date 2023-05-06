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
                h.PIC(org.IsOfRetail ? "/rtl.webp" : org.IsCenter ? "/ctr.webp" : "/sup.webp", circle: true, css: "uk-width-small");

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

        CreateWork<OrglyEvalWork>("ceval");

        // retail shop

        CreateWork<RtllyItemWork>("ritem");

        CreateWork<RtllyPosWork>("rpos");

        CreateWork<RtllyBuyWork>("rbuy");

        CreateWork<RtllyPurWork>("rpur");

        CreateWork<RtllyBuyAggWork>("rbuyagg");

        CreateWork<RtllyPurAggWork>("rpuragg");

        CreateWork<RtllyVipWork>("rvip");

        // mkt

        CreateWork<MktlyOrgWork>("morg");

        CreateWork<MktlyEvalWork>("meval");

        CreateWork<MktlyBuyWork>("mbuy");

        CreateWork<MktlyPurWork>("mpur");

        CreateWork<MktlyBuyAggWork>("mbuyagg");

        CreateWork<MktlyPurAggWork>("mpuragg");
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

        CreateWork<OrglyPurClearWork>("purclr", state: true); // true = is org

        CreateWork<OrglyEvalWork>("eval");

        // supply shop

        CreateWork<SuplyFabWork>("sfab");

        CreateWork<SuplyLotWork>("slot");

        CreateWork<SuplyPurWork>("spurspot", state: Pur.TYP_SPOT, ui: new("销售订单-现货", "商户"));

        CreateWork<SuplyPurWork>("spurpre", state: Pur.TYP_PRE, ui: new("销售订单-助农", "商户"));

        CreateWork<SuplyPurAggWork>("spuragg");

        // ctr

        CreateWork<CtrlyOrgWork>("corg");

        CreateWork<CtrlyEvalWork>("ceval");

        CreateWork<CtrlyPurWork>("cpur");

        CreateWork<CtrlyLotWork>("clot");

        CreateWork<CtrlyPurAggWork>("cpuragg");
    }
}