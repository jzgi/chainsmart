using System.Threading.Tasks;
using ChainFx.Fabric;
using ChainFx.Web;

namespace ChainMart
{
    public abstract class PublyWork : WebWork
    {
    }


    public class PublyTraceWork : PublyWork
    {
        public async Task oid(WebContext wc, int id)
        {
            int distribid = wc[0];
            using var dc = Nodality.NewDbContext();

            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM distribs WHERE id = @1");
            var distrib = await dc.QueryTopAsync<Lot>(p => p.Set(distribid));

            Item item = null;
            if (distrib != null)
            {
                dc.Sql("SELECT ").collst(Item.Empty).T(" FROM products WHERE id = @1");
                item = await dc.QueryTopAsync<Item>(p => p.Set(distrib.productid));
            }

            wc.GivePage(200, h =>
            {
                if (distrib == null || item == null)
                {
                    h.ALERT("没有找到");
                    return;
                }
                h.DIV_();
                h.UL_();
                h.LI_().STATIC("品名", item.name)._LI();
                h._UL();
                h._DIV();
            });
        }
    }
}