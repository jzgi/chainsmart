using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart
{
    public abstract class AssetVarWork : WebWork
    {
        public async Task @default(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Asset.Empty).T(" FROM assets_vw WHERE id = @1 AND orgid = @2");
            var o = await dc.QueryTopAsync<Asset>(p => p.Set(id).Set(org.id));

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("常用名", o.name)._LI();
                h.LI_().FIELD("类别", o.typ, Asset.Typs)._LI();
                h.LI_().FIELD("简介", o.tip)._LI();
                h.LI_().FIELD("基地", o.reserve)._LI();
                h.LI_().FIELD("规格参数", o.specs)._LI();
                h.LI_().FIELD("进展状态", o.status, Org.Statuses)._LI();
                h.LI_().FIELD2("创建", o.created, o.creator)._LI();
                if (o.adapter != null) h.LI_().FIELD2("修改", o.adapted, o.adapter)._LI();
                if (o.fixer != null) h.LI_().FIELD2("上线", o.@fixed, o.fixer)._LI();
                h._UL();

                h.TOOLBAR(bottom: true, status: o.status, state: o.state);
            }, false, 4);
        }

        protected async Task doimg(WebContext wc, string col, bool shared, short maxage)
        {
            int id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").T(col).T(" FROM assets WHERE id = @1");
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
                dc.Sql("UPDATE assets SET ").T(col).T(" = @1 WHERE id = @2");
                if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else wc.Give(500); // internal server error
            }
        }
    }

    public class PublyAssetVarWork : AssetVarWork
    {
        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon), true, 3600 * 4);
        }

        public async Task pic(WebContext wc)
        {
            await doimg(wc, nameof(pic), true, 3600 * 4);
        }

        public async Task m(WebContext wc, int sub)
        {
            await doimg(wc, nameof(m) + sub, true, 3600 * 4);
        }
    }

    public class OrglyAssetVarWork : AssetVarWork
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
                dc.Sql("SELECT ").collst(Asset.Empty).T(" FROM assets WHERE id = @1");
                var o = dc.QueryTop<Asset>(p => p.Set(itemid));
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("修改产品资料");

                    h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 12)._LI();
                    h.LI_().SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().TEXT("基地", nameof(o.reserve), o.reserve, tip: "自产可不填")._LI();
                    // h.LI_().SELECT("贮藏方法", nameof(o.x), o.x, Lot.Stores, required: true).NUMBER("保存天数", nameof(o.y), o.y, min: 1, required: true)._LI();
                    h.LI_().TEXTAREA("规格参数", nameof(o.specs), o.specs, max: 100)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认")._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_TYP | MSK_EDIT;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Asset
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                });

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE assets ")._SET_(Asset.Empty, msk).T(" WHERE id = @1 AND srcid = @2");
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
            await doimg(wc, nameof(icon), false, 4);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("照片", icon: "image"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED, size: 2)]
        public async Task pic(WebContext wc)
        {
            await doimg(wc, nameof(pic), false, 4);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("资料", icon: "album"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED, size: 3, subs: 6)]
        public async Task m(WebContext wc, int sub)
        {
            await doimg(wc, "m" + sub, false, 4);
        }

        [OrglyAuthorize(0, User.ROL_MGT)]
        [Ui("上线", "上线投入使用", icon: "cloud-upload"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
        public async Task ok(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("UPDATE assets SET status = 4, fixed = @1, fixer = @2 WHERE id = @3 AND srcid = @4");
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
            dc.Sql("UPDATE assets SET status = 2, fixed = NULL, fixer = NULL WHERE id = @1 AND srcid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.GivePane(200);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui(tip: "确认删除或者作废此产品？", icon: "trash"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
        public async Task rm(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            try
            {
                dc.Sql("DELETE FROM assets WHERE id = @1 AND srcid = @2");
                await dc.ExecuteAsync(p => p.Set(id).Set(org.id));
            }
            catch (Exception e)
            {
                dc.Sql("UPDATE assets SET status = 0 WHERE id = @1 AND srcid = @2");
                await dc.ExecuteAsync(p => p.Set(id).Set(org.id));
            }

            wc.Give(204);
        }
    }
}