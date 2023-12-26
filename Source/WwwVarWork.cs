using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Nodality;
using static ChainFX.Web.ToolAttribute;
using static ChainFX.Entity;

namespace ChainSmart;

[UserAuthenticate(OmitDefault = true)]
public class WwwVarWork : ItemWork<WwwItemVarWork>
{
    /// <summary>
    /// The home for a retail shop.
    /// </summary>
    public async Task @default(WebContext wc)
    {
        int orgid = wc[0];
        var org = GrabTwin<int, Org>(orgid);

        var mkt = org.IsMarket ? org : GrabTwin<int, Org>(org.upperid);

        //
        // item list
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
                h.PIC_("/void-m.webp");

            h.AICON("../", "home", css: "uk-overlay uk-position-small uk-position-top-left");
            h.ATEL(org.tel, css: "uk-overlay uk-position-small uk-position-top-right");

            if (!org.IsOked)
            {
                h.ALERT("商户已下线", icon: "bell", css: "uk-position-bottom uk-overlay uk-alert-primary");
                return;
            }
            if (org.AsRetail && !org.IsOpen(DateTime.Now.TimeOfDay))
            {
                h.ALERT("商户已打烊，订单待后处理", icon: "bell", css: "uk-position-bottom uk-overlay uk-alert-primary");
            }
            h._PIC();

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
                //
                h.ADIALOG_(o.Key, "/", MOD_SHOW, false, tip: o.name, css: "uk-width-1-5");
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
                if (o.unitw > 0)
                {
                    h.SP().SMALL_().T(Unit.Weights[o.unitw])._SMALL();
                }
                // top right corner span
                h.SPAN_(css: "uk-badge");
                if (o.promo)
                {
                    h.SPAN_().T("原价 ").T(o.price)._SPAN().SP();
                }
                if (o.rank > 0)
                {
                    h.MARK(Item.Ranks[o.rank]);
                }
                h._SPAN();
                h._HEADER();

                h.Q(o.tip, "uk-width-expand");

                // FOOTER: price and qty select & detail
                h.T($"<footer cookie= \"vip\" onfix=\"fillPriceAndQtySelect(this,event,'{o.unit}',{o.price},{o.off},{o.step},{o.max},{o.stock},{o.min});\">"); // pricing portion
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
            if (!org.Orderable) return;

            var topay = 0.00M;

            h.BOTTOMBAR_(large: true);

            h.SECTION_(css: "uk-col");

            h.SPAN_("uk-flex uk-width-1-1");
            h.T("<output class=\"uk-label uk-padding-small\" name=\"name\" cookie=\"name\"></output>");
            h.T("<output class=\"uk-label uk-padding-small\" name=\"tel\" cookie=\"tel\"></output>");
            h.T("<output hidden class=\"uk-h6 uk-margin-auto-left uk-padding-small\" name=\"fee\" title=\"").T(BankUtility.rtlfee).T("\">派送到楼下 +").T(BankUtility.rtlfee).T("</output>");
            h._SPAN();

            string com;

            h.SPAN_("uk-flex uk-width-1-1");
            h.SELECT_SPEC(nameof(com), mkt.specs, onchange: "this.form.addr.placeholder = (this.value) ? '区栋／单元': '备注'; buyRecalc();", css: "uk-width-medium");
            h.T("<input type=\"text\" name=\"addr\" class=\"uk-input\" placeholder=\"备注\" maxlength=\"30\" minlength=\"4\" local=\"addr\" required>");
            h._SPAN();

            h._SECTION();

            h.BUTTON_(nameof(buy), css: "uk-button-danger uk-width-medium uk-height-1-1", onclick: "return $buy(this);").CNYOUTPUT(nameof(topay), topay)._BUTTON();

            h._BOTTOMBAR();

            h._FORM();
        }, true, arr == null ? 720 : 120, title: org.Title, onload: "fixAll(); buyRecalc();");
    }

    public async Task buy(WebContext wc)
    {
        int orgid = wc[0];
        var org = GrabTwin<int, Org>(orgid);
        var prin = (User)wc.Principal;

        var f = await wc.ReadAsync<Form>();
        string com = f[nameof(com)];
        string addr = f[nameof(addr)];

        // lines of detail
        var lst = new List<BuyItem>();
        for (int i = 0; i < f.Count; i++)
        {
            var ety = f.EntryAt(i);
            int id = ety.Key.ToInt();
            short qty = ety.Value;

            if (id <= 0 || qty <= 0) // filter out the non-selected (but submitted)
            {
                continue;
            }

            lst.Add(new BuyItem(id, qty));
        }

        using var dc = NewDbContext(IsolationLevel.ReadCommitted);
        try
        {
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND id ")._IN_(lst);
            var map = await dc.QueryAsync<int, Item>(p => p.Set(orgid).SetForIn(lst));

            foreach (var bi in lst)
            {
                var item = map[bi.itemid];
                if (item != null)
                {
                    bi.Init(item, vip: prin.IsVipFor(orgid) || item.promo);
                }
            }

            var m = new Buy(prin, org, lst.ToArray())
            {
                created = DateTime.Now,
                creator = prin.name,
                ucom = com,
                uaddr = addr,
                fee = string.IsNullOrEmpty(com) ? 0.00M : BankUtility.rtlfee,
                status = -1, // before confirmation of payment
            };
            m.InitTopay();

            // check and try to use an existing record
            int buyid = 0;
            if (await dc.QueryTopAsync("SELECT id FROM buys WHERE uid = @1 AND status = -1 AND typ = 1 LIMIT 1", p => p.Set(prin.id)))
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
            dc.Let(out decimal topay);

            // // call WeChatPay to prepare order there
            string trade_no = Buy.GetOutTradeNo(buyid, topay);
            var (prepay_id, _) = await WeixinUtility.PostUnifiedOrderAsync(sup: false,
                trade_no,
                topay,
                prin.im, // the payer
                wc.RemoteIpAddress.ToString(),
                MainApp.WwwUrl + "/" + nameof(WwwService.onpay),
                m.ToString()
            );
            if (prepay_id != null)
            {
                wc.Give(200, WeixinUtility.BuildPrepayContent(prepay_id));
            }
            else
            {
                dc.Rollback();
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
    /// The list of shops by sector
    /// </summary>
    public void lst(WebContext wc, int sector)
    {
        if (sector == 0)
        {
            sector = wc.Subscript = Reg.HOME_REGID;
        }

        int orgid = wc[0];
        var regs = Grab<short, Reg>();
        var org = GrabTwin<int, Org>(orgid);

        if (!org.IsMarket)
        {
            wc.GiveMsg(304, "not a market");
            return;
        }

        var arr = GrabTwinArray<int, Org>(orgid, x => x.regid == sector && x.status == 4);
        if (sector == Reg.HOME_REGID && org.IsOked) // default sector
        {
            arr = arr.AddOf(org, first: true);
        }

        wc.GivePage(200, h =>
        {
            h.NAVBAR(nameof(lst), sector, regs, (_, v) => v.IsSector);

            if (arr == null)
            {
                h.ALERT("尚无上线商户");
                return;
            }

            // market info

            var now = DateTime.Now.TimeOfDay;

            if (sector == Reg.HOME_REGID)
            {
                h.SLIDERUL_();

                h.LI_("uk-tile");
                if (org.scene)
                {
                    h.PIC_("/org/", org.id, "/scene");
                }
                else
                {
                    h.PIC_("/void.webp");
                }
                h._PIC();
                h._LI();

                h.LI_("uk-tile");
                h.H3("统一派送区域");
                var specs = org.specs;
                for (int i = 0; i < specs?.Count; i++)
                {
                    var spec = specs.EntryAt(i);
                    var v = spec.Value;
                    if (v.IsObject)
                    {
                        h.DL_();
                        h.DT(spec.Key);

                        h.DD_();
                        var sub = (JObj)v;
                        for (int k = 0; k < sub.Count; k++)
                        {
                            if (k > 0) h.T('，');
                            h.T(sub.KeyAt(k));
                        }
                        h._DD();
                        h._DL();
                    }
                }
                h._LI();

                h._SLIDERUL();
            }

            h.MAIN_(grid: true);
            foreach (var m in arr)
            {
                h.ARTICLE_("uk-card uk-card-default");
                lock (m)
                {
                    var open = m.IsOpen(now);
                    if (m.IsLink)
                    {
                        h.A_(m.addr, css: "uk-card-body uk-flex");
                    }
                    else
                    {
                        h.ADIALOG_("/", m.Key, "/", MOD_OPEN, false, tip: m.Title, inactive: !open, "uk-card-body uk-flex");
                    }

                    if (m.icon)
                    {
                        h.PIC("/org/", m.id, "/icon", css: "uk-width-1-5");
                    }
                    else
                        h.PIC("/void.webp", css: "uk-width-1-5");

                    h.ASIDE_();
                    h.HEADER_().H4(m.Name).SPAN(m.IsLink ? string.Empty : open ? "营业" : "休息", css: "uk-badge uk-badge-success")._HEADER();
                    h.Q(m.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                }
                h._ARTICLE();
            }
            h._MAIN();
        }, true, 720, org.Cover);
    }
}