using System.Threading.Tasks;
using SkyChain.Web;

namespace Zhnt.Supply
{
    public class PublyCodeVarWork : WebWork
    {
        public async Task @default(WebContext wc, int page)
        {
            int code = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE @1 BETWEEN start AND end");
            var o = dc.QueryTop<Purch>(p => p.Set(code));


            var prod = ObtainValue<short, Prod>(o.prodid);
            var src = ObtainValue<short, Org>(o.partyid);
            var ctr = ObtainValue<short, Org>(o.ctrid);
        }
    }
}