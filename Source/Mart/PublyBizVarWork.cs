using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;

namespace Revital.Mart
{
    public class PublyBizVarWork : WebWork
    {
        /// <summary>
        /// The home page for the biz.
        /// </summary>
        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[0];
            var orgs = ObtainMap<short, Org>();
            var org = orgs[orgid];

            using var dc = NewDbContext();
        }

        public async Task icon(WebContext wc)
        {
            short id = wc[0];
            using var dc = NewDbContext();
            if (await dc.QueryTopAsync("SELECT icon FROM orgs WHERE id = @1", p => p.Set(id)))
            {
                dc.Let(out byte[] bytes);
                if (bytes == null) wc.Give(204); // no content 
                else wc.Give(200, new StaticContent(bytes), shared: false, 60);
            }
            else wc.Give(404, shared: true, maxage: 3600 * 24); // not found
        }
    }
}