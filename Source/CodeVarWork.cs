using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital.Supply
{
    public class PublyCodeVarWork : WebWork
    {
        public async Task @default(WebContext wc, int page)
        {
            int code = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purchase.Empty).T(" FROM purchs WHERE @1 BETWEEN start AND end");
            var o = dc.QueryTop<Purchase>(p => p.Set(code));


            var prod = Obtain<short, Supply_>(o.prodid);
            var src = Obtain<short, Org_>(o.partyid);
            var ctr = Obtain<short, Org_>(o.ctrid);
        }
    }
}