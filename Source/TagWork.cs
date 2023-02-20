using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart
{
    public abstract class TagWork : WebWork
    {
    }

    public class PublyTagWork : TagWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyTagVarWork>();
        }

        public async Task @default(WebContext wc, int id)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT id FROM lots WHERE nend >= @1 AND nstart <= @1 ORDER BY nend ASC LIMIT 1");
            if (await dc.QueryTopAsync(p => p.Set(id)))
            {
                dc.Let(out int lotid);
                wc.GiveRedirect("/lot/" + lotid + "/");
            }
            else
            {
                wc.GivePage(304, h => h.ALERT("此溯源码没有绑定产品"));
            }
        }
    }
}