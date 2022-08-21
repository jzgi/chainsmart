using System.Threading.Tasks;
using ChainFx.Fabric;
using ChainFx.Web;

namespace ChainMart
{
    public class DistribVarWork : WebWork
    {
    }

    public class PublyDistribVarWork : DistribVarWork
    {
        public async Task oid(WebContext wc, int id)
        {
            int distribid = wc[0];
            using var dc = Nodality.NewDbContext();

            dc.Sql("SELECT ").collst(Distrib.Empty).T(" FROM distribs WHERE id = @1");
            var distrib = await dc.QueryTopAsync<Distrib>(p => p.Set(distribid));

            Product product = null;
            if (distrib != null)
            {
                dc.Sql("SELECT ").collst(Product.Empty).T(" FROM products WHERE id = @1");
                product = await dc.QueryTopAsync<Product>(p => p.Set(distrib.productid));
            }

            wc.GivePage(200, h =>
            {
                if (distrib == null || product == null)
                {
                    h.ALERT("没有找到");
                    return;
                }
                h.DIV_();
                h.UL_();
                h.LI_().STATIC("品名", product.name)._LI();
                h._UL();
                h._DIV();
            });
        }
    }
}