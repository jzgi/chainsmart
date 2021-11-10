using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    /// <summary>
    /// The home directory for a biz.
    /// </summary>
    public class PublyVarVarWork : WebWork
    {
        public async Task @default(WebContext wc, int page)
        {
            int bizid = wc[0];
            var biz = Obtain<int, Org>(bizid);
            var mrt = Obtain<int, Org>(biz.sprid);
            var regs = ObtainMap<short, Reg>();

            // get current posts of this biz 
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Post.Empty).T(" FROM posts WHERE bizid = @1 AND status > 0 ORDER BY created DESC");
            var map = await dc.QueryAsync<int, Post>(p => p.Set(bizid));
            wc.GivePage(200, h =>
            {
                h.TOPBAR_BIZ(biz);

                h.GRID(map, ety =>
                {
                    var v = ety.Value;
                    h.HEADER_().T(v.name)._HEADER();
                });
            }, title: mrt.name);
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