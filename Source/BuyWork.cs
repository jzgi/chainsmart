using System;
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
            var items = o.lns;

            if (items == null || items.Length == 0)
            {
                h.PIC("/void.webp", css: "uk-width-1-5");
            }
            else
            {
                var bi = items[0]; // buyitem
                h.PIC(MainApp.WwwUrl, "/item/", bi.itemid, "/icon",
                    marker: items.Length > 1 ? "more-vertical" : null,
                    css: "uk-width-1-5"
                );
            }

            h.ASIDE_();
            h.HEADER_().H4(o.uname).SPAN_("uk-badge").T(o.created, time: 0).SP();
            if (pick)
            {
                h.PICK(o.Key);
            }
            else
            {
                h.T(Buy.Statuses[o.status]);
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
            h.FOOTER_().SPAN(string.IsNullOrEmpty(o.uarea) ? "非派送区" : o.uarea, "uk-width-expand").SPAN(o.utel, "uk-width-1-3 uk-output").SPAN_("uk-width-1-3 uk-flex-right").CNY(o.pay)._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }


    [OrgWatch(BUY_CREATED)]
    [Ui(tip: "新订单", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE orgid = @1 AND status = 1 AND typ = 1 ORDER BY created DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id);

            if (arr == null)
            {
                h.ALERT("尚无新订单");
                return;
            }

            MainGrid(h, arr, pick: true);
        }, false, 6);
    }

    [Ui(tip: "已合单", icon: "chevron-double-right", status: 2), Tool(Anchor)]
    public async Task adapted(WebContext wc)
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
                h.ALERT("尚无已合单的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [OrgWatch(BUY_OKED)]
    [Ui(tip: "已派发", icon: "arrow-right", status: 4), Tool(Anchor)]
    public async Task after(WebContext wc, int page)
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
                h.ALERT("尚无已派发的订单");
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
                h.ALERT("尚无已撤销的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui("合单", icon: "chevron-double-right", status: 1), Tool(ButtonPickShow)]
    public async Task adapt(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        int[] key;
        bool print = false;

        if (wc.IsGet)
        {
            key = wc.Query[nameof(key)];

            wc.GivePane(200, h =>
            {
                h.SECTION_("uk-card uk-card-primary");
                h.H2("送存到合单区", css: "uk-card-header");
                h.DIV_("uk-card-body").T("将备好的货贴上派送标签或小票，送存到合单区，等待统一派送")._DIV();
                h._SECTION();

                h.FORM_("uk-card uk-card-primary uk-margin-top");
                foreach (var k in key)
                {
                    h.HIDDEN(nameof(key), k);
                }
                h.DIV_("uk-card-body").CHECKBOX(null, nameof(print), true, tip: "使用共享机打印小票", disabled: !print)._DIV();
                h.BOTTOM_BUTTON("确认", nameof(adapt), post: true);
                h._FORM();
            });
        }
        else
        {
            var f = await wc.ReadAsync<Form>();
            key = f[nameof(key)];
            print = f[nameof(print)];

            using var dc = NewDbContext();
            dc.Sql("UPDATE buys SET adapted = @1, adapter = @2, status = 2 WHERE orgid = @3 AND id ")._IN_(key).T(" AND status = 1");
            await dc.ExecuteAsync(p =>
            {
                p.Set(DateTime.Now).Set(prin.name).Set(org.id);
                p.SetForIn(key);
            });

            wc.GivePane(200);
        }
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
        dc.Sql("SELECT ucom, count(CASE WHEN status = 1 THEN 1 END), count(CASE WHEN status = 2 THEN 2 END) FROM buys WHERE mktid = @1 AND typ = 1 AND (status = 1 OR status = 2) GROUP BY ucom");
        await dc.QueryAsync(p => p.Set(mkt.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("社区").TH("收单", css: "uk-width-tiny").TH("合单", css: "uk-width-tiny")._THEAD();

            while (dc.Next())
            {
                dc.Let(out string ucom);
                dc.Let(out int created);
                dc.Let(out int adapted);

                string ucomlabel = string.IsNullOrEmpty(ucom) ? "非派送区" : ucom;

                h.TR_();
                h.TD_().ADIALOG_(string.IsNullOrEmpty(ucom) ? "_" : ucom, "/com", mode: ToolAttribute.MOD_OPEN, false, tip: ucomlabel, css: "uk-link uk-button-link").T(ucomlabel)._A()._TD();
                h.TD_(css: "uk-text-center");
                if (created > 0)
                {
                    h.T(created);
                }
                h._TD();
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
        dc.Sql("SELECT first(name), count(CASE WHEN status = 1 THEN 1 END), count(CASE WHEN status = 2 THEN 2 END) FROM buys WHERE mktid = @1 AND typ = 1 AND (status = 1 OR status = 2) GROUP BY orgid");
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
                dc.Let(out int created);
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
                h.TD(created, right: null);
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
        }, false, 12, title: mkt.Full, refresh: 60);
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