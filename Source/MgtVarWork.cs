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
        public async Task @default(WebContext wc)
        {
            int id = wc[0];

            var topOrgs = Grab<int, Org>();
            var ctr = topOrgs[id];

            wc.GivePage(200, h =>
            {
                h.TOPBAR_()._TOPBAR();

                for (int i = 0; i < topOrgs.Count; i++)
                {
                    var org = topOrgs.ValueAt(i);
                    if (org.toctrs != null && org.toctrs.Contains(id))
                    {
                        h.LI_();

                        h._LI();
                    }
                }
            });
        }

        public async Task prod(WebContext wc, int sect)
        {
            int ctrid = wc[0];
            var org = GrabObject<int, Org>(ctrid);
            var regs = Grab<short, Reg>();

            // get list of assocated sources
            var orgs = Grab<int, Org>();
            var ids = new ValueList<int>();
            orgs.ForEach((k, v) => v.HasCtr(ctrid), (k, v) => ids.Add(k));

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Product.Empty).T(" FROM products WHERE orgid IN (SELECT id FROM orgs WHERE sprid = @1) ORDER BY typ");
            var bizs = await dc.QueryAsync<Product>(p => p.Set(ctrid).Set(sect));
            wc.GivePage(200, h =>
            {
                h.TOPBAR_().SUBNAV(regs, string.Empty, sect, filter: (k, v) => v.typ == Reg.TYP_SECT);
                h.T("<button class=\"uk-icon-button uk-circle uk-margin-left-auto\" formaction=\"search\" onclick=\"return dialog(this,8,false,4,'&#x1f6d2; 按厨坊下单')\"><span uk-icon=\"search\"></span></button>");
                h._TOPBAR();
                h.GRID(bizs, o =>
                {
                    h.SECTION_("uk-card-body");
                    h._SECTION();
                }, width: 2);
            }, title: org.name);
        }

        public async Task search(WebContext wc, int cur)
        {
        }
    }
}