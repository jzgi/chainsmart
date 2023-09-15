using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

public abstract class PosWork<V> : WebWork where V : BuyVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainTable(HtmlBuilder h, IList<Buy> lst)
    {
        h.TABLE(lst, o =>
        {
            h.TD_("uk-width-tiny").T(o.created, 0, 2)._TD();
            h.TD_().ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name);
            for (var i = 0; i < o.items.Length; i++)
            {
                if (i > 0)
                {
                    h.SP();
                }
                var v = o.items[i];
                h.IMG(MainApp.WwwUrl + "/item/", v.itemid, "/icon", css: "uk-width-micro");
            }
            h._A()._TD();
            h.TD_(css: "uk-width-tiny uk-text-right").SP().CNY(o.pay)._TD();
            h.TD_(css: "uk-width-tiny").T(Buy.Typs[o.typ])._TD();
        });
    }
}

[Ui("POS")]
public class RtllyPosWork : PosWork<RtllyPosVarWork>
{
    [Ui("POS", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND status = 4 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Item>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            int itemid = 0;
            decimal qtyx = 0;

            //
            // form input

            h.FORM_("uk-card uk-card-primary").FIELDSUL_();
            h.LI_().SELECT_(nameof(itemid), onchange: "posItemChange(this);", empty: string.Empty, required: false);

            for (var i = 0; i < arr?.Length; i++)
            {
                var o = arr[i];

                h.T("<option value=\"").T(o.id).T("\" lotid=\"").T(o.lotid).T("\" name=\"").T(o.name).T("\" unit=\"").T(o.unit).T("\" unitw=\"").T(o.unitw).T("\" price=\"").T(o.price).T("\" stock=\"").T(o.stock).T("\">");
                h.T(o.name);
                if (o.step > 1)
                {
                    h.T(o.step).T(o.unit);
                }
                h.T('（').T(o.stock).SP().T(o.unit).T('）');

                h.T("</option>");
            }

            h._SELECT();
            h._LI();

            decimal subtotal = 0;
            decimal pay = 0;

            h.LI_();
            h.SPAN_("uk-width-expand").T("<input type=\"number\" name=\"price\" class=\"uk-input\" placeholder=\"改单价\" local=\"\" onchange=\"posRecalc(this);\" step=\"any\" required><output class=\"suffix\">元</output>")._SPAN();
            h.SPAN_("uk-width-expand").T("<input type=\"number\" name=\"qty\" class=\"uk-input\" placeholder=\"填数量\" oninput=\"posRecalc(this);\" step=\"any\" required><output name=\"unit\" class=\"suffix\"></output>")._SPAN();
            h.T("<button type=\"button\" class=\"uk-button-danger uk-width-medium\" onclick=\"return posAdd(this);\">").CNYOUTPUT(nameof(subtotal), subtotal)._BUTTON();
            h._LI();

            h._FIELDSUL()._FORM();

            // 
            // form lines
            //
            h.FORM_("uk-card uk-card-primary");

            h.TABLE_();
            h.T("<thead>").TH("商品", css: "uk-width-1-2 uk-text-left").TH("单价", css: "uk-text-right").TH("数量", css: "uk-text-right").TH("小计", css: "uk-text-right").T("<th class=\"uk-width-micro\"></th></thead>");
            h.T("<tbody id=\"items\">"); // line items
            h.T("</tbody>");
            h._TABLE();

            h.NAV_(css: "uk-flex-center");
            h.T("<a class=\"uk-icon-button\" uk-icon=\"arrow-left\" onclick=\"posResum(ancestorOf(this, 'form'), 1);\"></a>");
            h.NUMBER(null, nameof(pay), pay, @readonly: true, css: "uk-width-medium uk-text-right");
            h.T("<a class=\"uk-icon-button\" uk-icon=\"arrow-right\" onclick=\"posResum(ancestorOf(this, 'form'), 2);\"></a>");
            h._NAV();

            h.BOTTOMBAR_();
            for (short i = 1; i < Buy.Typs.Count; i++)
            {
                var e = Buy.Typs.EntryAt(i);
                h.BUTTON_(nameof(buy), subscript: e.Key, onclick: "return $pos(this);", css: "uk-button-default").T(e.Value)._BUTTON();
            }
            h._BOTTOMBAR();

            h._FORM();
        }, false, 60, onload: "fixAll();");
    }

    [Ui(tip: "今日业务", icon: "table", status: 2), Tool(Anchor)]
    public async Task today(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND typ >= 2 AND status = 4 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无业务记录");
                return;
            }
            MainTable(h, arr);
        }, false, 6);
    }

    static readonly string[] DAYS = { "日", "一", "二", "三", "四", "五", "六" };

    [Ui(tip: "以往记录", icon: "history", status: 2), Tool(AnchorPrompt)]
    public async Task past(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var today = DateTime.Today;
        int day = 0;
        bool inner = wc.Query[nameof(inner)];
        if (inner)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();
                for (day = 1; day <= 7; day++)
                {
                    var dt = today.AddDays(-day);
                    h.LI_().RADIO(nameof(day), day, dt.ToString("yyyy-MM-dd")).SP().T("周").T(DAYS[(int)dt.DayOfWeek])._LI();
                }
                h._FIELDSUL()._FORM();
            });
        }
        else // OUTER
        {
            day = wc.Query[nameof(day)];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND typ > 1 AND created BETWEEN @2 AND @3");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id).Set(today.AddDays(-day - 1)).Set(today.AddDays(-day)));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("没有业务记录");
                    return;
                }
                MainTable(h, arr);
            }, false, 60);
        }
    }

    [Ui(tip: "已撤销", icon: "trash", status: 4), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND typ > 1 AND status = 0 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无已撤销的业务");
                return;
            }
            MainTable(h, arr);
        }, false, 6);
    }


    public async Task buy(WebContext wc, int payTyp)
    {
        var rtl = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        var frm = await wc.ReadAsync<Form>();
        decimal pay = frm[nameof(pay)];

        // detail lines
        var lst = new List<BuyItem>();
        for (var i = 0; i < frm.Count; i++)
        {
            var ety = frm.EntryAt(i);
            int itemid = ety.Key.ToInt();
            if (itemid == 0)
            {
                continue;
            }

            var comp = ((string)ety.Value).Split('-');

            lst.Add(new BuyItem(itemid, comp));
        }

        if (lst.Count == 0) return;

        var now = DateTime.Now;
        var m = new Buy()
        {
            typ = (short)payTyp,
            rtlid = rtl.id,
            name = rtl.name,
            mktid = rtl.MarketId,
            created = now,
            creator = prin.name,
            items = lst.ToArray(),
            status = STU_OKED,
            oked = now,
            oker = prin.name,
            pay = pay,
        };
        m.InitTopay();

        const short msk = MSK_BORN | MSK_EDIT | MSK_STATUS | MSK_LATER;

        try
        {
            using var dc = NewDbContext();

            dc.Sql("INSERT INTO buys ").colset(Buy.Empty, msk)._VALUES_(Buy.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));
        }
        catch (Exception e)
        {
            wc.Give(500); // data error, maybe a check violdate
            return;
        }

        wc.Give(201); // created
    }
}