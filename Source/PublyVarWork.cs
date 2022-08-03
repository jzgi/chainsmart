using System.Threading.Tasks;
using CoChain;
using CoChain.Web;
using static CoChain.Nodal.Store;

namespace Revital
{
    public class PublyVarWork : WebWork
    {
    }


    /// <summary>
    /// The market home page.
    /// </summary>
    public class PublyMrtVarWork : PublyVarWork
    {
        public async Task @default(WebContext wc, int sec)
        {
            int mrtid = wc[0];
            var mrt = GrabObject<int, Org>(mrtid);
            var regs = Grab<short, Reg>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE sprid = @1 AND regid = @2 AND state > 0 ORDER BY addr");
            var bizs = await dc.QueryAsync<Org>(p => p.Set(mrtid).Set(sec));
            if (sec == 0) // when default sect
            {
                wc.Subscript = sec = 99;
                bizs = bizs.AddOf(mrt); // append the supervising market
            }
            wc.GivePage(200, h =>
            {
                h.TOPBAR_().SUBNAV(regs, string.Empty, sec, filter: (k, v) => v.typ == Reg.TYP_STK_SEC)._TOPBAR();
                h.GRID(bizs, o =>
                {
                    h.SECTION_("uk-flex uk-card-body");
                    h.SPAN(o.ShopLabel, "uk-circle-label").SP();
                    h.A_("/biz/", o.id, "/", css: "uk-button-link").T(o.Shop).T(o.name)._A();
                    h._SECTION();
                }, min: 2);
            }, title: mrt.name);
        }
    }

    public class PublyBizVarWork : PublyVarWork
    {
        public async Task @default(WebContext wc)
        {
            int bizid = wc[0];
            var biz = GrabObject<int, Org>(bizid);
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE bizid = @1 AND status > 0 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Purch>(p => p.Set(biz.id));
            wc.GivePage(200, h =>
            {
                h.FORM_().FIELDSUL_();
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
            }, title: biz.name);
        }
    }

    public class PublyCtrVarWork : WebWork
    {
        /// <summary>
        /// To display bound provisions and wares therein.
        /// </summary>
        public async Task @default(WebContext wc)
        {
            int ctrid = wc[0];
            var topOrgs = Grab<int, Org>();
            var ctr = topOrgs[ctrid];

            var lst = new ValueList<int>();
            topOrgs.ForEach((k, v) => v.IsPrvWith(ctrid), (k, v) => lst.Add(v.id));

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty, Entity.EXTRA, "w").T(", o.id AS prvid FROM wares AS w, orgs AS o WHERE w.srcid = o.id AND o.sprid")._IN_(lst.ToArray(), true).T(" AND w.state > 0 AND o.state > 0");
            var arr = await dc.QueryAsync<Ware>();
            wc.GivePage(200, h =>
            {
                h.TABLE_();
                var last = 0;
                for (var i = 0; i < arr?.Length; i++)
                {
                    var o = arr[i];
                    if (o.prvid != last)
                    {
                        var spr = topOrgs[o.prvid];
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    h.TD(o.name);
                    h.TD(o.price, true);
                    h._TR();

                    last = o.prvid;
                }
                h._TABLE();
            }, title: ctr.tip);
        }

        public async Task ware(WebContext wc, int wareid)
        {
            int prvid = wc[0];
            var topOrgs = Grab<int, Org>();
            var prv = topOrgs[prvid];
            var items = Grab<short, Item>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM prods WHERE id = @1 AND state > 0");
            var obj = await dc.QueryTopAsync<Ware>(p => p.Set(wareid));
            wc.GivePage(200, h =>
            {
                h.PIC("/prod/", obj.id, "/icon");
                h.SECTION_();
                h.T(obj.name);

                h._SECTION();

                h.BOTTOMBAR_().BUTTON("付款")._BOTTOMBAR();
            });
        }
    }
}