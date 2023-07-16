using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;
using static ChainSmart.OrgNoticePack;

namespace ChainSmart;

public abstract class BuyWork<V> : WebWork where V : BuyVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }
}

[Ui("网售订单")]
public class RtllyBuyWork : BuyWork<RtllyBuyVarWork>
{
    static void MainGrid(HtmlBuilder h, IList<Buy> lst)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, tip: o.uname, css: "uk-card-body uk-flex");

            // the first detail
            var items = o.items;

            if (items == null || items.Length == 0)
            {
                h.PIC("/void.webp", css: "uk-width-1-5");
            }
            else if (items.Length == 1)
            {
                var bi = items[0]; // buyitem
                if (bi.lotid > 0)
                {
                    h.PIC(MainApp.WwwUrl, "/lot/", bi.lotid, "/icon", css: "uk-width-1-5");
                }
                else
                    h.PIC(MainApp.WwwUrl, "/item/", bi.itemid, "/icon", css: "uk-width-1-5");
            }
            else
            {
                h.PIC("/solid.webp", css: "uk-width-1-5");
            }

            h.ASIDE_();
            h.HEADER_().H4(o.uname).SPAN_("uk-badge").T(o.created, time: 0)._SPAN().SP().PICK(o.Key)._HEADER();
            h.Q_("uk-width-expand");
            for (int i = 0; i < o.items?.Length; i++)
            {
                var it = o.items[i];
                if (i > 0) h.T('；');
                h.T(it.name).SP().T(it.qty).T(it.unit);
            }
            h._Q();
            h.FOOTER_().SPAN(string.IsNullOrEmpty(o.ucom) ? "非派送区" : o.ucom, "uk-width-expand").SPAN(o.utel, "uk-width-1-3 uk-output").SPAN_("uk-width-1-3 uk-flex-right").CNY(o.pay)._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }


    static string[] ExcludeActions = { nameof(adapted), nameof(adapt) };

    [OrgSpy(BUY_CREATED)]
    [Ui("网售订单", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND typ = 1 AND status = 1 ORDER BY created DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id, exclude: org.IsService ? ExcludeActions : null);

            if (arr == null)
            {
                h.ALERT("尚无网售订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已集合", icon: "chevron-double-right", status: 2), Tool(Anchor)]
    public async Task adapted(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND typ = 1 AND status = 2 ORDER BY adapted DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id, exclude: org.IsService ? ExcludeActions : null);

            if (arr == null)
            {
                h.ALERT("尚无已集合的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [OrgSpy(BUY_OKED)]
    [Ui(tip: "已派发", icon: "arrow-right", status: 4), Tool(Anchor)]
    public async Task oked(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND typ = 1 AND status = 4 ORDER BY oked DESC LIMIT 20 OFFSET 20 * @2");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id, exclude: org.IsService ? ExcludeActions : null);

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
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND typ = 1 AND status = 0 ORDER BY id DESC");
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

    [Ui("集合", icon: "chevron-double-right", status: 1), Tool(ButtonPickOpen)]
    public async Task adapt(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        int[] key;
        bool print;

        if (wc.IsGet)
        {
            key = wc.Query[nameof(key)];

            wc.GivePane(200, h =>
            {
                h.FORM_();
                h.ALERT("备好的货集合到统一派送区");
                foreach (var k in key) h.HIDDEN(nameof(key), k);
                h.CHECKBOX(null, nameof(print), true, tip: "是否打印");
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
            dc.Sql("UPDATE buys SET adapted = @1, adapter = @2, status = 2 WHERE rtlid = @3 AND id ")._IN_(key).T(" AND status = 1");
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
[Ui("网售统一派发")]
public class MktlyBuyWork : BuyWork<MktlyBuyVarWork>
{
    internal void MainGrid(HtmlBuilder h, IList<Buy> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.UL_("uk-card-body uk-list uk-list-divider");
            h.LI_().H4(o.utel).SPAN_("uk-badge").T(o.created, time: 0).SP().T(Buy.Statuses[o.status])._SPAN()._LI();

            foreach (var it in o.items)
            {
                h.LI_();

                h.SPAN_("uk-width-expand").T(it.name);
                if (it.unitw > 0)
                {
                    h.SP().SMALL_().T(it.unitw).T(it.unit)._SMALL();
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

    [Ui("按社区", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        // group by commuity 
        dc.Sql("SELECT ucom, utel, first(uaddr), count(CASE WHEN status = 1 THEN 1 END), count(CASE WHEN status = 2 THEN 2 END) FROM buys WHERE mktid = @1 AND typ = 1 AND (status = 1 OR status = 2) GROUP BY ucom, utel");
        await dc.QueryAsync(p => p.Set(mkt.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("地址").TH("收单", css: "uk-text-right").TH("集合", css: "uk-text-right")._THEAD();

            string last = null;
            while (dc.Next())
            {
                dc.Let(out string ucom);
                dc.Let(out string utel);
                dc.Let(out string uaddr);
                dc.Let(out int created);
                dc.Let(out int adapted);

                if (string.IsNullOrEmpty(ucom)) ucom = "非派送区";

                if (ucom != last)
                {
                    h.TR_().TD_("uk-label", colspan: 2).ADIALOG_(ucom, "/com", mode: ToolAttribute.MOD_OPEN, false, tip: ucom).T(ucom)._A()._TD()._TR();
                }

                h.TR_();
                h.TD2(utel, uaddr);
                h.TD(created);
                h.TD(adapted);
                h._TR();

                last = ucom;
            }
            h._TABLE();
        });
    }

    [Ui("按商户", status: 2), Tool(Anchor)]
    public async Task orgs(WebContext wc)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql($"SELECT rtlid, count(CASE WHEN status = 1 THEN 1 END), count(CASE WHEN status = 2 THEN 2 END) FROM buys WHERE mktid = @1 AND typ = 1 AND (status = 1 OR status = 2) GROUP BY rtlid");
        await dc.QueryAsync(p => p.Set(mkt.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("商户").TH("收单", css: "uk-text-right").TH("集合", css: "uk-text-right")._THEAD();
            while (dc.Next())
            {
                dc.Let(out int rtlid);
                dc.Let(out int count);
                dc.Let(out int adapted);

                var rtl = GrabTwin<int, Org>(rtlid);

                h.TR_();
                h.TD(rtl.name);
                h.TD(count);
                h.TD(adapted);
                h._TR();
            }
            h._TABLE();
        }, false, 6);
    }

    [Ui("我的派发", tip: "我的派发任务", icon: "user", status: 7), Tool(ButtonOpen)]
    public async Task my(WebContext wc)
    {
        var mkt = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE mktid = @1 AND status = 4  AND typ = 1 AND oker = @2 ORDER BY oked DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(mkt.id).Set(prin.name));

        wc.GivePage(200, h =>
        {
            if (arr == null)
            {
                h.ALERT("尚无派发任务");
                return;
            }

            MainGrid(h, arr);
        });
    }
}