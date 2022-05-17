using System.Threading.Tasks;
using Chainly;
using Chainly.Web;
using static Chainly.Nodal.Store;

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
        public async Task @default(WebContext wc)
        {
            int mrtid = wc[0];
            var mrt = GrabObject<int, Org>(mrtid);
            var regs = Grab<short, Reg>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE sprid = @1 AND status > 0 ORDER BY addr");
            var bizs = await dc.QueryAsync<Org>(p => p.Set(mrtid));
            wc.GivePage(200, h =>
            {
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
            dc.Sql("SELECT ").collst(Supply.Empty).T(" FROM purchs WHERE bizid = @1 AND status > 0 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Supply>(p => p.Set(biz.id));
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
        /// To display provisions related to present center.
        /// </summary>
        public void @default(WebContext wc)
        {
            int ctrid = wc[0];
            var topOrgs = Grab<int, Org>();
            var ctr = topOrgs[ctrid];

            wc.GivePage(200, h =>
            {
                h.GRID(topOrgs, o =>
                {
                    h.HEADER(o.name, "uk-card-header");
                    h.A_("prv-", o.id).T(o.name)._A();
                    h.SECTION_("uk-card-body");
                    h.T(o.tip);
                    h._HEADER();
                }, o => o.IsPrvWith(ctrid));
            }, title: ctr.tip);
        }

        /// <summary>
        /// To display products related to present sector.
        /// </summary>
        public async Task prv(WebContext wc, int prvid)
        {
            int ctrid = wc[0];
            var topOrgs = Grab<int, Org>();
            var ctr = topOrgs[ctrid];
            var prv = topOrgs[prvid];
            var regs = Grab<short, Reg>();

            // get list of assocated sources
            var orgs = Grab<int, Org>();
            var ids = new ValueList<int>();
            orgs.ForEach((k, v) => v.IsPrvWith(ctrid), (k, v) => ids.Add(k));

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM prods WHERE state > 0 AND orgid IN (SELECT id FROM orgs WHERE sprid = @1) ORDER BY typ");
            var arr = await dc.QueryAsync<Ware>(p => p.Set(ctrid).Set(prvid));
            wc.GivePage(200, h =>
            {
                h.TOPBAR_().T(ctr.tip).T(" > ").T(prv.name)._TOPBAR();
                h.GRID(arr, o =>
                {
                    h.SECTION_("uk-card-body");
                    h.T(o.name);
                    h._SECTION();
                }, min: 2);
            });
        }

        public async Task prod(WebContext wc, int prodid)
        {
            int prvid = wc[0];
            var topOrgs = Grab<int, Org>();
            var prv = topOrgs[prvid];
            var items = Grab<short, Item>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM prods WHERE id = @1 AND state > 0");
            var obj = await dc.QueryTopAsync<Ware>(p => p.Set(prodid));
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