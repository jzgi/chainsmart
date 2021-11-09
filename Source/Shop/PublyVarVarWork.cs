using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital.Shop
{
    /// <summary>
    /// The home directory for a biz.
    /// </summary>
    public class PublyVarVarWork : WebWork
    {
        public async Task @default(WebContext wc, int page)
        {
            short bizid = wc[0];
            var regs = ObtainMap<short, Reg>();

            using var dc = NewDbContext();
            wc.GivePage(200, h =>
            {
                h.SUBNAV(regs, "", page, filter: (k, v) => v.typ == Reg.TYP_INDOOR);
                h.DIV_()._DIV();
            });
        }

        // public async Task icon(WebContext wc)
        // {
        //     short id = wc[0];
        //     using var dc = NewDbContext();
        //     if (await dc.QueryTopAsync("SELECT icon FROM orgs WHERE id = @1", p => p.Set(id)))
        //     {
        //         dc.Let(out byte[] bytes);
        //         if (bytes == null) wc.Give(204); // no content 
        //         else wc.Give(200, new StaticContent(bytes), shared: false, 60);
        //     }
        //     else wc.Give(404, shared: true, maxage: 3600 * 24); // not found
        // }
    }
}