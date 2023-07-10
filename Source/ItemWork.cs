using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

public abstract class ItemWork<V> : WebWork where V : ItemVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }
}

public class PublyItemWork : ItemWork<PublyItemVarWork>
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
                h.ARTICLE_("uk-card uk-card-primary");
                h.SECTION_("uk-card-body").T(org.tip)._SECTION();
                if (org.m1)
                {
                    h.SECTION_("uk-card-body").PIC("/org/", org.id, "/m-1")._SECTION();
                }
                h.SECTION_("uk-card-body").T(org.descr)._SECTION();
                if (org.m2)
                {
                    h.SECTION_("uk-card-body").PIC("/org/", org.id, "/m-2")._SECTION();
                }
                if (org.m3)
                {
                    h.SECTION_("uk-card-body").PIC("/org/", org.id, "/m-3")._SECTION();
                }
                h._ARTICLE();

                h.ALERT("暂无商品");
            }
            else
            {
                h.FORM_();

                h.MAINGRID(arr, o =>
                {
                    h.SECTION_("uk-card-body uk-flex");

                    // the cclickable icon
                    //
                    h.ADIALOG_(o.Key, "/", MOD_SHOW, false, css: "uk-width-1-5");
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
                    if (o.step > 1)
                    {
                        h.SP().SMALL_().T(o.step).T(o.unit).T("为整")._SMALL();
                    }
                    // top right corner span
                    h.SPAN_(css: "uk-badge");
                    if (o.promo)
                    {
                        h.SPAN_().T("秒杀 ").T(o.min).SP().T(o.unit)._SPAN().SP();
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
                    h.SPAN_("uk-width-2-5").T("<output class=\"rmb fprice\"></output>&nbsp;<sub>").T(o.unit).T("</sub>")._SPAN();
                    h.SELECT_(o.id, onchange: $"buyRecalc(this);", css: "uk-width-1-4 qtyselect ", empty: "0")._SELECT();
                    h.T("<output class=\"rmb subtotal uk-invisible uk-width-expand uk-text-end\"></output>");
                    h._FOOTER();

                    h._ASIDE();

                    h._SECTION();
                });

                var topay = 0.00M;

                h.BOTTOMBAR_(large: true);

                h.DIV_(css: "uk-col");

                h.DIV_("uk-flex uk-width-1-1");
                h.T("<output class=\"uk-label uk-padding-small\" name=\"name\" cookie=\"name\"></output>");
                h.T("<output class=\"uk-label uk-padding-small\" name=\"tel\" cookie=\"tel\"></output>");
                h.T("<output hidden class=\"uk-h6 uk-margin-auto-left uk-padding-small\" name=\"fee\" title=\"").T(BankUtility.fee).T("\">派送到楼下 +").T(BankUtility.fee).T("</output>");
                h._DIV();

                string com;

                h.DIV_("uk-flex uk-width-1-1");
                h.SELECT_SPEC(nameof(com), mkt.specs, onchange: "this.form.addr.placeholder = (this.value) ? '区栋／单元': '完整收货地址'; buyRecalc();", css: "uk-width-medium");
                h.T("<input type=\"text\" name=\"addr\" class=\"uk-input\" placeholder=\"区栋／单元\" maxlength=\"30\" minlength=\"4\" local=\"addr\" required>");
                h._DIV();

                h._DIV();

                h.BUTTON_(nameof(buy), css: "uk-button-danger uk-width-medium uk-height-1-1", onclick: "return $buy(this);").CNYOUTPUT(nameof(topay), topay)._BUTTON();

                h._BOTTOMBAR();

                h._FORM();
            }
        }, true, arr == null ? 900 : 120, title: org.Title, onload: "fixAll(); buyRecalc();");
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
                fee = string.IsNullOrEmpty(com) ? 0.00M : BankUtility.fee,
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
}

[Ui("商品")]
public class RtllyItemWork : ItemWork<RtllyItemVarWork>
{
    protected static void MainGrid(HtmlBuilder h, Item[] arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            if (o.icon)
            {
                h.PIC(MainApp.WwwUrl, "/item/", o.id, "/icon", css: "uk-width-1-5");
            }
            else
                h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name);
            if (o.step > 1)
            {
                h.SP().SMALL_().T(o.step).T(o.unit).T("为整")._SMALL();
            }

            h.SPAN(Statuses[o.status], "uk-badge");
            h._HEADER();

            h.Q(o.tip, "uk-width-expand");
            h.FOOTER_().SPAN3("剩余", o.stock, o.unit).SPAN_("uk-margin-auto-left").CNY(o.price)._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }

    [Ui("上线商品", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND status = 4 ORDER BY oked DESC");
        var arr = await dc.QueryAsync<Item>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无上线商品");
                return;
            }

            MainGrid(h, arr);
        }, false, 4);
    }

    [Ui(tip: "下线商品", icon: "cloud-download", status: 2), Tool(Anchor)]
    public async Task down(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND status BETWEEN 1 AND 2 ORDER BY adapted DESC");
        var arr = await dc.QueryAsync<Item>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无下线商品");
                return;
            }

            MainGrid(h, arr);
        }, false, 4);
    }

    [Ui(tip: "已作废商品", icon: "trash", status: 4), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE orgid = @1 AND status = 0 ORDER BY adapted DESC");
        var arr = await dc.QueryAsync<Item>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无作废商品");
                return;
            }

            MainGrid(h, arr);
        }, false, 4);
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("自建", status: 2), Tool(ButtonOpen)]
    public async Task def(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        bool product = !org.IsServiceSector;

        const short MAX = 100;
        var o = new Item
        {
            typ = product ? Item.TYP_PRODUCT : Item.TYP_SERVICE,
            orgid = org.id,
            created = DateTime.Now,
            creator = prin.name,
            unit = product ? "斤" : "位",
            unitw = product ? (short)500 : (short)0,
            step = 1,
            min = 1,
            max = MAX
        };
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("自建" + (product ? "非溯源产品型商品" : "服务型商品"));

                h.LI_().TEXT("商品名", nameof(o.name), o.name, max: 12)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("零售单位", nameof(o.unit), o.unit, Unit.Typs, showkey: true, onchange: "this.form.unitw.value = this.selectedOptions[0].title").SELECT("单位含重", nameof(o.unitw), o.unitw, Unit.Metrics)._LI();
                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("为整", nameof(o.step), o.step, min: 1, money: false, onchange: $"this.form.min.value = this.value; this.form.max.value = this.value * {MAX}; ")._LI();
                h.LI_().NUMBER("大客户优惠", nameof(o.off), o.off, min: 0.00M, max: 999.99M).CHECKBOX("全民优惠", nameof(o.promo), o.promo)._LI();
                h.LI_().NUMBER("起订量", nameof(o.min), o.min, min: 1, max: o.stock).NUMBER("限订量", nameof(o.max), o.max, min: MAX)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(def))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, o);

            // insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO items ").colset(Item.Empty, msk)._VALUES_(Item.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("导入", status: 2), Tool(ButtonOpen)]
    public async Task imp(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        const short MAX = 100;
        var o = new Item
        {
            typ = Item.TYP_PRODUCT,
            created = DateTime.Now,
            creator = prin.name,
            step = 1,
            min = 1,
            max = MAX
        };

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT DISTINCT lotid, concat(supname, ' ', name), id FROM purs WHERE rtlid = @1 AND status = 4 ORDER BY id DESC LIMIT 50");
            await dc.QueryAsync(p => p.Set(org.id));
            var lots = dc.ToIntMap();

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("导入供应链产品");

                h.LI_().SELECT("供应产品名", nameof(o.lotid), o.lotid, lots, required: true)._LI();
                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("为整", nameof(o.step), o.step, min: 1, money: false, onchange: $"this.form.min.value = this.value; this.form.max.value = this.value * {MAX}; ")._LI();
                h.LI_().NUMBER("ＶＩＰ减价", nameof(o.off), o.off, min: 0.00M, max: 999.99M).CHECKBOX("全民秒杀期", nameof(o.promo), o.promo)._LI();
                h.LI_().NUMBER("起订量", nameof(o.min), o.min, min: 1, max: o.stock).NUMBER("限订量", nameof(o.max), o.max, min: MAX)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(imp))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            await wc.ReadObjectAsync(msk, o);

            using var dc = NewDbContext(IsolationLevel.ReadCommitted);

            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1");
            var lot = await dc.QueryTopAsync<Lot>(p => p.Set(o.lotid));

            // init by lot
            {
                o.typ = lot.typ;
                o.name = lot.name;
                o.tip = lot.tip;
                o.unit = lot.unit;
                o.unitw = lot.unitw;
            }

            // insert
            dc.Sql("INSERT INTO items ").colset(Item.Empty, msk)._VALUES_(Item.Empty, msk).T(" RETURNING id");
            var itemid = (int)await dc.ScalarAsync(p => o.Write(p, msk));

            dc.Sql("UPDATE items SET (icon, pic) = (SELECT icon, pic FROM lots WHERE id = @1) WHERE id = @2");
            await dc.ExecuteAsync(p => p.Set(o.lotid).Set(itemid));

            wc.GivePane(200); // close dialog
        }
    }
}