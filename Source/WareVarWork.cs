using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public class WareVarWork : WebWork
    {
        public virtual async Task @default(WebContext wc)
        {
            int wareid = wc[0];
            var org = wc[-2].As<Org>();
            var topOrgs = Grab<int, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares_vw WHERE id = @1 AND shpid = @2");
            var o = await dc.QueryTopAsync<Ware>(p => p.Set(wareid).Set(org.id));

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品名", o.name)._LI();
                h.LI_().FIELD("简介", o.tip)._LI();
                h.LI_().FIELD("计价单位", o.unit).FIELD("每件含量", o.unitx, false)._LI();
                h.LI_().FIELD("单价", o.price).FIELD("立减", o.off)._LI();
                h.LI_().FIELD("起订件数", o.min).FIELD("限订件数", o.max)._LI();
                h.LI_().FIELD("递增", o.step)._LI();
                h.LI_().FIELD("处理进展", o.status, Ware.Statuses).FIELD("应用状况", States[o.state])._LI();
                h.LI_().FIELD2("创建", o.created, o.creator, "&nbsp;")._LI();
                if (o.adapter != null) h.LI_().FIELD2("调整", o.adapted, o.adapter, "&nbsp;")._LI();
                if (o.oker != null) h.LI_().FIELD2("上线", o.oked, o.oker, "&nbsp;")._LI();
                h._UL();

                h.TOOLBAR(bottom: true, status: o.status, state: o.state);
            });
        }

        protected async Task doimg(WebContext wc, string col, bool shared, int maxage)
        {
            int id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").T(col).T(" FROM wares WHERE id = @1");
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
                dc.Sql("UPDATE wares SET ").T(col).T(" = @1 WHERE id = @2");
                if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else
                    wc.Give(500); // internal server error
            }
        }
    }

    public class ShplyWareVarWork : WareVarWork
    {
        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui(tip: "修改产品资料", icon: "pencil"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
        public async Task edit(WebContext wc)
        {
            int wareid = wc[0];
            var src = wc[-2].As<Org>();
            var prin = (User) wc.Principal;
            var cats = Grab<short, Cat>();

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares_vw WHERE id = @1");
                var o = dc.QueryTop<Ware>(p => p.Set(wareid));

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("产品和销售信息");

                    h.LI_().TEXT("产品名", nameof(o.name), o.name, max: 12).SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().TEXT("计价单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("每件含量", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("大客户立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订件数", nameof(o.min), o.min).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增", nameof(o.step), o.step)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_TYP | MSK_EDIT;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Ware
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                });

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE wares ")._SET_(Ware.Empty, msk).T(" WHERE id = @1 AND shpid = @2");
                await dc.ExecuteAsync(p =>
                {
                    m.Write(p, msk);
                    p.Set(wareid).Set(src.id);
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
        [Ui(tip: "确认删除此商品？", icon: "trash"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
        public async Task rm(WebContext wc)
        {
            int itemid = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("DELETE FROM wares WHERE id = @1 AND srcid = @2");
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
            dc.Sql("UPDATE wares SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND shpid = @4");
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
            dc.Sql("UPDATE wares SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND shpid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.GivePane(200);
        }

        [OrglyAuthorize(0, User.ROL_MGT)]
        [Ui("无效", "将商品设为无效", icon: "ban"), Tool(ButtonConfirm, status: STU_ADAPTED | STU_OKED)]
        public async Task @void(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("UPDATE wares SET status = 0 WHERE id = @1 AND shpid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.Give(204); // no content
        }
    }
}