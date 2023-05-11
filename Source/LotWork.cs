using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

public abstract class LotWork<V> : WebWork where V : LotVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, Lot[] arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

            if (o.icon)
            {
                h.PIC(MainApp.WwwUrl, "/lot/", o.id, "/icon", css: "uk-width-1-5");
            }
            else
                h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name).SPAN(Lot.Typs[o.typ], "uk-badge")._HEADER();
            h.Q(o.tip, "uk-width-expand");
            h.FOOTER_().T("每件").SP().T(o.unitx).SP().T(o.unit).SPAN_("uk-margin-auto-left").CNY(o.price)._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

public class PublyLotWork : LotWork<PublyLotVarWork>
{
    static readonly string[] TAGS = { "", "label" };

    [Ui("预制标签")]
    public async Task @default(WebContext wc)
    {
        int num = 0;
        wc.GivePage(200, h =>
        {
            h.TOPBAR_().SUBNAV(TAGS)._TOPBAR();
            //
            h.FORM_("uk-card uk-card-primary").FIELDSUL_("预制的硬质标签");
            h.LI_().NUMBER("溯源编号", nameof(num), num)._LI();
            h.LI_().BUTTON("查询")._LI();
            h._FIELDSUL();
            h._FORM();
        }, shared: true, 120, title: "产品批次溯源查询");
    }

    [Ui("印制贴标")]
    public async Task label(WebContext wc)
    {
        int num = 0;
        wc.GivePage(200, h =>
        {
            h.TOPBAR_().SUBNAV(TAGS)._TOPBAR();
            //
            h.FORM_("uk-card uk-card-primary").FIELDSUL_("印制的纸质贴标");
            h.LI_().NUMBER("溯源编号", nameof(num), num)._LI();
            h.LI_().BUTTON("查询")._LI();
            h._FIELDSUL();
            h._FORM();
        }, shared: true, 120, title: "产品批次溯源查询");
    }
}

[Ui("产品批次", "商户")]
public class SuplyLotWork : LotWork<SuplyLotVarWork>
{
    [Ui("产品批次", group: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE orgid = @1 AND status = 4 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无上线产品批次");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }

    [Ui(tip: "已下线", icon: "cloud-download", group: 2), Tool(Anchor)]
    public async Task down(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE orgid = @1 AND status BETWEEN 1 AND 2 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无已下线产品批次");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }

    [Ui(tip: "已删除", icon: "trash", group: 4), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE orgid = @1 AND status = 0 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无已删除产品批次");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("现货", "新建现货产品批次", icon: "plus", group: 1), Tool(ButtonOpen)]
    public async Task newspot(WebContext wc, int typ)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var topOrgs = Grab<int, Org>();
        var cats = Grab<short, Cat>();

        var o = new Lot
        {
            typ = Lot.TYP_SPOT,
            status = Entity.STU_CREATED,
            orgid = org.id,
            orgname = org.name,
            unit = "斤",
            cap = 2000,
            created = DateTime.Now,
            creator = prin.name,
            off = 0,
            unitx = 1,
            flashx = 0,
            maxx = 1,
        };

        if (wc.IsGet)
        {
            var fabs = GrabTwinSet<int, int, Fab>(o.orgid);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("现货（货入品控库之后再销售）");

                h.LI_().TEXT("产品名称", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().SELECT("分类", nameof(o.catid), o.catid, cats, required: true)._LI();
                h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("产品源", nameof(o.fabid), o.fabid, fabs)._LI();
                h.LI_().SELECT("限域投放", nameof(o.targs), o.targs, topOrgs, filter: (_, v) => v.IsCenter, capt: v => v.Ext, size: 2, required: false)._LI();
                h.LI_().SELECT("单位", nameof(o.unit), o.unit, Unit.Typs, keyonly: true, required: true).NUMBER("批次总量", nameof(o.cap), o.cap)._LI();
                h.LI_().NUMBER("每件含量", nameof(o.unitx), o.unitx, min: 1)._LI();

                h._FIELDSUL().FIELDSUL_("销售及优惠");

                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("直降", nameof(o.off), o.off, min: 0.01M, max: 999.99M)._LI();
                h.LI_().NUMBER("秒杀件数", nameof(o.flashx), o.flashx, min: 1, max: o.AvailX).NUMBER("限订件数", nameof(o.maxx), o.maxx, min: 1, max: o.AvailX)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(newspot));

                h._FORM();
            });
        }
        else // POST
        {
            const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
            // populate 
            await wc.ReadObjectAsync(msk, instance: o);

            // db insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO lots ").colset(Lot.Empty, msk)._VALUES_(Lot.Empty, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("助农", "新建助农产品批次", icon: "plus", group: 1), Tool(ButtonOpen)]
    public async Task newpre(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var topOrgs = Grab<int, Org>();
        var cats = Grab<short, Cat>();

        var o = new Lot
        {
            typ = Lot.TYP_PRE,
            status = Entity.STU_CREATED,
            orgid = org.id,
            orgname = org.name,
            started = DateTime.Today.AddDays(14),
            unit = "斤",
            cap = 2000,
            created = DateTime.Now,
            creator = prin.name,
            off = 0,
            unitx = 1,
            flashx = 0,
            maxx = 1,
        };

        if (wc.IsGet)
        {
            var fabs = GrabTwinSet<int, int, Fab>(o.orgid);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("助农（货入品控库之前先销售）");

                h.LI_().TEXT("产品名称", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().SELECT("分类", nameof(o.catid), o.catid, cats, required: true)._LI();
                h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("产品源", nameof(o.fabid), o.fabid, fabs)._LI();
                h.LI_().SELECT("限域投放", nameof(o.targs), o.targs, topOrgs, filter: (_, v) => v.IsCenter, capt: v => v.Ext, size: 2, required: false)._LI();
                h.LI_().DATE("输运起始日", nameof(o.started), o.started)._LI();
                h.LI_().SELECT("单位", nameof(o.unit), o.unit, Unit.Typs, keyonly: true, required: true).NUMBER("批次总量", nameof(o.cap), o.cap)._LI();
                h.LI_().NUMBER("每件含量", nameof(o.unitx), o.unitx, min: 1)._LI();

                h._FIELDSUL().FIELDSUL_("销售及优惠");

                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("直降", nameof(o.off), o.off, min: 0.01M, max: 999.99M)._LI();
                h.LI_().NUMBER("秒杀件数", nameof(o.flashx), o.flashx, min: 1, max: o.AvailX).NUMBER("限订件数", nameof(o.maxx), o.maxx, min: 1, max: o.AvailX)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(newpre));

                h._FORM();
            });
        }
        else // POST
        {
            const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
            // populate 
            await wc.ReadObjectAsync(msk, instance: o);
            o.avail = o.cap; // initial available

            // db insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO lots ").colset(Lot.Empty, msk)._VALUES_(Lot.Empty, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

[OrglyAuthorize(Org.TYP_CTR)]
[Ui("产品批次集中盘库", "机构")]
public class CtrlyLotWork : LotWork<CtrlyLotVarWork>
{
    [Ui("统一盘库", group: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE orgid = @1 AND status = 4 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无产品批次");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }
}

public class RtllyPurLotWork : LotWork<RtllyPurLotVarWork>
{
}