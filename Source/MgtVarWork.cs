using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Storage;

namespace ChainSmart;

public abstract class MgtVarWork : WebWork
{
    public void @default(WebContext wc)
    {
        var org = wc[0].As<Org>();
        var prin = (User)wc.Principal;

        wc.GivePage(200, h =>
        {
            h.TOPBARXL_();

            h.HEADER_("uk-width-expand uk-col uk-padding-left");
            h.H1_().T(org.name).SP().SUB_().T(org.id, 5)._SUB()._H1();
            if (org.AsEst)
            {
                h.H4(org.Cover);
            }
            h.P(prin.name);
            h._HEADER();

            if (org.icon)
            {
                h.PIC(MainApp.WwwUrl, "/org/", org.id, "/icon", circle: true, css: "uk-width-small");
            }
            else
            {
                h.PIC(org.AsMkt ? "/mkt.webp" : org.IsHub ? "/hub.webp" : "/sup.webp", circle: true, css: "uk-width-small");
            }

            h._TOPBARXL();

            h.WORKBOARD(accessTyp: org.typ, twinSpy: org.id);

            h.TOOLBAR(bottom: true, status: org.Status, state: org.ToState());
        }, false, 30, title: org.name, onload: "forWebview();");
    }


    [Ui("上线", "上线投入使用", status: 3), Tool(ButtonConfirm, state: Org.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        var m = wc[0].As<Org>();
        var prin = (User)wc.Principal;

        var now = DateTime.Now;
        await GetTwinCache<OrgTwinCache, int, Org>().UpdateAsync(m,
            async (dc) =>
            {
                dc.Sql("UPDATE orgs SET status = 4, oked = @1, oker = @2 WHERE id = @3");
                return await dc.ExecuteAsync(p => p.Set(now).Set(prin.name).Set(m.id)) == 1;
            },
            x =>
            {
                x.oked = now;
                x.oker = prin.name;
                x.status = 4;
            }
        );

        wc.Give(200);
    }

    [Ui("下线", "下线停用或调整", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        var org = wc[0].As<Org>();

        await GetTwinCache<OrgTwinCache, int, Org>().UpdateAsync(org,
            async (dc) =>
            {
                dc.Sql("UPDATE orgs SET status = 2, oked = NULL, oker = NULL WHERE id = @1");
                return await dc.ExecuteAsync(p => p.Set(org.id)) == 1;
            },
            x =>
            {
                x.oked = default;
                x.oker = null;
                x.status = 2;
            }
        );

        wc.Give(200);
    }
}

[MgtAuthorize(Org.TYP_MKT_)]
[Ui("市场操作")]
public class MktlyVarWork : MgtVarWork
{
    protected override void OnCreate()
    {
        // org

        CreateWork<OrglyMbrWork>("mbr", state: true, header: "常规"); // true = retail


        CreateWork<ShplyItemWork>("sitem", header: "商户");

        CreateWork<ShplyBatWork>("sbat");

        CreateWork<ShplyBuyWork>("sbuy");

        CreateWork<ShplyPosWork>("spos");

        CreateWork<ShplyBuyApWork>("sbuyap");

        CreateWork<ShplyBuyLdgWork>("sbuyldg");

        CreateWork<ShplyVipWork>("svip");

        CreateWork<StalyPurWork>("spur");

        // mkt

        CreateWork<MktlyOrgWork>("msta", state: Org.TYP_STL, ui: new UiAttribute("成员商户"), header: "机构");

        CreateWork<MktlyOrgWork>("mshp", state: Org.TYP_SHP, ui: new UiAttribute("成员门店"));

        CreateWork<MktlyBuyWork>("mbuy");

        CreateWork<MktlyPurWork>("mpur");

        CreateWork<MktlyTestWork>("mtest");

        CreateWork<MktlyCerWork>("mcer");

        CreateWork<MktlyAdWork>("mad");
    }

    [Ui(tip: "摊铺直通车", icon: "thumbnails", status: 7), Tool(ButtonShow)]
    public void qrcode(WebContext wc)
    {
        var org = wc[0].As<Org>();

        wc.GivePane(200, h =>
        {
            h.NAV_("uk-col uk-flex-middle uk-margin-large-top");
            h.QRCODE(MainApp.WwwUrl + "/" + org.parentid + "/" + org.id + "/", css: "uk-width-small");
            h.SPAN(org.name);
            h._NAV();

            if (org.AsEst)
            {
                h.NAV_("uk-col uk-flex-middle uk-margin-large-top");
                h.QRCODE(MainApp.WwwUrl + "/" + org.id + "/", css: "uk-width-small");
                h.SPAN(org.Cover);
                h._NAV();
            }
        }, false, 720);
    }

    [MgtAuthorize(Org.TYP_MKT, User.ROL_MGT)]
    [Ui("设置", "设置基本信息和参数", status: 7), Tool(ButtonShow)]
    public async Task setg(WebContext wc)
    {
        var m = wc[0].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);
                h.LI_().TEXTAREA("简介语", nameof(m.tip), m.tip, max: 40)._LI();
                h.LI_().TEXT("营业电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                h.LI_().TIME("开档时间", nameof(m.openat), m.openat).TIME("收档时间", nameof(m.closeat), m.closeat)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(setg))._FORM();
            });
        }
        else
        {
            await wc.ReadObjectAsync(instance: m); // use existing object
            lock (m)
            {
                m.oked = default;
                m.oker = null;
            }

            await GetTwinCache<OrgTwinCache, int, Org>().UpdateAsync(m,
                async (dc) =>
                {
                    return await dc.ExecuteAsync(
                        "UPDATE orgs SET tip = @1, tel = @2, openat = @3, closeat = @4, adapted = @5, adapter = @6 WHERE id = @7",
                        p => p.Set(m.tip).Set(m.tel).Set(m.openat).Set(m.closeat).Set(DateTime.Now).Set(prin.name).Set(m.id)
                    ) == 1;
                }
            );

            wc.GivePane(200);
        }
    }

    public void @event(WebContext wc)
    {
        var org = wc[0].As<Org>();
        var now = DateTime.Now;

        // receive incoming events
        if (wc.IsPost)
        {
            //
        }

        // outgoing events
        //
        var bdr = new JsonBuilder(true, 1024 * 32);
        org.EventPack.Dump(bdr, now);

        wc.Give(200, bdr);
    }
}

[MgtAuthorize(Org.TYP_SUP_)]
[Ui("供应操作")]
public class SuplyVarWork : MgtVarWork
{
    protected override void OnCreate()
    {
        // org

        CreateWork<OrglyMbrWork>("mbr", state: false, header: "常规"); // false = supply

        // sup

        CreateWork<SuplyItemWork>("sitem", header: "供应源");

        CreateWork<SuplyPurWork>("spur");

        CreateWork<SuplyPurApWork>("spurap");

        CreateWork<SuplyPurLdgWork>("spurldg");

        CreateWork<SupSrclyBatWork>("sbat", header: "供应源");

        CreateWork<SrclyCodeWork>("scode");

        CreateWork<SrclyTieWork>("stie");

        // hub

        CreateWork<HublyPurWork>("hpur", header: "云仓");

        CreateWork<HublyPurLdgWork>("hpurldg");

        CreateWork<HublyLotWork>("hlot");

        CreateWork<HublyBatWork>("hjob");

        CreateWork<HublyTestWork>("htest");
    }
}