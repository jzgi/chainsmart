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
            var cat_ctr_id = Comp(0, org.hubid);
            h.TOOLBAR(subscript: cat_ctr_id);
            if (arr == null)
            {
                h.ALERT("尚无新采购订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "待中转库发货", icon: "chevron-double-right", status: 2), Tool(Anchor)]
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
                h.ALERT("尚无待中转库发货的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已发货", icon: "arrow-right", status: 4), Tool(Anchor)]
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
                h.ALERT("尚无已撤销订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    internal static (short catid, int ctrid) Decomp(int cat_ctr_id) => ((short)(cat_ctr_id >> 24), cat_ctr_id & 0x00ffffff);

    internal static int Comp(short catid, int ctrid) => (catid << 24) | ctrid;

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("新建", "创建采购订单", "plus", status: 1), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int cat_ctr_id) // NOTE so that it is publicly cacheable
    {
        var cats = Grab<short, Cat>();

        (short catid, int ctrid) = Decomp(cat_ctr_id);

        if (catid == 0)
        {
            catid = cats.KeyAt(0);
            wc.Subscript = Comp(catid, ctrid);
        }

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty, alias: "o").T(", d.stock FROM lots_vw o, lotinvs d WHERE o.id = d.lotid AND d.hubid = @2 AND o.status = 4 AND catid = @1");
        var arr = await dc.QueryAsync<Lot>(p => p.Set(catid).Set(ctrid));

        wc.GivePage(200, h =>
        {
            h.TOPBAR_().LOTNAVBAR(nameof(@new), catid, cats, ctrid)._TOPBAR();

            if (arr == null)
            {
                h.ALERT("尚无上线产品");
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

            h.SPAN_("uk-badge").T(o.created, time: 0).SP().T(Pur.Statuses[o.status])._SPAN()._HEADER();
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

    [Ui(tip: "已备发", icon: "eye", status: 2), Tool(Anchor)]
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
                h.ALERT("尚无已备发订单");
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
                h.ALERT("尚无已发货订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已撤单", icon: "trash", status: 8), Tool(Anchor)]
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
                h.ALERT("尚无已撤销订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }
}

[OrglyAuthorize(Org.TYP_MKT)]
[Ui("采购统一收货")]
public class MktlyPurWork : PurWork<MktlyPurVarWork>
{
    [Ui("按产品批次", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT lotid, first(name), count(qty), first(unit) FROM purs WHERE mktid = @1 AND status = 4 GROUP BY lotid LIMIT 30 OFFSET 30 * @2");
        await dc.QueryAsync(p => p.Set(mkt.id).Set(page));

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
    public async Task shop(WebContext wc)
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
}

[OrglyAuthorize(Org.TYP_CTR)]
[Ui("中转库发货")]
public class CtrlyPurWork : PurWork<CtrlyPurVarWork>
{
    [Ui("按产品批次", status: 2), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var ctr = wc[-1].As<Org>();
        var topOrgs = Grab<int, Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT mktid, lotid, last(name), sum(qty) AS qty FROM purs WHERE ctrid = @1 AND status = 2 GROUP BY mktid, lotid ORDER BY mktid");
        await dc.QueryAsync(p => p.Set(ctr.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            h.MAIN_();
            int last = 0;
            while (dc.Next())
            {
                dc.Let(out int mktid);
                dc.Let(out int itemid);
                dc.Let(out string name);
                dc.Let(out decimal qty
                );
                if (mktid != last)
                {
                    var spr = topOrgs[mktid];
                    h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                }

                h.TR_();
                h.TD(name);
                h.TD_("uk-visible@l").T(qty)._TD();
                h._TR();

                last = mktid;
            }

            h._MAIN();
        }, false, 6);
    }

    [Ui("↳", "以往按产品批次", status: 4), Tool(Anchor)]
    public async Task lotpast(WebContext wc)
    {
    }

    [Ui("按市场", status: 8), Tool(Anchor)]
    public async Task mkt(WebContext wc)
    {
        var ctr = wc[-1].As<Org>();
        var topOrgs = Grab<int, Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT mktid, itemid, last(name), sum(qty - qtyre) AS qty FROM purs WHERE ctrid = @1 AND status = 4 GROUP BY mktid, itemid ORDER BY mktid");
        await dc.QueryAsync(p => p.Set(ctr.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            h.MAIN_();
            int last = 0;
            while (dc.Next())
            {
                dc.Let(out int mktid);
                dc.Let(out int itemid);
                dc.Let(out string name);
                dc.Let(out decimal qty
                );
                if (mktid != last)
                {
                    var spr = topOrgs[mktid];
                    h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                }

                h.TR_();
                h.TD(name);
                h.TD_("uk-visible@l").T(qty)._TD();
                h._TR();

                last = mktid;
            }

            h._MAIN();
        }, false, 6);
    }

    [Ui("↳", "以往按市场", status: 16), Tool(Anchor)]
    public async Task mktpast(WebContext wc)
    {
    }

    [Ui("发货", icon: "arrow-right", status: 255), Tool(ButtonOpen)]
    public async Task send(WebContext wc)
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