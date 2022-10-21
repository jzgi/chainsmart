using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class ItemVarWork : WebWork
    {
        protected async Task doimg(WebContext wc, string col, bool shared, short maxage)
        {
            int id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").T(col).T(" FROM items WHERE id = @1");
                if (dc.QueryTop(p => p.Set(id)))
                {
                    dc.Let(out byte[] bytes);
                    if (bytes == null) wc.Give(204); // no content 
                    else wc.Give(200, new WebStaticContent(bytes), shared, maxage);
                }
                else
                {
                    wc.Give(404, null, shared, maxage); // not found
                }
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ArraySegment<byte> img = f[nameof(img)];
                using var dc = NewDbContext();
                dc.Sql("UPDATE items SET ").T(col).T(" = @1 WHERE id = @2");
                if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else wc.Give(500); // internal server error
            }
        }
    }

    public class PublyItemVarWork : ItemVarWork
    {
        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon), true, 3600 * 6);
        }

        public async Task pic(WebContext wc)
        {
            await doimg(wc, nameof(pic), true, 3600 * 6);
        }

        public async Task m(WebContext wc, int sub)
        {
            await doimg(wc, nameof(m) + sub, true, 3600 * 6);
        }
    }

    public class SrclyItemVarWork : ItemVarWork
    {
        public async Task @default(WebContext wc)
        {
            int itemid = wc[0];
            var src = wc[-2].As<Org>();
            var cats = Grab<short, Cat>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE id = @1 AND srcid = @2");
            var o = await dc.QueryTopAsync<Item>(p => p.Set(itemid).Set(src.id));

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品名称", o.name)._LI();
                h.LI_().FIELD("类别", o.typ, cats)._LI();
                h.LI_().FIELD("简述", o.tip)._LI();
                h.LI_().FIELD("贮藏方法", o.store, Item.Stores)._LI();
                h.LI_().FIELD2("保存周期", o.duration, "天")._LI();
                h.LI_().FIELD("基础单位", o.unit)._LI();
                h.LI_().FIELD("包装单位", o.unitpkg)._LI();
                h.LI_().FIELD("包装含量", o.unitx)._LI();
                h.LI_().FIELD("只供代理", o.agt)._LI();
                h.LI_().FIELD("状态", o.status, Statuses)._LI();
                h._UL();

                h.TOOLBAR(bottom: true);
            });
        }

        [Ui("修改", "修改产品资料", icon: "pencil"), Tool(ButtonShow)]
        public async Task edit(WebContext wc)
        {
            int itemid = wc[0];
            var src = wc[-2].As<Org>();
            var prin = (User) wc.Principal;
            var cats = Grab<short, Cat>();

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE id = @1");
                var o = dc.QueryTop<Item>(p => p.Set(itemid));
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("修改产品资料");

                    h.LI_().TEXT("产品名称", nameof(o.name), o.name, max: 12).SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().SELECT("贮藏方法", nameof(o.store), o.store, Item.Stores, required: true).NUMBER("保存周期", nameof(o.duration), o.duration, min: 1, required: true)._LI();
                    h.LI_().TEXT("基础单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).TEXT("包装单位", nameof(o.unitpkg), o.unitpkg)._LI();
                    h.LI_().TEXT("包装基础比", nameof(o.unitx), o.unitx, tip: "如有多个值要用空格分开", required: true)._LI();
                    h.LI_().CHECKBOX("只供代理", nameof(o.agt), o.agt).SELECT("状态", nameof(o.status), o.status, Statuses, filter: (k, v) => k >= STU_VOID, required: true)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认")._FORM();
                });
            }
            else // POST
            {
                // populate 
                var m = await wc.ReadObjectAsync(0, new Item
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                });

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE products ")._SET_(Item.Empty, 0).T(" WHERE id = @1 AND srcid = @2");
                await dc.ExecuteAsync(p =>
                {
                    m.Write(p, 0);
                    p.Set(itemid).Set(src.id);
                });

                wc.GivePane(200); // close dialog
            }
        }

        [Ui("图标", icon: "happy"), Tool(ButtonCrop)]
        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon), false, 3);
        }

        [Ui("照片", icon: "image"), Tool(ButtonCrop)]
        public async Task pic(WebContext wc)
        {
            await doimg(wc, nameof(pic), false, 3);
        }

        [Ui("多图", icon: "album"), Tool(ButtonCrop, size: 3, subs: 4)]
        public async Task m(WebContext wc, int sub)
        {
            await doimg(wc, "m" + sub, false, 3);
        }

        [Ui("删除", "确认删除此产品？", icon: "trash"), Tool(ButtonConfirm)]
        public async Task rm(WebContext wc)
        {
            int itemid = wc[0];
            var src = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("DELETE FROM items WHERE id = @1 AND srcid = @1");
            await dc.ExecuteAsync(p => p.Set(itemid).Set(src.id));

            wc.GivePane(200);
        }
    }
}