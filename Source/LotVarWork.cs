using System;
using System.Threading.Tasks;
using ChainFx.Fabric;
using ChainFx.Web;
using static ChainFx.Entity;

namespace ChainMart
{
    public class LotVarWork : WebWork
    {
    }

    public class PublyLotVarWork : LotVarWork
    {
        public async Task oid(WebContext wc, int id)
        {
            int distribid = wc[0];
            using var dc = Nodality.NewDbContext();

            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM distribs WHERE id = @1");
            var distrib = await dc.QueryTopAsync<Lot>(p => p.Set(distribid));

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

    public class SrclyLotVarWork : LotVarWork
    {
        public async Task @default(WebContext wc)
        {
            int distribid = wc[0];
            var src = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            if (wc.IsGet)
            {
                using var dc = Nodality.NewDbContext();
                dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM distribs WHERE id = @1");
                var m = dc.QueryTop<Lot>(p => p.Set(distribid));

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本资料");
                    h.LI_().STATIC("产品名称", m.name)._LI();
                    h.LI_().STATIC("", m.name)._LI();
                    h.LI_().STATIC("", m.name)._LI();

                    h._FIELDSUL().FIELDSUL_("订货参数");
                    h.LI_().NUMBER("单价", nameof(m.price), m.price, min: 0.00M, max: 99999.99M).NUMBER("直降", nameof(m.off), m.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(m.min), m.min).NUMBER("限订量", nameof(m.max), m.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(m.step), m.step)._LI();
                    h.LI_().NUMBER("总量", nameof(m.cap), m.cap).NUMBER("剩余量", nameof(m.remain), m.remain)._LI();

                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_EDIT ;

                // populate 
                var m = await wc.ReadObjectAsync(0, new Product
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                });

                // update
                using var dc = Nodality.NewDbContext();
                dc.Sql("UPDATE products ")._SET_(Lot.Empty, msk).T(" WHERE id = @1 AND srcid = @2");
                await dc.ExecuteAsync(p =>
                {
                    m.Write(p, 0);
                    p.Set(distribid).Set(src.id);
                });

                wc.GivePane(200); // close dialog
            }
        }
    }
}