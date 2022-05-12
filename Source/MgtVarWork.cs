using System.Threading.Tasks;
using Chainly;
using Chainly.Web;
using static Chainly.Nodal.Store;

namespace Revital
{
    public class MgtVarWork : WebWork
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
                h.TOPBAR_().H4_().T("与").T(ctr.tip).T("关联的版块")._H4()._TOPBAR();

                h.GRID(topOrgs, o =>
                {
                    h.HEADER(o.name, "uk-card-header");
                    h.SECTION_("uk-card-body");
                    h.T(o.tip);
                    h._HEADER();
                }, o => o.IsPrvWith(ctrid));

                // h.UL_("uk-list");
                // for (int i = 0; i < topOrgs.Count; i++)
                // {
                //     var org = topOrgs.ValueAt(i);
                //     if (org.IsTiedToCtr(ctrid))
                //     {
                //         h.LI_("uk-card uk-card-default");
                //         h._LI();
                //     }
                // }
                // h._UL();
            });
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
            dc.Sql("SELECT ").collst(Prod.Empty).T(" FROM prods WHERE status > 0 AND orgid IN (SELECT id FROM orgs WHERE sprid = @1) ORDER BY typ");
            var arr = await dc.QueryAsync<Prod>(p => p.Set(ctrid).Set(prvid));
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
            dc.Sql("SELECT ").collst(Prod.Empty).T(" FROM prods WHERE id = @1 AND status > 0");
            var obj = await dc.QueryTopAsync<Prod>(p => p.Set(prodid));
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