using System.Threading.Tasks;
using SkyChain.Web;

namespace Supply
{
    public class PublyCodeVarWork : WebWork
    {
        public async Task @default(WebContext wc, int page)
        {
            int code = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE @1 BETWEEN start AND end");
            var o = dc.QueryTop<Purch>(p => p.Set(code));


            var prod = Obtain<short, Plan>(o.prodid);
            var src = Obtain<short, Org>(o.partyid);
            var ctr = Obtain<short, Org>(o.ctrid);
        }
    }
}