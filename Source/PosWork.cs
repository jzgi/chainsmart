﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
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

                    h.T("<option value=\"").T(o.id).T("\" unit=\"").T(o.unit).T("\" unitx=\"").T(o.unitx).T("\" price=\"").T(o.price).T("\" avail=\"").T(o.avail).T("\">");
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
                decimal topay = 0;

                h.LI_();
                h.SELECT_(nameof(qtyx))._SELECT();
                h.SPAN_("uk-width-1-1").T("<input type=\"number\" name=\"qty\" class=\"uk-input\" placeholder=\"填写数量\" oninput=\"posRecalc(this);\" step=\"any\" required><output name=\"unit\" class=\"suffix\"></output>")._SPAN();
                h.T("<button type=\"button\" class=\"uk-button-danger uk-width-medium\" onclick=\"return posAdd(this);\">").CNYOUTPUT(nameof(subtotal), subtotal)._BUTTON();
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

                h.BOTTOMBAR_();
                h.T("<a class=\"uk-icon-button\" uk-icon=\"arrow-left\" onclick=\"posResum(ancestorOf(this, 'form'), 1);\"></a>");
                h.BUTTON_(nameof(buy), onclick: "eturn call_pos(this);", css: "uk-button-default uk-width-medium").CNYOUTPUT(nameof(topay), topay)._BUTTON();
                h.T("<a class=\"uk-icon-button\" uk-icon=\"arrow-right\" onclick=\"posResum(ancestorOf(this, 'form'), 2);\"></a>");
                h._BOTTOMBAR();

                h._FORM();
            }, false, 60, onload: "fixAll();");
        }

        [Ui(tip: "零售记录", icon: "list", group: 2), Tool(Anchor)]
        public async Task lst(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE shpid = @1 AND typ = 0 AND status = 4 ORDER BY id DESC");
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
                    h.TD(o.created);
                    h.TD(o.topay);
                });
            }, false, 6);
        }

        [Ui(tip: "已作废", icon: "trash", group: 4), Tool(Anchor)]
        public async Task aborted(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE shpid = @1 AND typ = 1 AND status = 8 ORDER BY id DESC");
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

        public async Task buy(WebContext wc, int cmd)
        {
            int shpid = wc[-1];
            var shp = GrabObject<int, Org>(shpid);
            var prin = (User) wc.Principal;

            var f = await wc.ReadAsync<Form>();
            string addr = f[nameof(addr)];

            // detail lines
            var lines = new List<BuyLn>();
            for (int i = 0; i < f.Count; i++)
            {
                var ety = f.EntryAt(i);
                int id = ety.Key.ToInt();
                short qty = ety.Value;

                if (id <= 0 || qty <= 0) // filter out the non-selected (but submitted)
                {
                    continue;
                }

                lines.Add(new BuyLn
                {
                    wareid = id,
                    qty = qty
                });
            }
        }
    }
}