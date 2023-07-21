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

    [Ui("硬质标签")]
    public void @default(WebContext wc)
    {
        int num = 0;
        wc.GivePage(200, h =>
        {
            h.TOPBAR_("uk-padding-left").SUBNAV(TAGS)._TOPBAR();
            //
            h.FORM_("uk-card uk-card-primary").FIELDSUL_("预制的硬质标签");
            h.LI_().NUMBER("溯源编号", nameof(num), num)._LI();
            h.LI_("uk-flex-center").BUTTON("查询")._LI();
            h._FIELDSUL();
            h._FORM();
        }, shared: true, 120, title: "产品批次溯源查询");
    }

    [Ui("纸质贴标")]
    public void label(WebContext wc)
    {
        int num = 0;
        wc.GivePage(200, h =>
        {
            h.TOPBAR_("uk-padding-left").SUBNAV(TAGS)._TOPBAR();
            //
            h.FORM_("uk-card uk-card-primary").FIELDSUL_("印制的纸质贴标");
            h.LI_().NUMBER("溯源编号", nameof(num), num)._LI();
            h.LI_("uk-flex-center").BUTTON("查询")._LI();
            h._FIELDSUL();
            h._FORM();
        }, shared: true, 120, title: "产品批次溯源查询");
    }
}

[Ui("批次")]
public class SuplyLotWork : LotWork<SuplyLotVarWork>
{
    [Ui("批次", status: 1), Tool(Anchor)]
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
                h.ALERT("暂无上线批次");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }

    [Ui(tip: "已下线", icon: "cloud-download", status: 2), Tool(Anchor)]
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
                h.ALERT("暂无已下线批次");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }

    [Ui(tip: "已删除", icon: "trash", status: 4), Tool(Anchor)]
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
                h.ALERT("暂无已删除批次");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("品控仓", "新建从品控仓发售的批次", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task newhub(WebContext wc, int typ)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var cats = Grab<short, Cat>();

        var o = new Lot
        {
            typ = Lot.TYP_HUB,
            status = Entity.STU_CREATED,
            orgid = org.id,
            unit = "斤",
            created = DateTime.Now,
            creator = prin.name,
            off = 0,
            unitx = 1,

            cap = 2000,
            min = 1,
            max = 100,
        };

        if (wc.IsGet)
        {
            var srcs = GrabTwinSet<int, Src>(o.orgid);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("批次信息");

                h.LI_().TEXT("产品名", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().SELECT("分类", nameof(o.cattyp), o.cattyp, cats, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("产源设施", nameof(o.srcid), o.srcid, srcs)._LI();
                h.LI_().SELECT("零售单位", nameof(o.unit), o.unit, Unit.Typs, showkey: true).SELECT("单位含重", nameof(o.unitw), o.unitw, Unit.Metrics)._LI();
                h.LI_().NUMBER("整件", nameof(o.unitx), o.unitx, min: 1, money: false).NUMBER("批次件数", nameof(o.cap), o.cap)._LI();

                h._FIELDSUL().FIELDSUL_("销售参数");

                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("优惠立减", nameof(o.off), o.off, min: 0.00M, max: 999.99M)._LI();
                h.LI_().NUMBER("起订件数", nameof(o.min), o.min, min: 0, max: o.stock).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: o.stock)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(newhub));

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
    [Ui("产源", "新建从产源发售的批次", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task newsrc(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var cats = Grab<short, Cat>();

        var o = new Lot
        {
            typ = Lot.TYP_SRC,
            status = Entity.STU_CREATED,
            orgid = org.id,
            started = DateTime.Today.AddDays(14),
            unit = "斤",
            created = DateTime.Now,
            creator = prin.name,
            off = 0,
            unitx = 1,

            cap = 2000,
            min = 1,
            max = 100,
        };

        if (wc.IsGet)
        {
            var srcs = GrabTwinSet<int, Src>(o.orgid);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("批次信息");

                h.LI_().TEXT("产品名", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().SELECT("分类", nameof(o.cattyp), o.cattyp, cats, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("产源设施", nameof(o.srcid), o.srcid, srcs)._LI();
                h.LI_().DATE("交货起始日", nameof(o.started), o.started)._LI();
                h.LI_().SELECT("零售单位", nameof(o.unit), o.unit, Unit.Typs, showkey: true).SELECT("单位含重", nameof(o.unitw), o.unitw, Unit.Metrics)._LI();
                h.LI_().NUMBER("整件", nameof(o.unitx), o.unitx, min: 1, money: false).NUMBER("批次件数", nameof(o.cap), o.cap)._LI();

                h._FIELDSUL().FIELDSUL_("销售参数");

                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("优惠立减", nameof(o.off), o.off, min: 0.00M, max: 999.99M)._LI();
                h.LI_().NUMBER("起订件数", nameof(o.min), o.min, min: 0, max: o.stock).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: o.stock)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(newsrc));

                h._FORM();
            });
        }
        else // POST
        {
            const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
            // populate 
            await wc.ReadObjectAsync(msk, instance: o);
            o.stock = o.cap; // initial inventory

            // db insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO lots ").colset(Lot.Empty, msk)._VALUES_(Lot.Empty, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

public class RtllyPurLotWork : LotWork<RtllyPurLotVarWork>
{
}