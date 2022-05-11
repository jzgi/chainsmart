using System.Threading.Tasks;
using Chainly;
using Chainly.Web;
using static Chainly.Nodal.Store;

namespace Revital
{
    public class MgtVarWork : WebWork
    {
        /// <summary>
        /// To display sectors related to present provision center.
        /// </summary>
        public void @default(WebContext wc)
        {
            int ctrid = wc[0];
            var topOrgs = Grab<int, Org>();
            var ctr = topOrgs[ctrid];

            wc.GivePage(200, h =>
            {
                h.TOPBAR_().H4_().T("品控中枢：").T(ctr.name)._H4()._TOPBAR();

                h.UL_("uk-list");
                for (int i = 0; i < topOrgs.Count; i++)
                {
                    var org = topOrgs.ValueAt(i);
                    if (org.IsTiedToCtr(ctrid))
                    {
                        h.LI_("uk-card uk-card-default");
                        h.HEADER(org.name, "uk-card-header");
                        h._LI();
                    }
                }
                h._UL();
            });
        }

        /// <summary>
        /// To display products related to present sector.
        /// </summary>
        public async Task sec(WebContext wc, int secid)
        {
            int ctrid = wc[0];
            var topOrgs = Grab<int, Org>();
            var org = Grab<int, Org>();
            var regs = Grab<short, Reg>();

            // get list of assocated sources
            var orgs = Grab<int, Org>();
            var ids = new ValueList<int>();
            orgs.ForEach((k, v) => v.IsTiedToCtr(ctrid), (k, v) => ids.Add(k));

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Prod.Empty).T(" FROM products WHERE status > 0 AND orgid IN (SELECT id FROM orgs WHERE sprid = @1) ORDER BY typ");
            var bizs = await dc.QueryAsync<Prod>(p => p.Set(ctrid).Set(secid));
            wc.GivePage(200, h =>
            {
                h.TOPBAR_().SUBNAV(regs, string.Empty, secid, filter: (k, v) => v.typ == Reg.TYP_SECT);
                h.T("<button class=\"uk-icon-button uk-circle uk-margin-left-auto\" formaction=\"search\" onclick=\"return dialog(this,8,false,4,'&#x1f6d2; 按厨坊下单')\"><span uk-icon=\"search\"></span></button>");
                h._TOPBAR();
                h.GRID(bizs, o =>
                {
                    h.SECTION_("uk-card-body");
                    h._SECTION();
                }, width: 2);
            });
        }

        public async Task product(WebContext wc, int productid)
        {
        }

        public async Task search(WebContext wc, int cur)
        {
        }
    }
}