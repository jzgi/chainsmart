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
        public async Task @default(WebContext wc)
        {
            int itemid = wc[0];
            var src = wc[-2].As<Org>();
            var cats = Grab<short, Cat>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE id = @1 AND srcid = @2");
            var o = await dc.QueryTopAsync<Item>(p => p.Set(itemid).Set(src.id));

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("常用名", o.name)._LI();
                h.LI_().FIELD("类别", o.typ, cats)._LI();
                h.LI_().FIELD("简介", o.tip)._LI();
                h.LI_().FIELD("基地", o.origin)._LI();
                h.LI_().FIELD("贮藏方法", o.store, Item.Stores).FIELD2("保存天数", o.duration, "天")._LI();
                h.LI_().FIELD("规格参数", o.specs)._LI();
                h.LI_().FIELD("进展状态", o.status, Org.Statuses)._LI();
                h.LI_().FIELD2("创建", o.created, o.creator)._LI();
                if (o.adapter != null) h.LI_().FIELD2("修改", o.adapted, o.adapter)._LI();
                if (o.oker != null) h.LI_().FIELD2("上线", o.oked, o.oker)._LI();
                h._UL();

                h.TOOLBAR(bottom: true, status: o.status, state: o.state);
            });
        }

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
        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui(tip: "修改产品资料", icon: "pencil"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
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

                    h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 12)._LI();
                    h.LI_().SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 50)._LI();
                    h.LI_().TEXT("基地", nameof(o.origin), o.origin, tip: "自产可不填")._LI();
                    h.LI_().SELECT("贮藏方法", nameof(o.store), o.store, Item.Stores, required: true).NUMBER("保存天数", nameof(o.duration), o.duration, min: 1, required: true)._LI();
                    h.LI_().TEXTAREA("规格参数", nameof(o.specs), o.specs, max: 100)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认")._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_TYP | MSK_EDIT;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Item
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                });

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE items ")._SET_(Item.Empty, msk).T(" WHERE id = @1 AND srcid = @2");
                await dc.ExecuteAsync(p =>
                {
                    m.Write(p, msk);
                    p.Set(itemid).Set(src.id);
                });

                wc.GivePane(200); // close dialog
            }
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui(tip: "图标", icon: "github-alt"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED)]
        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon), false, 3);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("照片", icon: "image"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED, size: 2)]
        public async Task pic(WebContext wc)
        {
            await doimg(wc, nameof(pic), false, 3);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("资料", icon: "album"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED, size: 3, subs: 6)]
        public async Task m(WebContext wc, int sub)
        {
            await doimg(wc, "m" + sub, false, 3);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui(tip: "确认删除此产品？", icon: "trash"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
        public async Task rm(WebContext wc)
        {
            int itemid = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("DELETE FROM items WHERE id = @1 AND srcid = @2");
            await dc.ExecuteAsync(p => p.Set(itemid).Set(org.id));

            wc.Give(204);
        }

        [OrglyAuthorize(0, User.ROL_MGT)]
        [Ui("上线", "上线投入使用", icon: "cloud-upload"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
        public async Task ok(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("UPDATE items SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND srcid = @4");
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

            wc.GivePane(200);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("下线", "下线以便修改", icon: "cloud-download"), Tool(ButtonConfirm, status: STU_OKED)]
        public async Task unok(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("UPDATE items SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND srcid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.GivePane(200);
        }

        [OrglyAuthorize(0, User.ROL_MGT)]
        [Ui("无效", "将产品设为无效", icon: "ban"), Tool(ButtonConfirm, status: STU_ADAPTED | STU_OKED)]
        public async Task @void(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("UPDATE items SET status = 0 WHERE id = @1 AND srcid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.Give(204); // no content
        }
    }
}