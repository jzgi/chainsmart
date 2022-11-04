using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainMart
{
    public class PublyVarWork : WebWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyVarVarWork>();
        }

        public async Task @default(WebContext wc, int sec)
        {
            int mrtid = wc[0];
            var mrt = GrabObject<int, Org>(mrtid);
            var regs = Grab<short, Reg>();

            if (sec == 0) // when default sect
            {
                wc.Subscript = sec = regs.First(v => v.IsSection).id;
            }

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE (prtid = @1 OR id = @1) AND regid = @2 AND status > 0 ORDER BY addr");
            var arr = await dc.QueryAsync<Org>(p => p.Set(mrtid).Set(sec));

            wc.GivePage(200, h =>
            {
                h.NAVBAR(regs, string.Empty, sec, filter: (k, v) => v.IsSection);

                if (arr == null)
                {
                    h.ALERT("尚无商户");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");

                    if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(ChainMartApp.WwwUrl).T("/org/").T(o.id).T("/icon")._PIC();
                    }
                    else
                    {
                        h.PIC("/void.webp", css: "uk-width-1-5");
                    }

                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name);
                    h.P(o.tip);
                    h._DIV();

                    h._A();
                });
            }, true, 900, mrt.name);
        }
    }

    public class PublyVarVarWork : WebWork
    {
        public async Task @default(WebContext wc)
        {
            int bizid = wc[0];
            var biz = GrabObject<int, Org>(bizid);

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares WHERE shpid = @1 AND status > 0 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Ware>(p => p.Set(biz.id));

            wc.GivePage(200, h =>
            {
                h.FORM_().FIELDSUL_();
                if (arr == null)
                {
                    return;
                }

                foreach (var o in arr)
                {
                    h.LI_("uk-card uk-card-default");
                    h.HEADER_("uk-card-header").T(o.name)._HEADER();
                    h.DIV_();
                    h.SELECT(null, nameof(o.name), 1, new int[] {1, 2, 3});
                    h._DIV();
                    h._LI();
                }
                h._FIELDSUL();
                h.BOTTOMBAR_().BUTTON("付款")._BOTTOMBAR();
                h._FORM();
            }, true, 900, title: biz.name);
        }
    }


    public class PublyCtrVarWork : WebWork
    {
        /// <summary>
        /// To display territories marked by centers.
        /// </summary>
        public async Task @default(WebContext wc)
        {
            int ctrid = wc[0];
            var topOrgs = Grab<int, Org>();
            var ctr = topOrgs[ctrid];
            var cats = Grab<short, Cat>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE ctrid = @1 AND status > 0");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(ctrid));

            wc.GivePage(200, h =>
            {
                if (arr == null)
                {
                    h.ALERT("没有在批发的产品");
                    return;
                }

                h.NAVBAR(cats, "", 0);

                h.TABLE_();
                var last = 0;
                for (var i = 0; i < arr?.Length; i++)
                {
                    var o = arr[i];
                    // if (o.prvid != last)
                    // {
                    //     var spr = topOrgs[o.prvid];
                    //     h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
                    // }
                    h.TR_();
                    h.TD(o.name);
                    // h.TD(o.price, true);
                    h._TR();

                    // last = o.prvid;
                }
                h._TABLE();
            }, title: ctr.tip);
        }

        public async Task lot(WebContext wc, int lotid)
        {
            int prvid = wc[0];
            var topOrgs = Grab<int, Org>();
            var prv = topOrgs[prvid];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1 AND status > 0");
            var obj = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

            wc.GivePage(200, h =>
            {
                h.PIC("/prod/", obj.id, "/icon");
                h.SECTION_();
                h.T(obj.name);

                h._SECTION();

                h.BOTTOMBAR_().BUTTON("付款")._BOTTOMBAR();
            }, title: prv.tip);
        }

    }
}