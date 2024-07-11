using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;
using static ChainSmart.OrgWatchAttribute;

namespace ChainSmart;

public abstract class BuyWork<V> : WebWork where V : BuyVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }
}

[MgtAuthorize(Org.TYP_SHP)]
[Ui("网售")]
public class ShplyBuyWork : BuyWork<ShplyBuyVarWork>
{
    static void MainGrid(HtmlBuilder h, IEnumerable<Buy> lst, bool pick = false)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, tip: o.uname, css: "uk-card-body uk-flex");

            // the first detail
            var lns = o.lns;

            if (lns == null || lns.Length == 0)
            {
                h.PIC("/void.webp", css: "uk-width-1-5");
            }
            else
            {
                var ln = lns[0]; // buyln
                h.PIC(MainApp.WwwUrl, "/item/", ln.itemid, "/icon",
                    marker: lns.Length > 1 ? "more-vertical" : null,
                    css: "uk-width-1-5"
                );
            }

            h.ASIDE_();
            h.HEADER_().H4(o.uname).SP().SUB(o.utel).SPAN_("uk-badge").T(o.adapted, time: 0).SP();
            if (pick)
            {
                h.PICK(o.Key);
            }
            else
            {
                h.MARK(Buy.Statuses[o.status]);
            }
            h._SPAN()._HEADER();
            h.Q_("uk-width-expand");
            for (int i = 0; i < o.lns?.Length; i++)
            {
                var it = o.lns[i];
                if (i > 0) h.T('；');
                h.T(it.name).SP().T(it.qty).T(it.unit);
            }
            h._Q();
            h.FOOTER_().SPAN(o.uarea, "uk-width-expand").SPAN_("uk-width-1-3 uk-flex-right").CNY(o.pay)._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }


    [OrgWatch(BUY_ADAPTED)]
    [Ui("网售收单", tip: "新收订单", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE orgid = @1 AND status = 2 AND typ = 1 ORDER BY adapted DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id);

            if (arr == null)
            {
                h.ALERT("尚无新收订单");
                return;
            }

            MainGrid(h, arr, pick: true);
        }, false, 6);
    }


    [OrgWatch(BUY_OKED)]
    [Ui(tip: "已派发", icon: "forward", status: 4), Tool(Anchor)]
    public async Task oked(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE orgid = @1 AND status >= 4 AND typ = 1 ORDER BY oked DESC LIMIT 20 OFFSET 20 * @2");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id);

            if (arr == null)
            {
                h.ALERT("尚无已派发订单");
                return;
            }

            MainGrid(h, arr);

            h.PAGINATION(arr.Length == 20);
        }, false, 6);
    }


    [Ui(tip: "已撤销", icon: "trash", status: 8), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE orgid = @1 AND status = 0 AND typ = 1 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

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
}

[MgtAuthorize(Org.TYP_MKV)]
[Ui("网售统一派发")]
public class MktlyBuyWork : BuyWork<MktlyBuyVarWork>
{
    internal void MainGrid(HtmlBuilder h, IList<Buy> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.UL_("uk-card-body uk-list uk-list-divider");
            h.LI_().H4(o.utel).SPAN_("uk-badge").T(o.created, time: 0).SP().T(Buy.Statuses[o.status])._SPAN()._LI();

            foreach (var it in o.lns)
            {
                h.LI_();

                h.SPAN_("uk-width-expand").T(it.name);
                if (it.unitip != null)
                {
                    h.SP().SMALL_().T(it.unitip).T(it.unit)._SMALL();
                }
                h._SPAN();

                h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.RealPrice).SP().SUB(it.unit)._SPAN();
                h.SPAN_("uk-width-tiny uk-flex-right").T(it.qty).SP().T(it.unit)._SPAN();
                h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.SubTotal)._SPAN();
                h._LI();
            }
            h._LI();

            h._UL();
        });
    }

    [Ui("网售统一派发", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT uarea, count(id) FROM buys WHERE mktid = @1 AND typ = 1 AND status = 2 GROUP BY uarea");
        await dc.QueryAsync(p => p.Set(mkt.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("社区").TH("收单", css: "uk-width-tiny")._THEAD();

            while (dc.Next())
            {
                dc.Let(out string uarea);
                dc.Let(out int adapted);

                string ucomlabel = string.IsNullOrEmpty(uarea) ? "非派送区" : uarea;

                h.TR_();
                h.TD_().ADIALOG_(string.IsNullOrEmpty(uarea) ? "_" : uarea, "/area", mode: ToolAttribute.MOD_OPEN, false, tip: ucomlabel, css: "uk-link uk-button-link").T(ucomlabel)._A()._TD();
                h.TD_(css: "uk-text-center");
                if (adapted > 0)
                {
                    h.T(adapted);
                }
                h._TD();
                h._TR();
            }

            h._TABLE();
        });
    }

    public async Task lst(WebContext wc)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT first(name), count(id) FROM buys WHERE mktid = @1 AND typ = 1 AND status = 2 GROUP BY orgid");
        await dc.QueryAsync(p => p.Set(mkt.id));

        const int PAGESIZ = 5;

        wc.GivePage(200, h =>
        {
            h.T("<main uk-slider=\"autoplay: true; utoplay-interval: 6000; pause-on-hover: true; center: true\">");
            h.UL_("uk-slider-items uk-grid uk-child-width-1-1");

            int num = 0;

            while (dc.Next())
            {
                dc.Let(out string name);
                dc.Let(out int adapted);

                if (num % PAGESIZ == 0)
                {
                    if (num > 0)
                    {
                        h._TABLE();
                        h._LI();
                    }
                    h.LI_();
                    h.TABLE_(dark: true);
                    h.THEAD_().TH("商户").TH("收单", css: "uk-width-medium uk-text-center").TH("合单", css: "uk-width-medium uk-text-center")._THEAD();
                }

                // each row
                //
                h.TR_();
                h.TD_().T(name)._TD();
                h.TD(adapted, right: null);
                h._TR();

                num++;
            }

            if (num > 0)
            {
                h._TABLE();
                h._LI();
            }
            h._UL();

            if (num == 0)
            {
                h.DIV_(css: "uk-position-center uk-text-xlarge uk-text-success").T("当前无在处理订单")._DIV();
            }

            h._MAIN();
        }, false, 12, title: mkt.Whole, refresh: 60);
    }

    [Ui(tip: "已统一派送", icon: "arrow-right", status: 4), Tool(AnchorPrompt)]
    public async Task oked(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        string com;

        bool inner = wc.Query[nameof(inner)];
        if (inner)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_(css: "uk-card uk-card-primary uk-card-body");

                var specs = org?.specs;
                for (int i = 0; i < specs?.Count; i++)
                {
                    var spec = specs.EntryAt(i);
                    var v = spec.Value;
                    if (v.IsObject)
                    {
                        h.FIELDSUL_(spec.Key, css: "uk-list uk-list-divider");

                        var sub = (JObj)v;
                        for (int k = 0; k < sub.Count; k++)
                        {
                            var e = sub.EntryAt(k);

                            h.LI_().RADIO(nameof(com), e.Key, e.Key)._LI();
                        }

                        h._FIELDSUL();
                    }
                }
                h._FORM();
            });
        }
        else // OUTER
        {
            com = wc.Query[nameof(com)];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE mktid = @1 AND status = 4 AND (typ = 1 AND adapter IS NOT NULL) AND ucom = @2 ORDER BY oked DESC LIMIT 20 OFFSET 20 * @3");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id).Set(com).Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("没有找到记录");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.UL_("uk-card-body uk-list uk-list-divider");
                    h.LI_().H4(o.name).SPAN_("uk-badge").T(o.oked, date: 2, time: 2).SP().T(Buy.Statuses[o.status])._SPAN()._LI();

                    foreach (var it in o.lns)
                    {
                        h.LI_();

                        h.SPAN_("uk-width-expand").T(it.name);
                        if (it.unitip != null)
                        {
                            h.SP().SMALL_().T(it.unitip)._SMALL();
                        }

                        h._SPAN();

                        h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.RealPrice)._SPAN();
                        h.SPAN_("uk-width-tiny uk-flex-right").T(it.qty).SP().T(it.unit)._SPAN();
                        h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.SubTotal)._SPAN();
                        h._LI();
                    }
                    h._LI();

                    h.LI_();
                    h.SPAN_("uk-width-expand").SMALL_().T(o.uarea).T(o.uaddr)._SMALL()._SPAN();
                    if (o.fee > 0)
                    {
                        h.SMALL_().T("派送到楼下 +").T(o.fee)._SMALL();
                    }
                    h.SPAN_("uk-width-1-5 uk-flex-right").CNY(o.topay)._SPAN();
                    h._LI();

                    h._UL();
                });

                h.PAGINATION(arr.Length == 20);
            }, false, 6);
        }
    }
}