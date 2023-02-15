using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSMart
{
    public abstract class PosWork<V> : WebWork where V : PosVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    [OrglyAuthorize(Org.TYP_SHP, User.ROL_OPN)]
    [Ui("零售终端", "商户")]
    public class ShplyPosWork : PosWork<ShplyPosVarWork>
    {
        [Ui("零售终端", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares WHERE shpid = @1 AND status = 4 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Ware>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                int wareid = 0;
                decimal qtyx = 0;

                //
                // form input

                h.FORM_().FIELDSUL_();
                h.LI_().SELECT_(nameof(wareid), onchange: "posWareChange(this);").T("<option></option>");

                for (var i = 0; i < arr?.Length; i++)
                {
                    var o = arr[i];

                    h.T("<option value=\"").T(o.id).T("\" itemid=\"").T(o.itemid).T("\" name=\"").T(o.name).T("\" unit=\"").T(o.unit).T("\" unitx=\"").T(o.unitx).T("\" price=\"").T(o.price).T("\" avail=\"").T(o.avail).T("\">");
                    h.T(o.name);
                    if (o.unitx != 1)
                    {
                        h.SP().T(o.unitx).T(o.unit).T("件");
                    }
                    h.T("</option>");
                }
                h._SELECT();

                h.SPAN_("uk-width-medium").T("<input type=\"number\" name=\"price\" class=\"uk-input\" placeholder=\"填写价格\" local=\"\" onchange=\"posRecalc(this);\" step=\"any\" required><output class=\"suffix\">元</output>")._SPAN();
                h._LI();

                decimal subtotal = 0;
                decimal pay = 0;

                h.LI_();
                h.SELECT_(nameof(qtyx))._SELECT();
                h.SPAN_("uk-width-1-1").T("<input type=\"number\" name=\"qty\" class=\"uk-input\" placeholder=\"填写数量\" oninput=\"posRecalc(this);\" step=\"any\" required><output name=\"unit\" class=\"suffix\"></output>")._SPAN();
                h.T("<button type=\"button\" class=\"uk-button-danger uk-width-medium\" onclick=\"return posAdd(this);\">").CNYOUTPUT(nameof(subtotal), subtotal).ICON("arrow-down", "uk-position-right")._BUTTON();
                h._LI();

                h._FIELDSUL()._FORM();

                // 
                // form lines
                //
                h.FORM_();

                h.TABLE_();
                h.T("<thead>").TH("商品", css: "uk-width-1-2").TH("单价").TH("数量").TH("小计").T("<th class=\"uk-width-micro\"></th></thead>");
                h.T("<tbody id=\"lns\">"); // lines
                h.T("</tbody>");
                h._TABLE();

                h.NAV_(css: "uk-flex-center");
                h.T("<a class=\"uk-icon-button\" uk-icon=\"arrow-left\" onclick=\"posResum(ancestorOf(this, 'form'), 1);\"></a>");
                h.NUMBER(null, nameof(pay), pay, @readonly: true, css: "uk-width-medium uk-text-right");
                h.T("<a class=\"uk-icon-button\" uk-icon=\"arrow-right\" onclick=\"posResum(ancestorOf(this, 'form'), 2);\"></a>");
                h._NAV();

                h.BOTTOMBAR_();
                for (short i = 2; i <= 4; i++)
                {
                    h.BUTTON(Buy.Typs[i], nameof(buy), subscript: i, onclick: "return call_pos(this);");
                }
                h._BOTTOMBAR();

                h._FORM();
            }, false, 60, onload: "fixAll();");
        }

        [Ui(tip: "零售记录", icon: "list", group: 2), Tool(Anchor)]
        public async Task lst(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE shpid = @1 AND typ >= 2 AND status = 4 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("尚无消费记录");
                    return;
                }

                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.created, 2, 2)._TD();
                    h.TD(o.pay);
                    h.TD(o.ender);
                });
            }, false, 6);
        }

        [Ui(tip: "已作废", icon: "trash", group: 4), Tool(Anchor)]
        public async Task aborted(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE shpid = @1 AND typ >= 2 AND status = 8 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("尚无零售记录");
                }
            });
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("汇总", "汇总当日记录", icon: "plus-circle", group: 2), Tool(ButtonOpen)]
        public async Task sum(WebContext wc, int state)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var cats = Grab<short, Cat>();

            if (wc.IsGet)
            {
                var o = new Item
                {
                    created = DateTime.Now,
                    state = (short) state,
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("填写产品资料");

                    h.LI_().FIELD("金额", 0.0M)._LI();

                    h._FIELDSUL()._FORM();
                });
            }
        }


        public async Task buy(WebContext wc, int payTyp)
        {
            var shp = wc[-1].As<Org>();
            var prin = (User) wc.Principal;

            var frm = await wc.ReadAsync<Form>();
            decimal pay = frm[nameof(pay)];

            // detail lines
            var lst = new List<BuyLn>();
            for (var i = 0; i < frm.Count; i++)
            {
                var ety = frm.EntryAt(i);
                int wareid = ety.Key.ToInt();
                if (wareid == 0)
                {
                    continue;
                }
                var comp = ((string) ety.Value).Split('-');

                lst.Add(new BuyLn(wareid, comp));
            }

            if (lst.Count == 0) return;

            var now = DateTime.Now;
            var m = new Buy()
            {
                typ = (short) payTyp,
                shpid = shp.id,
                name = shp.name,
                mktid = shp.MarketId,
                created = now,
                creator = prin.name,
                lns = lst.ToArray(),
                status = STU_OKED,
                ended = now,
                ender = prin.name,
                pay = pay,
            };
            m.SetToPay();

            const short msk = MSK_BORN | MSK_EDIT | MSK_LATER;

            using var dc = NewDbContext();

            dc.Sql("INSERT INTO buys ").colset(Buy.Empty, msk)._VALUES_(Buy.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));
        }
    }
}