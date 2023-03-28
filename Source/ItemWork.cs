using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart
{
    public abstract class ItemWork<V> : WebWork where V : ItemVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    public class PublyItemWork : ItemWork<PublyItemVarWork>
    {
        public async Task @default(WebContext wc)
        {
            int orgid = wc[0];
            var org = GrabObject<int, Org>(orgid);

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE shpid = @1 AND status = 4 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Item>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                if (org.pic)
                {
                    h.PIC_("/org/", org.id, "/pic");
                }
                else
                    h.PIC_("/void-shop.webp");

                h.ATEL(org.tel, css: "uk-overlay uk-position-center-right");
                h._PIC();

                if (arr == null)
                {
                    h.ALERT("暂无商品");
                    return;
                }

                decimal fprice = 0;

                h.FORM_(oninput: $"pay.value = {fprice} * parseInt(unitx.value) * parseInt(qty.value);");

                h.MAINGRID(arr, o =>
                {
                    h.SECTION_("uk-card-body uk-flex");

                    // the cclickable icon
                    //
                    if (o.icon)
                    {
                        h.PIC("/item/", o.id, "/icon", css: "uk-width-1-5");
                    }
                    else
                        h.PIC("/void.webp", css: "uk-width-1-5");

                    h.ASIDE_();

                    h.HEADER_().H4(o.name);
                    if (o.unitx != 1)
                    {
                        h.SMALL_().T('（').T(o.unitx).T(o.unit).T("）")._SMALL();
                    }

                    // top right corner span
                    h.SPAN_(css: "uk-badge");
                    // ran mark
                    h.ADIALOG_(o.Key, "/", MOD_SHOW, false, css: "uk-display-contents");
                    h.ICON("question", css: "uk-icon-link");
                    h._A();
                    h._SPAN();
                    h._HEADER();

                    h.Q(o.tip, "uk-width-expand");

                    // FOOTER: price and qty select & detail
                    h.T($"<footer cookie= \"vip\" onfix=\"fillPriceAndQtySelect(this,event,{o.price},{o.off},{o.min},{o.stock},{o.AvailX});\">"); // pricing portion
                    h.SPAN_("uk-width-1-3").T("<output class=\"rmb fprice\"></output>&nbsp;<sub>").T(o.unit).T("</sub>")._SPAN();
                    h.SELECT_(o.id, onchange: $"sumQtyDetails(this,{o.unitx});", css: "uk-width-1-5 qtyselect ", required: false)._SELECT();
                    h.SPAN_("qtydetail uk-invisible").T("&nbsp;<output class=\"qtyx\"></output>&nbsp;").T(o.unit).T("<output class=\"rmb subtotal uk-width-expand uk-text-end\"></output>")._SPAN();
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
                h._DIV();

                string com;

                h.DIV_("uk-flex uk-width-1-1");
                h.SELECT_SPEC(nameof(com), org.specs, css: "uk-width-medium");
                h.T("<input type=\"text\" name=\"addr\" class=\"uk-input\" placeholder=\"楼栋／单元\" maxlength=\"30\" minlength=\"4\" local=\"addr\" required>");
                h._DIV();

                h._DIV();

                h.BUTTON_(nameof(buy), css: "uk-button-danger uk-width-medium uk-height-1-1", onclick: "return call_buy(this);").CNYOUTPUT(nameof(topay), topay)._BUTTON();

                h._BOTTOMBAR();

                h._FORM();
            }, true, 300, title: org.name, onload: "fixAll();");
        }

        public async Task buy(WebContext wc, int cmd)
        {
            int shpid = wc[-1];
            var shp = GrabObject<int, Org>(shpid);
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
                dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE shpid = @1 AND id ")._IN_(lst);
                var map = await dc.QueryAsync<int, Item>(p => p.Set(shpid).SetForIn(lst));

                foreach (var ln in lst)
                {
                    var item = map[ln.itemid];
                    if (item != null)
                    {
                        ln.Init(item, vip: prin.vip?.Contains(shpid) ?? false);
                    }
                }

                var m = new Buy(prin, shp, lst.ToArray())
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    ucom = com,
                    uaddr = addr,
                    status = -1, // before confirmed of payment
                };
                m.SetToPay();

                // NOTE single unsubmitted record
                const short msk = MSK_BORN | MSK_EDIT | MSK_STATUS;
                dc.Sql("INSERT INTO buys ").colset(Buy.Empty, msk)._VALUES_(Buy.Empty, msk).T(" ON CONFLICT (shpid, typ, status) WHERE typ = 1 AND status = -1 DO UPDATE ")._SET_(Buy.Empty, msk).T(" RETURNING id, topay");
                await dc.QueryTopAsync(p => m.Write(p, msk));
                dc.Let(out int buyid);
                dc.Let(out decimal topay);

                // // call WeChatPay to prepare order there
                string trade_no = Buy.GetOutTradeNo(buyid, topay);
                var (prepay_id, err_code) = await WeixinUtility.PostUnifiedOrderAsync(sup: false,
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

    [Ui("商品", "商户")]
    public class ShplyItemWork : ItemWork<ShplyItemVarWork>
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
                if (o.unitx != 1)
                {
                    h.SP().SMALL_().T(o.unitx).T(o.unit).T("件")._SMALL();
                }

                h.SPAN(Item.Statuses[o.status], "uk-badge");
                h._HEADER();

                h.Q(o.tip, "uk-width-expand");
                h.FOOTER_().SPAN2("剩余", o.avail).SPAN_("uk-margin-auto-left").CNY(o.price)._SPAN()._FOOTER();
                h._ASIDE();

                h._A();
            });
        }

        [Ui("上线商品", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE shpid = @1 AND status = 4 ORDER BY oked DESC");
            var arr = await dc.QueryAsync<Item>(p => p.Set(src.id));

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

        [Ui(tip: "下线商品", icon: "cloud-download", group: 2), Tool(Anchor)]
        public async Task off(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE shpid = @1 AND status BETWEEN 1 AND 2 ORDER BY adapted DESC");
            var arr = await dc.QueryAsync<Item>(p => p.Set(src.id));

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

        [Ui(tip: "已作废", icon: "trash", group: 8), Tool(Anchor)]
        public async Task aborted(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE shpid = @1 AND status = 0 ORDER BY adapted DESC");
            var arr = await dc.QueryAsync<Item>(p => p.Set(src.id));

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
        [Ui("自创", "自创非供应商品", icon: "plus", group: 2), Tool(ButtonOpen)]
        public async Task def(WebContext wc, int state)
        {
            var org = wc[-1].As<Org>();

            var prin = (User)wc.Principal;
            var cats = Grab<short, Cat>();

            if (wc.IsGet)
            {
                var o = new Item
                {
                    created = DateTime.Now,
                    creator = prin.name
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_();

                    h.LI_().TEXT("商品名", nameof(o.name), o.name, max: 12).SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().TEXT("基准单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("批发件含量", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();
                    h.LI_().NUMBER("基准单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("大客户立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订件数", nameof(o.min), o.min)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(def))._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_BORN | MSK_EDIT;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Item
                {
                    shpid = org.id,
                    created = DateTime.Now,
                    creator = prin.name,
                });

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO items ").colset(Item.Empty, msk)._VALUES_(Item.Empty, msk);
                await dc.ExecuteAsync(p => m.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }

        [OrglyAuthorize(0, User.ROL_MGT)]
        [Ui("导入", "导入供应产品", icon: "plus-circle", group: 2), Tool(ButtonOpen)]
        public async Task imp(WebContext wc, int state)
        {
            var org = wc[-1].As<Org>();
            var prin = (User)wc.Principal;

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT DISTINCT lotid, concat(srcname, ' ', name), id FROM books WHERE shpid = @1 AND status = 4 ORDER BY id DESC LIMIT 50");
                await dc.QueryAsync(p => p.Set(org.id));
                var map = dc.ToIntMap();

                var o = new Item
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    unitx = 1,
                    min = 1,
                    stock = 30,
                };

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_();

                    h.LI_().SELECT("供应产品", nameof(o.lotid), o.lotid, map, required: true)._LI();
                    h.LI_().TEXT("基准单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("批发件含量", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();
                    h.LI_().NUMBER("基准单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("大客户立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订件数", nameof(o.min), o.min)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(imp))._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_BORN | MSK_EDIT;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Item
                {
                    shpid = org.id,
                    created = DateTime.Now,
                    creator = prin.name,
                });
                var lot = GrabObject<int, Lot>(m.lotid);
                m.typ = lot.typ;
                m.name = lot.name;
                m.tip = lot.tip;

                // insert
                using var dc = NewDbContext(IsolationLevel.ReadCommitted);

                dc.Sql("INSERT INTO items ").colset(Item.Empty, msk)._VALUES_(Item.Empty, msk).T(" RETURNING id");
                var itemid = (int)await dc.ScalarAsync(p => m.Write(p, msk));

                dc.Sql("UPDATE items SET (icon, pic) = (SELECT icon, pic FROM lots WHERE id = @1) WHERE id = @2");
                await dc.ExecuteAsync(p => p.Set(m.lotid).Set(itemid));

                wc.GivePane(200); // close dialog
            }
        }
    }
}