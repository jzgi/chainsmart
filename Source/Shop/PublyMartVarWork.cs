using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital.Shop
{
    public class PublyMartVarWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PublyMartBizVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            // mart id
            int mrtid = wc[0];

            // get biz list under the mart
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs WHERE spr = @1 ANd status > 0 ORDER BY tag, addr");
            var arr = await dc.QueryAsync<Org>();
            
            // two layer list
            // cache

        }
    }
}