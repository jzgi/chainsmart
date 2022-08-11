using System;
using System.Threading.Tasks;
using CoChain;
using CoChain.Web;
using static CoChain.Web.Modal;
using static CoChain.Nodal.Store;

namespace Revital
{
    public abstract class ProductVarWork : WebWork
    {
        protected async Task doimg(WebContext wc, string col)
        {
            int id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").T(col).T(" FROM products WHERE id = @1");
                if (dc.QueryTop(p => p.Set(id)))
                {
                    dc.Let(out byte[] bytes);
                    if (bytes == null) wc.Give(204); // no content 
                    else wc.Give(200, new StaticContent(bytes), shared: false, 60);
                }
                else
                    wc.Give(404, shared: true, maxage: 3600 * 24); // not found
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ArraySegment<byte> img = f[nameof(img)];
                using var dc = NewDbContext();
                dc.Sql("UPDATE products SET ").T(col).T(" = @1 WHERE id = @2");
                if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else wc.Give(500); // internal server error
            }
        }
    }

    public class PublyProductVarWork : ProductVarWork
    {
    }

    public class SrclyProductVarWork : ProductVarWork
    {
        public async Task @default(WebContext wc)
        {
            int prodid = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;
            var cats = Grab<short, Cat>();
            var items = Grab<short, Item>();
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Product.Empty).T(" FROM prods WHERE id = @1");
                var o = dc.QueryTop<Product>(p => p.Set(prodid));
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT("品目名", nameof(o.typ), o.typ, cats, required: true).TEXT("名称", nameof(o.name), o.name, max: 12)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().SELECT("贮藏方法", nameof(o.store), o.store, Product.Stores, required: true).SELECT("贮藏天数", nameof(o.duration), o.duration, Product.Durations, required: true)._LI();
                    h.LI_().CHECKBOX("只供给代理", nameof(o.agt), o.agt).SELECT("状态", nameof(o.state), o.state, Entity.States, filter: (k, v) => k > 0, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("规格参数");

                    h.LI_().TEXT("销售单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).TEXT("单位提示", nameof(o.unitip), o.unitip)._LI();

                    h._FIELDSUL();
                    h._FORM();
                });
            }
            else // POST
            {
                // populate 
                var m = await wc.ReadObjectAsync(0, new Product
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                });

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE prods ")._SET_(Product.Empty, 0).T(" WHERE id = @1");
                await dc.ExecuteAsync(p =>
                {
                    m.Write(p, 0);
                    p.Set(prodid);
                });

                wc.GivePane(200); // close dialog
            }
        }

        [Ui("冲量", "限时冲量模式"), Tool(ButtonShow, Appear.Small)]
        public async Task lim(WebContext wc)
        {
            int prodid = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Product.Empty).T(" FROM prods WHERE id = @1");
            var o = dc.QueryTop<Product>(p => p.Set(prodid));

            wc.GivePage(200, h =>
            {
                h.FORM_();
                h.FIELDSUL_("通过优惠，不达量可撤销订单");
                h._FIELDSUL();
                h._FORM();
            });
        }

        [Ui("◩", "照片"), Tool(ButtonCrop, Appear.Large)]
        public async Task img(WebContext wc)
        {
            await doimg(wc, nameof(img));
        }

        [Ui("▤", "质检"), Tool(ButtonCrop, Appear.Full)]
        public async Task cert(WebContext wc)
        {
            await doimg(wc, nameof(cert));
        }

        [Ui("✕", "删除"), Tool(ButtonShow, Appear.Small)]
        public async Task rm(WebContext wc)
        {
            int id = wc[0];
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.ALERT("删除标品？");
                    h.FORM_().HIDDEN(string.Empty, true)._FORM();
                });
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("DELETE FROM products WHERE id = @1");
                await dc.ExecuteAsync(p => p.Set(id));

                wc.GivePane(200);
            }
        }
    }
}