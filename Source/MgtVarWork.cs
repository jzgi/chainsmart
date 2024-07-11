using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Source;
using ChainFX.Web;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Storage;

namespace ChainSmart;

public abstract class MgtVarWork : WebWork
{
    public void @default(WebContext wc)
    {
        var org = wc[0].As<Org>();

        string title = org.name;
        if (this is MktlyVarWork)
        {
            var mkt = GrabTwin<int, Org>(org.MktId);
            title = mkt.whole + " - " + org.name;
        }

        wc.GivePage(200, h =>
        {
            h.TOPBARXL_();

            h.HEADER_("uk-width-expand uk-col uk-padding-small-left");
            h.H1_().T(org.name)._H1();
            if (org.AsEst)
            {
                h.H4(org.Whole);
            }
            h._HEADER();

            if (org.icon)
            {
                h.PIC(MainUtility.OrgUrl, org.id, "/icon", circle: true, css: "uk-width-small");
            }
            else
            {
                h.PIC(MainApp.WwwUrl, org.AsEst ? "/est.webp" : "/sup.webp", circle: true, css: "uk-width-small");
            }

            h._TOPBARXL();

            h.WORKBOARD(accessTyp: org.typ, twinSpy: org.id);

            h.TOOLBAR(bottom: true, status: org.Status, state: org.ToState());
        }, false, 30, title: title);
    }


    [Ui("上线", "上线投入使用", status: 1 | 2), Tool(ButtonConfirm, state: Org.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        var m = wc[0].As<Org>();
        var prin = (User)wc.Principal;

        var now = DateTime.Now;
        await GetTwinCache<OrgCache, int, Org>().UpdateAsync(m,
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

        await GetTwinCache<OrgCache, int, Org>().UpdateAsync(org,
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

[MgtAuthorize(Org.TYP_RTL_)]
[Ui("市场操作")]
public class MktlyVarWork : MgtVarWork, IExternable
{
    protected override void OnCreate()
    {
        // org

        CreateWork<ShplyMbrWork>("mbr", header: "常规");


        CreateWork<ShplyItemWork>("sitem", header: "商户");

        CreateWork<ShplyBuyWork>("sbuy");

        CreateWork<ShplyPosWork>("spos");

        CreateWork<ShplyBuyApWork>("sbuyap");

        CreateWork<ShplyBuyLdgWork>("sbuyldg");

        CreateWork<ShplyVipWork>("svip");

        CreateWork<MchlyPurWork>("mpur");

        // mkt

        CreateWork<MktlyOrgWork>("mmch", state: Org.TYP_SHX, ui: new UiAttribute("成员商户"), header: "机构");

        CreateWork<MktlyOrgWork>("mshp", state: Org.TYP_SHP, ui: new UiAttribute("成员门店"));

        CreateWork<MktlyBuyWork>("mbuy");

        CreateWork<MktlyPurWork>("mpur");

        CreateWork<MktlyTestWork>("mtest");

        CreateWork<MktlyCerWork>("mcer");
    }

    [MgtAuthorize(Org.TYP_RTL_, User.ROL_MGT)]
    [Ui("设置", "设置基本信息和参数", status: 1 | 2 | 4), Tool(ButtonShow)]
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

            await GetTwinCache<OrgCache, int, Org>().UpdateAsync(m,
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

    public void @extern(WebContext wc)
    {
        var org = wc[0].As<Org>();
        var now = DateTime.Now;

        // receive incoming data
        if (wc.IsPost)
        {
            // handle incoming
            wc.Give(200);
        }
        else // send outgoing data
        {
            var bdr = new JsonBuilder(true, 1024 * 32);
            org.BuySet.Dump(bdr, now);

            wc.Give(200, bdr);
        }
    }
}

[MgtAuthorize(Org.TYP_WHL_)]
[Ui("供应操作")]
public class SuplyVarWork : MgtVarWork
{
    protected override void OnCreate()
    {
        // org

        CreateWork<SuplyMbrWork>("mbr", header: "常规");

        // sup

        CreateWork<SuplyItemWork>("sitem", header: "供应源");

        CreateWork<SuplyPurWork>("spur");

        CreateWork<SuplyPurApWork>("spurap");

        CreateWork<SuplyPurLdgWork>("spurldg");

        CreateWork<SrclyCodeWork>("scode", header: "供应源");

        CreateWork<SrclyBatWork>("sbat", header: "供应源");

        CreateWork<SrclyBuyApWork>("sbuyap");


        // hub

        CreateWork<HublyPurWork>("hpur", header: "云仓");

        CreateWork<HublyPurLdgWork>("hpurldg");

        CreateWork<HublyLotWork>("hlot");

        CreateWork<HublyBatWork>("hbat");

        CreateWork<HublyTestWork>("htest");
    }
}