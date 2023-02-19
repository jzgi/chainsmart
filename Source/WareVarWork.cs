using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Nodality;
using static ChainFX.Web.Modal;

namespace ChainSMart
{
    public class WareVarWork : WebWork
    {
        public virtual async Task @default(WebContext wc)
        {
            int wareid = wc[0];
            var org = wc[-2].As<Org>();

            const short msk = 255 | MSK_EXTRA;
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty, msk).T(" FROM wares_vw WHERE id = @1 AND shpid = @2");
            var m = await dc.QueryTopAsync<Ware>(p => p.Set(wareid).Set(org.id), msk);

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("商品名", m.name)._LI();
                h.LI_().FIELD("简介", string.IsNullOrEmpty(m.tip) ? "无" : m.tip)._LI();
                h.LI_().FIELD("基准单位", m.unit).FIELD2("批发件含量", m.unitx, m.unit)._LI();
                h.LI_().FIELD("基准单价", m.price, money: true).FIELD("优惠立减", m.off, money: true)._LI();
                h.LI_().FIELD("起订件数", m.min).FIELD("限订件数", m.max)._LI();
                h.LI_().FIELD2("剩余库存", m.avail, m.unit)._LI();
                h.LI_().FIELD("状态", Ware.Statuses[m.status])._LI();

                if (m.creator != null) h.LI_().FIELD2("创建", m.creator, m.created)._LI();
                if (m.adapter != null) h.LI_().FIELD2("调整", m.adapter, m.adapted)._LI();
                if (m.fixer != null) h.LI_().FIELD2("上线", m.fixer, m.@fixed)._LI();

                h._UL();

                h.TABLE(m.ops, o =>
                {
                    h.TD_().T(o.dt, time: 1)._TD();
                    h.TD(o.qty, right: true);
                    h.TD(StockOp.Typs[o.typ]);
                    h.TD(o.avail, right: true);
                    h.TD(o.by);
                }, caption: "库存操作记录", reverse: true);

                h.TOOLBAR(bottom: true, status: m.status, state: m.state);
            }, false, 6);
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

    public class PublyWareVarWork : WareVarWork
    {
        public override async Task @default(WebContext wc)
        {
            int wareid = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares_vw WHERE id = @1");
            var o = await dc.QueryTopAsync<Ware>(p => p.Set(wareid));

            wc.GivePane(200, h =>
            {
                h.ARTICLE_("uk-card uk-card-primary");
                if (o.pic)
                {
                    h.PIC("/ware/", o.id, "/pic");
                }
                h.H4("产品详情", "uk-card-header");

                h.SECTION_("uk-card-body");
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品名", o.name)._LI();
                h.LI_().FIELD("产品描述", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();

                h._UL();
                h._SECTION();
                h._ARTICLE();
            }, true, 900);
        }

        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon), true, 3600 * 6);
        }

        public async Task pic(WebContext wc)
        {
            await doimg(wc, nameof(pic), true, 3600 * 6);
        }
    }

    public class ShplyWareVarWork : WareVarWork
    {
        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui(tip: "修改商品信息", icon: "pencil"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
        public async Task edit(WebContext wc)
        {
            int wareid = wc[0];
            var shp = wc[-2].As<Org>();
            var prin = (User) wc.Principal;
            var cats = Grab<short, Cat>();

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares_vw WHERE id = @1");
                var o = dc.QueryTop<Ware>(p => p.Set(wareid));

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("商品信息");

                    h.LI_().TEXT("商品名", nameof(o.name), o.name, max: 12).SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().TEXT("基本单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("每件含量", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("大客户立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订件数", nameof(o.min), o.min).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: 1000)._LI();

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
                    p.Set(wareid).Set(shp.id);
                });

                wc.GivePane(200); // close dialog
            }
        }


        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui(tip: "图标", icon: "github-alt"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED)]
        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon), false, 6);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui(tip: "照片", icon: "image"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED, size: 2)]
        public async Task pic(WebContext wc)
        {
            await doimg(wc, nameof(pic), false, 6);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("库存", icon: "database"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
        public async Task stock(WebContext wc)
        {
            int wareid = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            short typ = 0;
            decimal qty = 0.0M;

            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("库存操作");
                    h.LI_().SELECT("操作类型", nameof(typ), typ, StockOp.Typs, required: true)._LI();
                    h.LI_().NUMBER("数量", nameof(qty), qty, money: false)._LI();
                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(stock))._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                typ = f[nameof(typ)];
                qty = f[nameof(qty)];

                // update
                using var dc = NewDbContext();

                var now = DateTime.Now;
                if (typ < 5) // add
                {
                    dc.Sql("UPDATE wares SET ops[coalesce(array_length(ops,1),0) + 1] = ROW(@1, @2, @3, (avail + @3::NUMERIC(6,1)), @4), avail = avail + @3::NUMERIC(6,1) WHERE id = @5 AND shpid = @6 RETURNING array_length(ops,1)");
                    await dc.QueryTopAsync(p => p.Set(now).Set(typ).Set(qty).Set(prin.name).Set(wareid).Set(org.id));
                    dc.Let(out int len);
                    if (len > 16) // shrink
                    {
                        dc.Sql("UPDATE wares SET ops = ops[13:] WHERE id = @1 AND shpid = @2");
                        await dc.ExecuteAsync(p => p.Set(wareid).Set(org.id));
                    }
                }
                else // reduce
                {
                    dc.Sql("UPDATE wares SET ops[coalesce(array_length(ops,1),0) + 1] = ROW(@1, @2, @3, (avail - @3::NUMERIC(6,1)), @4), avail = avail - @3::NUMERIC(6,1) WHERE id = @5 AND shpid = @6");
                    await dc.ExecuteAsync(p => p.Set(now).Set(typ).Set(qty).Set(prin.name).Set(wareid).Set(org.id));
                }

                wc.GivePane(200); // close dialog
            }
        }

        [OrglyAuthorize(0, User.ROL_MGT)]
        [Ui("上线", "上线投入使用", icon: "cloud-upload"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
        public async Task ok(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("UPDATE wares SET status = 4, fixed = @1, fixer = @2 WHERE id = @3 AND shpid = @4");
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
            dc.Sql("UPDATE wares SET status = 2, fixed = NULL, fixer = NULL WHERE id = @1 AND shpid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.GivePane(200);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui(tip: "确认删除或者作废？", icon: "trash"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
        public async Task rm(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            try
            {
                dc.Sql("DELETE FROM wares WHERE id = @1 AND shpid = @2");
                await dc.ExecuteAsync(p => p.Set(id).Set(org.id));
            }
            catch (Exception e)
            {
                dc.Sql("UPDATE wares SET status = 0 WHERE id = @1 AND shpid = @2");
                await dc.ExecuteAsync(p => p.Set(id).Set(org.id));
            }

            wc.Give(204);
        }
    }
}