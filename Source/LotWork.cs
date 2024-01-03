using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Nodality;
using static ChainFX.Web.Modal;
using static ChainFX.Web.ToolAttribute;

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

[MgtAuthorize(Org._BIZ)]
[Ui("产品批次")]
public class SuplyLotWork : LotWork<SuplyLotVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE orgid = @1 AND status = 4 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(state: org.ToState());

            if (arr == null)
            {
                h.ALERT("暂无上线的产品批次");
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
            h.TOOLBAR(state: org.ToState());

            if (arr == null)
            {
                h.ALERT("暂无已下线的产品批次");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }

    [Ui(tip: "已作废", icon: "trash", status: 4), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE orgid = @1 AND status = 0 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(state: org.ToState());

            if (arr == null)
            {
                h.ALERT("暂无已作废的产品批次");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui("云仓", "新建从云仓供应的产品批次", icon: "plus", status: 2), Tool(ButtonOpen)]
    public async Task newhub(WebContext wc)
    {
        await @new(wc, Lot.TYP_HUB);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui("产源", "新建从产源供应的产品批次", icon: "plus", status: 2), Tool(ButtonOpen, state: Org.STA_AAPLUS)]
    public async Task newsrc(WebContext wc)
    {
        await @new(wc, Lot.TYP_SRC);
    }

    async Task @new(WebContext wc, int typ)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var cats = Grab<short, Cat>();

        var o = new Lot
        {
            typ = (short)typ,
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
            var srcs = GrabTwinArray<int, Org>(o.orgid);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().TEXT("产品名", nameof(o.name), o.name, min: 2, max: 12, required: typ == Lot.TYP_SRC)._LI();
                h.LI_().SELECT("分类", nameof(o.cattyp), o.cattyp, cats, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("产源设施", nameof(o.srcid), o.srcid, srcs, required: false)._LI();
                h.LI_().SELECT("零售单位", nameof(o.unit), o.unit, Unit.Typs, showkey: true).SELECT("单位含重", nameof(o.unitw), o.unitw, Unit.Weights)._LI();
                h.LI_().NUMBER("整件", nameof(o.unitx), o.unitx, min: 1, money: false).NUMBER("批次件数", nameof(o.cap), o.cap)._LI();

                h._FIELDSUL().FIELDSUL_("销售参数");

                if (typ == Lot.TYP_SRC)
                {
                    h.LI_().DATE("交货约期", nameof(o.shipon), o.shipon, disabled: true)._LI();
                }
                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("优惠立减", nameof(o.off), o.off, min: 0.00M, max: 999.99M)._LI();
                h.LI_().NUMBER("起订件数", nameof(o.min), o.min, min: 0, max: o.stock).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: o.stock)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", o.typ == Lot.TYP_SRC ? nameof(newsrc) : nameof(newhub));

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
}

public class RtllyPurLotWork : LotWork<RtllyPurLotVarWork>
{
}