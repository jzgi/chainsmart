using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;
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
            short catmsk = 0;
            if (org.IsMarket)
            {
                catmsk = 0xff;
            }
            else
            {
                var reg = Grab<short, Reg>()[org.regid];
                catmsk = reg.catmsk;
            }

            var reg_ctr_id = Comp(catmsk, org.hubid);
            h.TOOLBAR(subscript: reg_ctr_id);
            if (arr == null)
            {
                h.ALERT("尚无新采购订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "要品控仓备货", icon: "chevron-double-right", status: 2), Tool(Anchor)]
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
                h.ALERT("尚无要品控仓备货的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已由品控仓发货", icon: "arrow-right", status: 4), Tool(Anchor)]
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
                h.ALERT("尚无已发货订单");
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

    internal static (short catmsk, int ctrid) Decomp(int catmsk_ctrid) => ((short)(catmsk_ctrid >> 20), catmsk_ctrid & 0x000fffff);

    internal static int Comp(short catmsk, int ctrid) => (catmsk << 20) | ctrid;

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("新建", "创建采购订单", "plus", status: 1), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int catmsk_ctrid) // NOTE so that it is publicly cacheable
    {
        (short catmsk, int ctrid) = Decomp(catmsk_ctrid);

        var ctr = GrabTwin<int, Org>(ctrid);

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty, alias: "o").T(", d.stock FROM lots_vw o, lotinvs d WHERE o.id = d.lotid AND d.hubid = @1 AND o.status = 4 AND o.cattyp & @2 > 0");
        var arr = await dc.QueryAsync<Lot>(p => p.Set(ctrid).Set(catmsk));

        wc.GivePage(200, h =>
        {
            if (arr == null)
            {
                h.ALERT("尚无上线产品投放", $"{ctr.cover}大区，当前市场版块");
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
        }, true, 60); // NOTE we deliberately make the pages publicly cacheable though within a private context
    }
}

public class SuplyPurWork : PurWork<SuplyPurVarWork>
{
    // timer that automatically transfers orders 
    const uint FIVE_MINUTES = 1000 * 300;

    static readonly Timer TIMER = new(AutoProcess, null, FIVE_MINUTES, FIVE_MINUTES);

    static async void AutoProcess(object x)
    {
        using var dc = NewDbContext();
        dc.Sql("");
        await dc.ExecuteAsync();
    }

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


    private short PurTyp => (short)State;

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

    [Ui(tip: "要品控仓备货", icon: "chevron-double-right", status: 2), Tool(Anchor)]
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
                h.ALERT("尚无要品控仓备货的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [OrgSpy(PUR_OKED)]
    [Ui(tip: "已发货", icon: "arrow-right", status: 4), Tool(Anchor)]
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
                h.ALERT("尚无已发货的订单");
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

    [Ui("备货", icon: "chevron-double-right", status: 1), Tool(ButtonPickShow)]
    public async Task adapt(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        int[] key;

        if (wc.IsGet)
        {
            key = wc.Query[nameof(key)];

            wc.GivePane(200, h =>
            {
                h.SECTION_("uk-card uk-card-primary");
                h.H2("要品控仓备货", css: "uk-card-header");
                h.DIV_("uk-card-body").T("要品控仓备货")._DIV();
                h._SECTION();

                h.FORM_("uk-card uk-card-primary uk-margin-top");
                foreach (var k in key)
                {
                    h.HIDDEN(nameof(key), k);
                }
                h.BOTTOM_BUTTON("确认", nameof(adapt), post: true);
                h._FORM();
            });
        }
        else
        {
            var f = await wc.ReadAsync<Form>();
            key = f[nameof(key)];

            using var dc = NewDbContext();
            dc.Sql("UPDATE purs SET adapted = @1, adapter = @2, status = 2 WHERE supid = @3 AND id ")._IN_(key).T(" AND status = 1");
            await dc.ExecuteAsync(p =>
            {
                p.Set(DateTime.Now).Set(prin.name).Set(org.id);
                p.SetForIn(key);
            });

            wc.GivePane(200);
        }
    }
}

[OrglyAuthorize(Org.TYP_MKT)]
[Ui("采购统一收货")]
public class MktlyPurWork : PurWork<MktlyPurVarWork>
{
    [Ui("采购收货", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT lotid, first(name), count(qty), first(unitx), first(unit) FROM purs WHERE mktid = @1 AND status = 4 GROUP BY lotid LIMIT 30 OFFSET 30 * @2");
        await dc.QueryAsync(p => p.Set(mkt.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("产品").TH("件数", css: "uk-text-right").TH("每件", "uk-width-tiny")._THEAD();

            int n = 0;
            while (dc.Next())
            {
                dc.Let(out int lotid);
                dc.Let(out string name);
                dc.Let(out decimal qty);
                dc.Let(out short unitx);
                dc.Let(out string unit);
                h.TR_();
                h.TD(name);
                h.TD_("uk-text-right").T(qty).SP().T('件');
                h.TD_("uk-text-right").SMALL_().T(unitx).SP().T(unit)._SMALL()._TD();
                h._TR();
                n++;
            }

            h._TABLE();

            h.PAGINATION(n == 30);
        }, false, 6);
    }

    [Ui(tip: "历史", icon: "history", status: 2), Tool(Anchor)]
    public async Task past(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT rtlid, first(rtlname) AS rtlname, count(id) AS count FROM purs WHERE mktid = @1 AND status = 4 GROUP BY rtlid");
        var arr = await dc.QueryAsync<PurAgg>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE(arr, o =>
            {
                h.TD_();
                h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");
                h._A();
                h._TD();
            });
        }, false, 6);
    }

    [Ui("按商户", status: 4), Tool(Anchor)]
    public async Task rtl(WebContext wc)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT rtlid, first(rtlname), count(qty) AS qty FROM purs WHERE mktid = @1 AND status > 0 GROUP BY rtlid, lotid");
        var arr = await dc.QueryAsync<PurAgg>(p => p.Set(mkt.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            int n = 0;
            while (dc.Next())
            {
                dc.Let(out int lotid);
                dc.Let(out string name);
                dc.Let(out decimal qty);
                dc.Let(out string unit);
                h.TR_();
                h.TD(name);
                h.TD_("uk-visible@l").T(qty).SP().T(unit)._TD();
                h._TR();
                n++;
            }

            h._TABLE();

            h.PAGINATION(n == 30);
        }, false, 6);
    }

    [Ui("收货", icon: "download", status: 1), Tool(ButtonOpen)]
    public async Task end(WebContext wc)
    {
        var prin = (User)wc.Principal;
        short orgid = wc[-1];
        short typ = 0;
        decimal amt = 0;
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("指定统计区间");
                h._FIELDSUL()._FORM();
            });
        }
        else // POST
        {
            wc.GivePane(200); // close dialog
        }
    }
}

[OrglyAuthorize(Org.TYP_CTR)]
[Ui("品控仓统一备发")]
public class CtrlyPurWork : PurWork<CtrlyPurVarWork>
{
    [Ui("统一备发", status: 8), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var hub = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT mktid, sum(CASE WHEN status = 2 THEN qty END), count(CASE WHEN status = 4 THEN qty END) FROM purs WHERE hubid = @1 AND (status = 2 OR status = 4) GROUP BY mktid");
        await dc.QueryAsync(p => p.Set(hub.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("市场").TH("备货", css: "uk-width-tiny").TH("发货", css: "uk-width-tiny")._THEAD();

            while (dc.Next())
            {
                dc.Let(out int mktid);
                dc.Let(out int adapted);
                dc.Let(out int oked);

                var mkt = GrabTwin<int, Org>(mktid);

                h.TR_();
                h.TD_().ADIALOG_(mktid, "/mkt", mode: ToolAttribute.MOD_OPEN, false, tip: mkt.Cover, css: "uk-link uk-button-link").T(mkt.Cover)._A()._TD();
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