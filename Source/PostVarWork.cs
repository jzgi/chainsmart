using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public class PostVarWork : WebWork
    {
    }

    /// 
    /// post
    /// 
    public class PublyPostVarWork : PostVarWork
    {
        public async Task @default(WebContext wc)
        {
            int id = wc[0];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Post.Empty).T(" FROM posts WHERE id = @1");
            var o = await dc.QueryTopAsync<Post>(p => p.Set(id));
            wc.GivePage(200, h =>
            {
                // org

                // item

                // buy
            });
        }

        public async Task buy(WebContext wc)
        {
        }
    }

    public class BizlyPostVarWork : PostVarWork
    {
        [Ui("✎", "✎ 修改", group: 2), Tool(AnchorShow)]
        public async Task upd(WebContext wc)
        {
        }
    }
}