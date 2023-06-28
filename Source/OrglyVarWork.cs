using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;

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
            h.H1_().T(org.name).SP().Q(Org.Statuses[org.status])._H1();
            if (org.OfUpper) h.H4(org.Cover);
            h.Q_().T(prin.name).T('（').T(rol).T('）')._Q();
            h._HEADER();

            if (org.icon)
            {
                h.PIC(MainApp.WwwUrl, "/org/", org.id, "/icon", circle: true, css: "uk-width-small");
            }
            else
                h.PIC(org.OfRetail ? "/rtl.webp" : org.IsCenter ? "/ctr.webp" : "/sup.webp", circle: true, css: "uk-width-small");

            h._TOPBARXL();

            h.WORKBOARD(twinSpy: org.id);

            h.TOOLBAR(bottom: true, status: org.Status, state: org.State);
        }, false, 30, title: org.name);
    }


    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("上线", "上线投入使用", icon: "cloud-upload", status: 3), Tool(ButtonConfirm, state: Org.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE orgs SET status = 4, oked = @1, oker = @2 WHERE id = @3");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(org.id));

        wc.GivePane(200);
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("下线", "下线以便修改", icon: "cloud-download", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE orgs SET status = 2, oked = NULL, oker = NULL WHERE id = @1");
        await dc.ExecuteAsync(p => p.Set(org.id));

        wc.GivePane(200);
    }
}

[OrglyAuthorize(Org.TYP_RTL)]
[Ui("市场操作")]
public class RtllyVarWork : OrglyVarWork
{
    protected override void OnCreate()
    {
        // org

        CreateWork<OrglyAccessWork>("access", state: true, header: "常规"); // true = retail

        CreateWork<OrglyEvalWork>("eval");

        // retail shop

        CreateWork<RtllyItemWork>("ritem", header: "商户");

        CreateWork<RtllyVipWork>("rvip");

        CreateWork<RtllyPosWork>("rpos");

        CreateWork<RtllyBuyWork>("rbuy");

        CreateWork<RtllyBuyApWork>("rbuyap");

        CreateWork<RtllyBuyLdgWork>("rbuyldg");

        CreateWork<RtllyPurWork>("rpur");

        // mkt

        CreateWork<MktlyOrgWork>("morg", header: "机构");

        CreateWork<MktlyEvalWork>("meval");

        CreateWork<MktlyBuyWork>("mbuy"); // delivery

        CreateWork<MktlyPurWork>("mpur");
    }

    [Ui(tip: "摊铺直通车", icon: "thumbnails", status: 7), Tool(ButtonShow)]
    public void qrcode(WebContext wc)
    {
        var org = wc[0].As<Org>();

        wc.GivePane(200, h =>
        {
            h.NAV_("uk-col uk-flex-middle uk-margin-large-top");
            h.QRCODE(MainApp.WwwUrl + "/" + org.upperid + "/" + org.id + "/", css: "uk-width-small");
            h.SPAN(org.name);
            h._NAV();
        }, false, 720);
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("设置", icon: "cog", status: 7), Tool(ButtonShow)]
    public async Task setg(WebContext wc)
    {
        var org = wc[0].As<Org>();
        var prin = (User)wc.Principal;

        var m = GrabTwin<int, Org>(org.id);

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("设置基本信息和参数");
                h.LI_().TEXTAREA("简介", nameof(org.tip), org.tip, max: 40)._LI();
                h.LI_().TEXT("营业电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                h.LI_().TIME("开档时间", nameof(m.openat), m.openat).TIME("收档时间", nameof(m.closeat), m.closeat)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(setg))._FORM();
            });
        }
        else
        {
            await wc.ReadObjectAsync(instance: m); // use existing object

            using var dc = NewDbContext();
            // update the db record
            await dc.ExecuteAsync("UPDATE orgs SET tip = @1, tel = @2, adapted = @3, adapter = @4, status = 2 WHERE id = @5", p => p.Set(m.tip).Set(m.tel).Set(DateTime.Now).Set(prin.name).Set(org.id));

            wc.GivePane(200);
        }
    }

    /// <summary>
    /// The polling of events that belong to the presented org.
    /// </summary>
    [OrglyAuthorize(Org.TYP_MKT)]
    public void @event(WebContext wc)
    {
        var org = wc[0].As<Org>();
        var prin = (User)wc.Principal;

        var es = org.Events;

        var j = new JsonBuilder(true, 1024 * 32);
        try
        {
            j.ARR_();


            j._ARR();
        }
        finally
        {
            j.Clear();
        }
    }
}

[OrglyAuthorize(Org.TYP_SUP)]
[Ui("供应操作")]
public class SuplyVarWork : OrglyVarWork
{
    protected override void OnCreate()
    {
        // org

        CreateWork<OrglyAccessWork>("access", state: false, header: "常规"); // false = supply

        CreateWork<OrglyEvalWork>("eval");

        // supply shop

        CreateWork<SuplyFabWork>("sfab", header: "商户");

        CreateWork<SuplyLotWork>("slot");

        CreateWork<SuplyPurWork>("spurnorm", state: Pur.TYP_NORM, ui: new("销售订单-现供"));

        CreateWork<SuplyPurWork>("spuradvc", state: Pur.TYP_ADVC, ui: new("销售订单-助农"));

        CreateWork<SuplyPurLdgWork>("spurldg");

        CreateWork<SuplyPurApWork>("spurap");

        // ctr

        CreateWork<CtrlyOrgWork>("corg", header: "机构");

        CreateWork<CtrlyEvalWork>("ceval");

        CreateWork<CtrlyLotWork>("clot");

        CreateWork<CtrlyPurWork>("cpur");

        CreateWork<CtrlyPurLdgWork>("cpurldg");

        CreateWork<CtrlyTwinWork>("ctwin");
    }
}