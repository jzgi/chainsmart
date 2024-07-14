using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.ToolAttribute;
using static ChainFX.Entity;

namespace ChainSmart;

[UserAuthenticate]
public class PublyVarWork : ItemWork<PublyItemVarWork>
{
    /// <summary>
    /// The home for a shop / merchant.
    /// </summary>
    public async Task @default(WebContext wc)
    {
        int orgid = wc[0];
        var org = GrabTwin<int, Org>(orgid);

        var mkt = org.IsRtlEst ? org : GrabTwin<int, Org>(org.parentid);

        // show availlable item list
        //
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND status = 4 ORDER BY promo, oked DESC");
        var arr = await dc.QueryAsync<Item>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            if (org.pic)
            {
                h.PIC_("/org/", org.id, "/pic");
            }
            else
            {
                h.PIC_("/void-m.webp");
            }
            if (!string.IsNullOrEmpty(org.tip))
            {
                h.ALERT(org.tip, css: "uk-position-bottom uk-overlay uk-alert-primary");
            }
            h._PIC();

            // sticky info belt
            h.T("<section uk-sticky class=\"uk-card-footer\">");
            if (!org.IsOked)
            {
                h.SPAN_().ICON("bell").SP().T("商户已下线")._SPAN();
                return;
            }
            var open = org.Openable(DateTime.Now.TimeOfDay);
            if (org.AsRtl)
            {
                h.SPAN_().ICON("bell").SP().T(open ? "营业中" : "休息中")._SPAN();
            }
            h.SPAN_("uk-margin-left").ICON("clock").SP().T(org.openat).T(" - ").T(org.closeat)._SPAN();
            h.SPAN_("uk-margin-auto-left").T("电话").SP().ATEL(org.tel, css: "uk-light")._SPAN();
            h.T("</section>");

            if (arr == null)
            {
                h.ALERT("暂无上线商品");
                return;
            }

            h.FORM_();

            h.MAIN_(grid: true);

            foreach (var o in arr)
            {
                h.ARTICLE_("uk-card uk-card-default");
                h.SECTION_("uk-card-body uk-flex");

                // the cclickable icon
                h.ADIALOG_(o.Key, "/", MOD_SHOW, false, tip: o.name, css: "uk-width-1-4");
                if (o.icon)
                {
                    h.IMG("/item/", o.id, "/icon");
                }
                else
                {
                    h.IMG("/void.webp");
                }
                h._A();

                h.ASIDE_();

                h.HEADER_().H4(o.name);
                if (o.unitip != null)
                {
                    h.SP().SMALL_().T(o.unitip)._SMALL();
                }
                // top right corner span
                h.SPAN_(css: "uk-badge");
                if (o.tag > 0)
                {
                    var tags = Grab<short, Tag>();
                    h.MARK(tags[o.tag].name);
                }
                if (o.sym > 0)
                {
                    var syms = Grab<short, Sym>();
                    h.MARK(syms[o.sym].name);
                }
                h._SPAN();
                h._HEADER();

                h.Q(o.tip, "uk-width-expand");

                // price and qty select & detail
                //
                h.T("<footer cookie= \"vip\" onfix=\"fillPriceAndQtySelect(this,event,'").T(o.unit).T("',").T(o.price).T(',').T(o.off).T(',').T(o.promo).T(',').T(o.unitx).T(',').T(o.min).T(',').T(o.max).T(',').T(o.stock).T(")\">"); // pricing portion
                h.SPAN_("uk-width-2-5").T("<output class=\"rmb fprice\">")._SPAN();
                h.SELECT_(o.id, onchange: $"buyRecalc(this);", css: "uk-width-1-4 qtyselect ");
                if (o.stock > 0)
                {
                    h.OPTION_(string.Empty).T("0 ").T(o.unit);
                }
                h._SELECT();
                h.T("<output class=\"rmb subtotal uk-invisible uk-width-expand uk-text-end\"></output>");
                h._FOOTER();

                h._ASIDE();

                h._SECTION();
                h._ARTICLE();
            }

            h._MAIN();

            //
            // payment area
            //
            if (!org.Payable) return;

            var topay = 0.00M;

            h.BOTTOMBAR_(large: true);

            h.SECTION_("uk-col uk-flex-evenly uk-width-expand");

            // upper line
            h.DIV_("uk-flex uk-width-1-1");
            string area;
            h.SELECT_SPEC(nameof(area), mkt.specs, onchange: "this.form.addr.placeholder = (this.value == '') ? '省市／详细地址': '小区／楼栋门牌'; buyRecalc();", css: "uk-width-1-3 uk-border-rounded");
            h.T("<input type=\"text\" name=\"addr\" class=\"uk-input uk-border-rounded\" placeholder=\"收货地址\" maxlength=\"30\" minlength=\"4\" local=\"addr\" required>");
            h._DIV();

            // lower line
            h.DIV_("uk-flex uk-width-1-1 uk-input uk-padding");
            h.T("<output class=\"uk-label\" name=\"name\" cookie=\"name\"></output>").SP();
            h.T("<output class=\"uk-label\" name=\"tel\" cookie=\"tel\"></output>");
            if (org.IsModeUni)
            {
                var (min, rate, max) = FinanceUtility.mktdlvfee;
                h.SPAN_("uk-width-expand uk-flex-right uk-text-danger").T("运费").SP().T("<output name=\"fee\" min=\"").T(min).T("\" rate=\"").T(rate).T("\" max=").T(max).T("\">0.00</output>")._SPAN();
            }
            h._DIV();

            h._SECTION();

            h.BUTTON_(nameof(buy), css: "uk-button-danger uk-width-1-5 uk-height-1-1", onclick: "return $buy(this);").CNYOUTPUT(nameof(topay), topay)._BUTTON();

            h._BOTTOMBAR();

            h._FORM();
        }, true, arr == null ? 720 : 120, title: org.Name, onload: "fixAll(); buyRecalc();");
    }

    public async Task buy(WebContext wc)
    {
        int orgid = wc[0];
        var org = GrabTwin<int, Org>(orgid);
        var prin = (User)wc.Principal;

        var f = await wc.ReadAsync<Form>();
        string area = f[nameof(area)];
        string addr = f[nameof(addr)];

        // buy lines
        var lst = new List<BuyLn>();
        for (int i = 0; i < f.Count; i++)
        {
            var ety = f.EntryAt(i);
            int id = ety.Key.ToInt();
            short qty = ety.Value;

            if (id <= 0 || qty <= 0) // filter out the non-selected (but submitted)
            {
                continue;
            }

            lst.Add(new BuyLn(id, qty));
        }

        using var dc = NewDbContext(IsolationLevel.ReadCommitted);
        try
        {
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND id ")._IN_(lst);
            var map = await dc.QueryAsync<int, Item>(p => p.Set(orgid).SetForIn(lst));

            foreach (var ln in lst)
            {
                var item = map[ln.itemid];
                if (item != null)
                {
                    ln.Init(item, vip: prin.IsVipFor(orgid) || item.promo);
                }
            }

            var mkt = GrabTwin<int, Org>(org.MktId);
            
            var (topay, fee) = FinanceUtility.GetTopayAndFee(lst, org, mkt, area);
            var m = new Buy(prin, org, lst.ToArray())
            {
                created = DateTime.Now,
                creator = prin.name,
                uarea = area,
                uaddr = addr,
                fee = fee,
                topay = topay,
                status = STU_CREATED,
            };

            // check and try to use an existing record
            int buyid = 0;
            if (await dc.QueryTopAsync("SELECT id FROM buys WHERE uid = @1 AND status = 1 AND typ = 1 LIMIT 1", p => p.Set(prin.id)))
            {
                dc.Let(out buyid);
            }

            const short msk = MSK_BORN | MSK_EDIT | MSK_STATUS;
            if (buyid == 0)
            {
                dc.Sql("INSERT INTO buys ").colset(Buy.Empty, msk)._VALUES_(Buy.Empty, msk).T(" RETURNING id, topay");
                await dc.QueryTopAsync(p => m.Write(p, msk));
            }
            else
            {
                dc.Sql("UPDATE buys ")._SET_(Buy.Empty, msk).T(" WHERE id = @1 RETURNING id, topay");
                await dc.QueryTopAsync(p =>
                {
                    m.Write(p, msk);
                    p.Set(buyid);
                });
            }
            dc.Let(out buyid);
            dc.Let(out topay);

            // // call WeChatPay to prepare order there
            string trade_no = Buy.GetOutTradeNo(buyid, topay);
            var (prepay_id, err_code) = await CloudUtility.PostUnifiedOrderAsync(sup: false,
                trade_no,
                topay,
                prin.im, // the payer
                wc.RemoteIpAddress.ToString(),
                MainApp.WwwUrl + "/" + nameof(WwwService.onpay),
                m.ToString()
            );

            if (prepay_id != null)
            {
                wc.Give(200, CloudUtility.BuildPrepayContent(prepay_id));
            }
            else
            {
                dc.Rollback();
                Application.Err("err_code " + err_code);
                wc.Give(500);
            }
        }
        catch (Exception e)
        {
            dc.Rollback();
            Application.Err(e.Message);
            wc.Give(500);
        }
    }


    /// <summary>
    /// The home of the market by sector
    /// </summary>
    public void h(WebContext wc, int sector)
    {
        int orgid = wc[0];
        var regs = Grab<short, Reg>();
        var org = GrabTwin<int, Org>(orgid);

        if (!org.IsRtlEst)
        {
            wc.GiveMsg(304, "not a market");
            return;
        }

        var arr = GrabTwinArray<int, Org>(orgid, x => x.regid == sector && x.status == 4);
        if (sector == 0 && org.IsOked) // default sector
        {
            arr = arr.AddOf(org, first: true);
        }

        wc.GivePage(200, h =>
        {
            if (org.IsRtlEst)
            {
                h.NAVBAR(nameof(this.h), sector, regs, filter: (_, x) => x.IsSector);
            }

            if (arr == null)
            {
                h.ALERT("尚无上线商户");
                return;
            }

            // market info

            var now = DateTime.Now.TimeOfDay;

            h.MAIN_(grid: true);
            foreach (var m in arr)
            {
                h.ARTICLE_("uk-card uk-card-default");
                lock (m)
                {
                    var open = m.Openable(now);
                    if (m.IsLink)
                    {
                        h.A_(m.addr, css: "uk-card-body uk-flex");
                    }
                    else
                    {
                        h.ADIALOG_("/", m.Key, "/", MOD_OPEN, false, tip: m.Name, inactive: !open, "uk-card-body uk-flex");
                    }

                    if (m.icon)
                    {
                        h.PIC("/org/", m.id, "/icon", css: "uk-width-1-4");
                    }
                    else
                    {
                        h.PIC("/void.webp", css: "uk-width-1-4");
                    }

                    h.ASIDE_();
                    h.HEADER_().H4(m.Name).SPAN_(css: "uk-badge");
                    if (m.IsOrdinary && !string.IsNullOrEmpty(m.addr))
                    {
                        h.MARK(m.addr);
                    }
                    h._SPAN()._HEADER();

                    h.Q(m.tip, "uk-width-expand");

                    h.FOOTER_().SPAN_("uk-margin-auto-left");
                    if (m.rank > 0)
                    {
                        h.SPAN(Org.Ranks[m.rank], "uk-label");
                    }
                    h._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                }
                h._ARTICLE();
            }
            h._MAIN();
        }, true, 720, org.Whole);
    }
}