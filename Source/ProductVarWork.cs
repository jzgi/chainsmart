using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

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


    public class SrclyProductVarWork : ProductVarWork
    {
        public async Task @default(WebContext wc)
        {
            short id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;
            var items = Grab<short, Item>();
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Product.Empty).T(" FROM products WHERE id = @1");
                var o = dc.QueryTop<Product>(p => p.Set(id));
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("品目名", nameof(o.itemid), o.itemid, items, Item.Typs, required: true).TEXT("附加名", nameof(o.ext), o.ext, max: 10)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().SELECT("发货约定", nameof(o.fillg), o.fillg, Product.Fillgs, required: true).DATE("指定日期", nameof(o.fillon), o.fillon)._LI();
                    h.LI_().SELECT("供应级别", nameof(o.rankg), o.rankg, Org.Ranks, required: true).SELECT("状态", nameof(o.status), o.status, Info.Statuses, filter: (k, v) => k > 0, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("规格参数");

                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("标准比", nameof(o.unitx), o.unitx, min: 1, max: 1000, required: true)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step).NUMBER("现存量", nameof(o.cap), o.cap)._LI();
                    h.LI_().SELECT("市场约束", nameof(o.mrtg), o.mrtg, Product.Mrtgs, required: true).NUMBER("市场价", nameof(o.mrtprice), o.mrtprice, min: 0.00M, max: 10000.00M)._LI();

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
                var item = items[m.itemid];
                m.typ = item.typ;
                m.name = item.name + '（' + m.ext + '）';

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE products ")._SET_(Product.Empty, 0).T(" WHERE id = @1");
                await dc.ExecuteAsync(p =>
                {
                    m.Write(p, 0);
                    p.Set(id);
                });

                wc.GivePane(200); // close dialog
            }
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