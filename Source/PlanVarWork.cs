using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class PlanVarWork : WebWork
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

        [Ui("✕", "删除"), Tool(ButtonShow, Appear.Small)]
        public async Task rm(WebContext wc)
        {
            short id = wc[0];
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
                dc.Sql("DELETE FROM items WHERE id = @1");
                await dc.ExecuteAsync(p => p.Set(id));

                wc.GivePane(200);
            }
        }
    }


    public class SrclyProductVarWork : PlanVarWork
    {
        [Ui("修改现货供应"), Tool(AnchorOpen)]
        public async Task upd(WebContext wc)
        {
            short id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;
            var items = ObtainMap<short, Item>();
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Product.Empty).T(" FROM products WHERE id = @1");
                var o = dc.QueryTop<Product>(p => p.Set(id));
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("品目名", nameof(o.itemid), o.itemid, items, Item.Cats, filter: x => x.typ == org.fork, required: true).TEXT("附加名", nameof(o.ext), o.ext, max: 10)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().SELECT("供应对象", nameof(o.rank), o.rank, Org.Ranks).SELECT("状态", nameof(o.status), o.status, _Info.Statuses, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("规格参数");

                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("标准比", nameof(o.unitx), o.unitx, min: 1, max: 1000, required: true)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step).NUMBER("现存量", nameof(o.cap), o.cap)._LI();
                    h.LI_().SELECT("市场约束", nameof(o.postg), o.postg, Product.Postgs, required: true).NUMBER("市场价", nameof(o.postprice), o.postprice, min: 0.00M, max: 10000.00M)._LI();

                    h._FIELDSUL();

                    h.BOTTOM_BUTTON("确定");

                    h._FORM();
                });
            }
            else // POST
            {
                // populate 
                var o = await wc.ReadObjectAsync(0, new Product
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                    orgid = org.id,
                });
                var item = items[o.itemid];
                o.cat = item.cat;
                o.name = item.name + '（' + o.ext + '）';

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE products ")._SET_(Product.Empty, 0).T(" WHERE id = @1");
                await dc.ExecuteAsync(p =>
                {
                    o.Write(p, 0);
                    p.Set(id);
                });

                wc.GivePane(200); // close dialog
            }
        }

        [Ui("◑", "项目图标", group: 3), Tool(ButtonCrop, Appear.Small)]
        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon));
        }

        [Ui("◩", "项目照片", group: 3), Tool(ButtonCrop, Appear.Large)]
        public async Task img(WebContext wc)
        {
            await doimg(wc, nameof(img));
        }

        [Ui("▤", "质量证明", group: 3), Tool(ButtonCrop, Appear.Full)]
        public async Task cert(WebContext wc)
        {
            await doimg(wc, nameof(cert));
        }
    }
}