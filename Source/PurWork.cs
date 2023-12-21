using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Nodality;
using static ChainFX.Web.ToolAttribute;
using static ChainSmart.OrgNoticePack;

namespace ChainSmart;

public abstract class PurWork<V> : WebWork where V : PurVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }
}

[Ui("采购")]
public class RtllyPurWork : PurWork<RtllyPurVarWork>
{
    protected override void OnCreate()
    {
        base.OnCreate();

        // add sub work for purchase creation
        CreateWork<RtllyPurLotWork>("lot");
    }


    static void MainGrid(HtmlBuilder h, IList<Pur> lst)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

            h.PIC(MainApp.WwwUrl, "/lot/", o.lotid, "/icon", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name);
            if (o.unitx != 1)
            {
                h.SP().SMALL_().T(o.unitx).T(o.unit).T("件")._SMALL();
            }

            h.SPAN_("uk-badge").T(o.created, time: 0).SP().T(Pur.Statuses[o.status])._SPAN()._HEADER();
            h.Q_("uk-width-expand").T(o.ctrid)._Q();
            h.FOOTER_().SPAN_("uk-width-1-3").CNY(o.RealPrice)._SPAN().SPAN_("uk-width-1-3").T(o.QtyX).SP().T("件").SP().T(o.qty).SP().T(o.unit)._SPAN().SPAN_("uk-margin-auto-left").CNY(o.Total)._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }

    [Ui("采购订单", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Pur.Empty).T(" FROM purs WHERE rtlid = @1 AND status = 1 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Pur>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            var hubid_cat = Comp(org.hubid, 0);
            h.TOOLBAR(subscript: hubid_cat);
            if (arr == null)
            {
                h.ALERT("尚无新采购订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已发货", icon: "arrow-right", status: 2), Tool(Anchor)]
    public async Task adapted(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Pur.Empty).T(" FROM purs WHERE rtlid = @1 AND status = 2 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Pur>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id);
            if (arr == null)
            {
                h.ALERT("尚无已发货的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已收货", icon: "sign-in", status: 4), Tool(Anchor)]
    public async Task oked(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Pur.Empty).T(" FROM purs WHERE rtlid = @1 AND status = 4 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Pur>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无已收货的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [OrgSpy(PUR_VOID)]
    [Ui(tip: "已撤销", icon: "trash", status: 8), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Pur.Empty).T(" FROM purs WHERE rtlid = @1 AND status = 0 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Pur>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id);
            if (arr == null)
            {
                h.ALERT("尚无已撤销的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    internal static (int hubid, short cat) Decomp(int hubid_cat) => ((hubid_cat >> 8), (short)(hubid_cat & 0xff));

    internal static int Comp(int hubid, short cat) => (hubid << 8) | (int)cat;

    [UserAuthorize(Org.TYP_RTL, User.ROL_OPN)]
    [Ui("云仓下单", "新建采购订单", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int hubid_cat) // NOTE publicly cacheable
    {
        var cats = Grab<short, Cat>();

        var (hubid, cat) = Decomp(hubid_cat);
        if (cat == 0)
        {
            cat = cats.KeyAt(0);
            wc.Subscript = hubid_cat = Comp(hubid, cat);
        }

        const short Msk = 0xff | Entity.MSK_EXTRA;
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty, Msk, alias: "o").T(", d.stock FROM lots_vw o, lotinvs d WHERE o.id = d.lotid AND d.hubid = @1 AND o.status = 4 AND typ = 1 AND o.cattyp & @2 > 0");
        var arr = await dc.QueryAsync<Lot>(p => p.Set(hubid).Set(cat), Msk);

        wc.GivePage(200, h =>
        {
            h.NAVBAR(nameof(@new), cat, cats, (_, v) => v.IsEnabled, (k) => Comp(hubid, k));

            if (arr == null)
            {
                h.ALERT("尚无上线的产品");
                return;
            }

            h.MAINGRID(arr, o =>
            {
                // anchor to the lot sub work
                h.ADIALOG_("lot/", o.Key, "/", MOD_SHOW, false, tip: o.name, css: "uk-card-body uk-flex");

                h.PIC(MainApp.WwwUrl, "/lot/", o.id, "/icon", css: "uk-width-1-5");

                h.ASIDE_();
                h.HEADER_().H4(o.name).SPAN(Entity.Statuses[o.status], "uk-badge")._HEADER();
                h.Q(o.tip, "uk-width-expand");
                h.FOOTER_().T("每件").SP().T(o.unitx).SP().T(o.unit).SPAN_("uk-margin-auto-left").CNY(o.price)._SPAN()._FOOTER();
                h._ASIDE();

                h._A();
            });
        }, true, 120); // NOTE we deliberately make the pages publicly cacheable though within a private context
    }
}

public class SuplyPurWork : PurWork<SuplyPurVarWork>
{
    internal short PurTyp => (short)State;

    private static void MainGrid(HtmlBuilder h, IList<Pur> lst)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

            h.PIC(MainApp.WwwUrl, "/lot/", o.lotid, "/icon", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name);
            if (o.unitx != 1)
            {
                h.SP().SMALL_().T(o.unitx).T(o.unit).T("件")._SMALL();
            }

            var rtl = GrabTwin<int, Org>(o.rtlid);

            h.SPAN_("uk-badge").T(o.created, time: 0)._SPAN().SP().PICK(o.Key)._HEADER();
            h.Q_("uk-width-expand").T(rtl.name)._Q();
            h.FOOTER_().SPAN_("uk-width-1-3").CNY(o.RealPrice)._SPAN().SPAN_("uk-width-1-3").T(o.QtyX).SP().T("件").SP().T(o.qty).SP().T(o.unit)._SPAN().SPAN_("uk-margin-auto-left").CNY(o.Total)._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }


    [OrgSpy(PUR_CREATED)]
    [Ui("销售订单"), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Pur.Empty).T(" FROM purs WHERE typ = @1 AND supid = @2 AND status = 1 ORDER BY created DESC");
        var arr = await dc.QueryAsync<Pur>(p => p.Set(PurTyp).Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id);
            if (arr == null)
            {
                h.ALERT("尚无新订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "开始发货", icon: "chevron-double-right", status: 2), Tool(Anchor)]
    public async Task adapted(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Pur.Empty).T(" FROM purs WHERE typ = @1 AND supid = @2 AND status = 2 ORDER BY adapted DESC");
        var arr = await dc.QueryAsync<Pur>(p => p.Set(PurTyp).Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无开始发货的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [OrgSpy(PUR_OKED)]
    [Ui(tip: "已收货", icon: "arrow-right", status: 4), Tool(Anchor)]
    public async Task oked(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Pur.Empty).T(" FROM purs WHERE typ = @1 AND supid = @2 AND status = 4 ORDER BY oked DESC");
        var arr = await dc.QueryAsync<Pur>(p => p.Set(PurTyp).Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id);
            if (arr == null)
            {
                h.ALERT("尚无已收货的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已撤销", icon: "trash", status: 8), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Pur.Empty).T(" FROM purs WHERE typ = @1 AND supid = @2 AND status = 0 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Pur>(p => p.Set(PurTyp).Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无已撤销的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }
}

[UserAuthorize(Org.TYP_CTR)]
[Ui("云仓统一发货")]
public class CtrlyPurWork : PurWork<CtrlyPurVarWork>
{
    [Ui("统一发货", status: 8), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var hub = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT mktid, sum(CASE WHEN status = 1 THEN (qty / unitx) END), sum(CASE WHEN status = 2 THEN (qty / unitx) END) FROM purs WHERE hubid = @1 AND (status = 1 OR status = 2) GROUP BY mktid");
        await dc.QueryAsync(p => p.Set(hub.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("市场").TH("收单", css: "uk-width-tiny").TH("发货", css: "uk-width-tiny")._THEAD();

            while (dc.Next())
            {
                dc.Let(out int mktid);
                dc.Let(out int adapted);
                dc.Let(out int oked);

                var mkt = GrabTwin<int, Org>(mktid);

                h.TR_();
                h.TD_().ADIALOG_(mktid, "/mkt", mode: MOD_OPEN, false, tip: mkt.Cover, css: "uk-link uk-button-link").T(mkt.Cover)._A()._TD();
                h.TD_(css: "uk-text-center");
                if (adapted > 0)
                {
                    h.T(adapted);
                }
                h._TD();
                h.TD_(css: "uk-text-center");
                if (oked > 0)
                {
                    h.T(oked);
                }
                h._TD();
                h._TR();
            }

            h._TABLE();
        }, false, 6);
    }

    [Ui(tip: "以往按市场", icon: "history", status: 2), Tool(Anchor)]
    public async Task past(WebContext wc)
    {
    }
}

[UserAuthorize(Org.TYP_MKT)]
[Ui("采购统一收货（云仓）")]
public class MktlyPurWork : PurWork<MktlyPurVarWork>
{
    [Ui("采购统一收货", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT rtlid, sum(qty / unitx) FROM purs WHERE mktid = @1 AND status = 2 AND typ = 1 GROUP BY rtlid");
        await dc.QueryAsync(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("商户").TH("发货", css: "uk-text-right")._THEAD();

            while (dc.Next())
            {
                dc.Let(out int rtlid);
                dc.Let(out decimal qtyx);

                var rtl = GrabTwin<int, Org>(rtlid);

                h.TR_();
                h.TD(rtl.name);
                h.TD_().ADIALOG_(rtlid, "/rtl", mode: MOD_SHOW, false, css: "uk-link uk-button-link uk-flex-right").T(qtyx)._A()._TD();
                h._TR();
            }
            h._TABLE();
        }, false, 6);
    }

    [Ui(tip: "按产品批次", icon: "list", status: 2), Tool(Anchor)]
    public async Task lot(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT first(name), first(unitx), first(unit), sum(qty / unitx) FROM purs WHERE mktid = @1 AND status = 2 AND typ = 1 GROUP BY lotid");
        await dc.QueryAsync(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("产品").TH("发货", css: "uk-text-right")._THEAD();

            while (dc.Next())
            {
                dc.Let(out string name);
                dc.Let(out short unitx);
                dc.Let(out string unit);
                dc.Let(out decimal qtyx);
                h.TR_();
                h.TD_().T(name).SMALL_().T('（').T(unitx).T(unit).T('）')._SMALL()._TD();
                h.TD_().ADIALOG_("?utel=", "", mode: MOD_SHOW, false, css: "uk-link uk-button-link uk-flex-right").T(qtyx)._A()._TD();
                h._TR();
            }
            h._TABLE();
        }, false, 6);
    }

    static readonly string[] DAYS = { "日", "一", "二", "三", "四", "五", "六" };

    [Ui(tip: "历史记录", icon: "history", status: 4), Tool(AnchorPrompt)]
    public async Task past(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        var today = DateTime.Today;
        int days; //  date offset

        bool inner = wc.Query[nameof(inner)];
        if (inner)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(css: "uk-list uk-list-divider");
                for (days = 0; days < 7; days++)
                {
                    var dt = today.AddDays(-days);
                    h.LI_().RADIO(nameof(days), days, dt.ToString("yy-MM-dd")).SP();
                    if (days == 0)
                    {
                        h.T("今天");
                    }
                    else
                    {
                        h.T("周").T(DAYS[(int)dt.DayOfWeek]);
                    }
                    h._LI();
                }
                h._FIELDSUL()._FORM();
            });
        }
        else // OUTER
        {
            days = wc.Query[nameof(days)];

            var dt = today.AddDays(days);

            using var dc = NewDbContext();
            dc.Sql("SELECT rtlid, sum(qty / unitx) FROM purs WHERE mktid = @1 AND status = 4 AND typ = 1 AND (oked >= @2 AND oked < @3) GROUP BY rtlid");
            await dc.QueryAsync(p => p.Set(org.id).Set(dt).Set(dt.AddDays(1)));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.TABLE_();
                h.THEAD_().TH("商户").TH("件数", css: "uk-text-right")._THEAD();

                while (dc.Next())
                {
                    dc.Let(out int rtlid);
                    dc.Let(out int qtyx);

                    var rtl = GrabTwin<int, Org>(rtlid);

                    h.TR_();
                    h.TD(rtl.name);
                    h.TD_().ADIALOG_(rtlid, "/rtl", mode: MOD_SHOW, false, css: "uk-link uk-button-link uk-flex-right").T(qtyx)._A()._TD();
                    h._TR();
                }
                h._TABLE();
            }, false, 6);
        }
    }
}