using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public class LotVarWork : WebWork
    {
        public virtual async Task @default(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var topOrgs = Grab<int, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1 AND srcid = @2");
            var o = await dc.QueryTopAsync<Lot>(p => p.Set(lotid).Set(org.id));

            var item = GrabObject<int, Item>(o.itemid);

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品", item.name)._LI();
                h.LI_().FIELD("投放市场", topOrgs[o.ctrid].alias)._LI();
                if (o.IsSelfTransport)
                {
                    h.LI_().FIELD("自达市场", o.mktids, topOrgs)._LI();
                }
                // h.LI_().FIELD("状态", States[o.state])._LI();
                h.LI_().FIELD("单位", o.unit).FIELD("每包装含量", o.unitx)._LI();
                h.LI_().FIELD("单价", o.price).FIELD("立减", o.off)._LI();
                h.LI_().FIELD("起订量", o.min).FIELD("限订量", o.max)._LI();
                h.LI_().FIELD("递增量", o.step)._LI();
                h.LI_().FIELD("本批次总量", o.cap).FIELD("剩余量", o.remain)._LI();
                h.LI_().FIELD("起始溯源号", o.nstart).FIELD("截至溯源号", o.nend)._LI();
                h.LI_().FIELD("处理进展", o.status, Lot.Statuses).FIELD("应用状况", Lot.States[o.state])._LI();
                h.LI_().FIELD2("创建", o.created, o.creator)._LI();
                if (o.adapter != null) h.LI_().FIELD2("调整", o.adapted, o.adapter)._LI();
                if (o.oker != null) h.LI_().FIELD2("上线", o.oked, o.oker)._LI();
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
                dc.Sql("SELECT ").T(col).T(" FROM lots WHERE id = @1");
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
                dc.Sql("UPDATE lots SET ").T(col).T(" = @1 WHERE id = @2");
                if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else
                    wc.Give(500); // internal server error
            }
        }
    }

    public class PublyLotVarWork : LotVarWork
    {
        public override async Task @default(WebContext wc)
        {
            int lotid = wc[0];

            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1");
            var lot = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

            wc.GivePage(200, h =>
            {
                if (lot == null)
                {
                    h.ALERT("无效的溯源产品批次");
                    return;
                }

                var item = GrabObject<int, Item>(lot.itemid);

                var src = GrabObject<int, Org>(lot.srcid);

                h.TOPBARXL_(css: "uk-background-default");
                h.PIC("/item/", item.id, "/icon", css: "uk-width-small");
                h.DIV_("uk-width-expand uk-col uk-padding-left").H2(item.name)._DIV();
                h._TOPBARXL();

                h.DIV_("uk-card uk-card-primary");
                h.H4("批次信息", "uk-card-header");
                h.UL_("uk-card body uk-list uk-list-divider");
                h.LI_().FIELD("批次编号", $"{lot.id:0000 0000}")._LI();
                h.LI_().FIELD("品名", lot.name)._LI();
                h.LI_().FIELD("批次简介", string.IsNullOrEmpty(lot.tip) ? "无" : lot.tip)._LI();
                h.LI_().FIELD2("批次供量", lot.cap, lot.unit, true)._LI();
                h.LI_().FIELD2("创建", lot.created, lot.creator)._LI();
                h.LI_().FIELD2("制码", lot.adapted, lot.adapter)._LI();
                h.LI_().FIELD2("上线", lot.oked, lot.oker)._LI();
                h._UL();
                h._DIV();

                h.DIV_("uk-card uk-card-primary");
                h.H4("批次检验", "uk-card-header");
                if (lot.m1)
                {
                    h.PIC("/lot/", lot.id, "/m-1", css: "uk-width-1-1 uk-card-body");
                }
                if (lot.m2)
                {
                    h.PIC("/lot/", lot.id, "/m-2", css: "uk-width-1-1 uk-card-body");
                }
                if (lot.m3)
                {
                    h.PIC("/lot/", lot.id, "/m-3", css: "uk-width-1-1 uk-card-body");
                }
                if (lot.m4)
                {
                    h.PIC("/lot/", lot.id, "/m-4", css: "uk-width-1-1 uk-card-body");
                }
                h._DIV();

                h.DIV_("uk-card uk-card-primary");
                h.H4("产品详情", "uk-card-header");
                h.DIV_("uk-card-body");
                if (item.pic)
                {
                    h.PIC("/item/", lot.itemid, "/pic", css: "uk-width-1-1");
                }
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("品名", item.name)._LI();
                h.LI_().FIELD("产品描述", string.IsNullOrEmpty(item.tip) ? "无" : item.tip)._LI();
                if (!string.IsNullOrEmpty(item.origin))
                {
                    h.LI_().FIELD("生产基地", item.origin)._LI();
                }
                h.LI_().LABEL("产源／供应").A_("/org/", src.id, "/", css: "uk-button-link uk-active").T(src.fully)._A()._LI();
                h.LI_().FIELD2("创建", item.created, lot.creator)._LI();
                h.LI_().FIELD2("上线", item.oked, lot.oker)._LI();
                h._UL();
                h._DIV();
                h._DIV();

                h.DIV_("uk-card uk-card-primary");
                h.H4("产品证照", "uk-card-header");
                if (item.m1)
                {
                    h.PIC("/item/", item.id, "/m-1", css: "uk-width-1-1 uk-card-body");
                }
                if (item.m2)
                {
                    h.PIC("/item/", item.id, "/m-2", css: "uk-width-1-1 uk-card-body");
                }
                if (item.m3)
                {
                    h.PIC("/item/", item.id, "/m-3", css: "uk-width-1-1 uk-card-body");
                }
                if (item.m4)
                {
                    h.PIC("/item/", item.id, "/m-4", css: "uk-width-1-1 uk-card-body");
                }
                h._DIV();
            }, true, 3600, title: "中惠农通产品溯源信息");
        }

        public async Task m(WebContext wc, int sub)
        {
            await doimg(wc, nameof(m) + sub, true, 900);
        }
    }

    public class SrclyLotVarWork : LotVarWork
    {
        [Ui(tip: "修改产品销售批次", icon: "pencil"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
        public async Task edit(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var topOrgs = Grab<int, Org>();
            var prin = (User) wc.Principal;

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1 AND srcid = @2");
                var o = dc.QueryTop<Lot>(p => p.Set(lotid).Set(org.id));

                await dc.QueryAsync("SELECT id, name FROM items_vw WHERE srcid = @1 AND status = 4", p => p.Set(org.id));
                var items = dc.ToIntMap();

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("产品销售批次信息");

                    h.LI_().SELECT("已上线产品", nameof(o.itemid), o.itemid, items, required: true)._LI();
                    h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, tip: "可选", max: 50)._LI();
                    h.LI_().SELECT("投放市场", nameof(o.ctrid), o.ctrid, topOrgs, filter: (k, v) => v.IsCenter, tip: true, required: true, alias: true)._LI();
                    h.LI_().DATE("预售交货日", nameof(o.futured), o.futured)._LI();
                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).TEXT("包装说明", nameof(o.unitx), o.unitx, min: 2, max: 12)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step)._LI();
                    h.LI_().NUMBER("本批次总量", nameof(o.cap), o.cap).NUMBER("剩余量", nameof(o.remain), o.remain)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_EDIT;
                // populate 
                var o = await wc.ReadObjectAsync(msk, instance: new Lot
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                });

                var item = GrabObject<int, Item>(o.itemid);
                o.name = item.name;
                o.typ = item.typ;

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE lots ")._SET_(Lot.Empty, msk).T(" WHERE id = @1 AND srcid = @2");
                await dc.ExecuteAsync(p =>
                {
                    o.Write(p, msk);
                    p.Set(lotid).Set(org.id);
                });

                wc.GivePane(200); // close dialog
            }
        }

        [OrglyAuthorize(Org.TYP_SRC, User.ROL_OPN)]
        [Ui("资料", icon: "album"), Tool(ButtonCrop, size: 3, subs: 4)]
        public async Task m(WebContext wc, int sub)
        {
            await doimg(wc, nameof(m) + sub, false, 3);
        }

        [OrglyAuthorize(Org.TYP_SRC, User.ROL_OPN)]
        [Ui("自达", icon: "crosshairs"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
        public async Task self(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var topOrgs = Grab<int, Org>();

            int[] mktids;
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ctrid, mktids FROM lots WHERE id = @1 AND srcid = @2");
                await dc.QueryTopAsync<Lot>(p => p.Set(lotid).Set(org.id));
                dc.Let(out int ctrid);
                dc.Let(out mktids);

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("自行货运到达市场（不经过品控中心）");

                    h.LI_().SELECT("自达市场", nameof(mktids), mktids, topOrgs, filter: (k, v) => v.IsMarket && v.ctrid == ctrid, size: 12, required: true)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                mktids = f[nameof(mktids)];

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE lots SET mktids = @1 WHERE id = @2 AND srcid = @3");
                await dc.ExecuteAsync(p => p.Set(mktids).Set(lotid).Set(org.id));

                wc.GivePane(200); // close dialog
            }
        }

        [OrglyAuthorize(Org.TYP_SRC, User.ROL_OPN)]
        [Ui(tip: "删除该产品批次", icon: "trash"), Tool(ButtonConfirm, status: STU_CREATED)]
        public async Task rm(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("DELETE FROM lots WHERE id = @1 AND srcid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.Give(204); // no content
        }

        [OrglyAuthorize(Org.TYP_SRC, User.ROL_EXT)]
        [Ui("溯源", "溯源码绑定或印制", icon: "tag"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
        public async Task tag(WebContext wc, int cmd)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            if (wc.IsGet)
            {
                if (cmd == 0)
                {
                    wc.GivePane(200, h =>
                    {
                        h.FORM_().FIELDSUL_("溯源标签方式");
                        h.LI_().AGOTO("Ａ）绑定预制标签", "tag-1")._LI();
                        h.LI_().AGOTO("Ｂ）现场印制专属贴标", "tag-2")._LI();
                        h._FIELDSUL()._FORM();
                    });
                    return;
                }

                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1 AND srcid = @2");
                var o = dc.QueryTop<Lot>(p => p.Set(lotid).Set(org.id));

                if (cmd == 1)
                {
                    wc.GivePane(200, h =>
                    {
                        h.FORM_().FIELDSUL_("绑定预制标签号码");

                        h.LI_().NUMBER("起始号码", nameof(o.nstart), o.nstart)._LI();
                        h.LI_().NUMBER("截至号码", nameof(o.nend), o.nend)._LI();

                        h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(tag), subscript: cmd)._FORM();
                    });
                }
                else
                {
                    var src = GrabObject<int, Org>(o.srcid);

                    wc.GivePane(200, h =>
                    {
                        int count = o.remain;
                        h.UL_(grid: true, css: "uk-child-width-1-2@s");
                        for (int i = 0; i < count; i++)
                        {
                            h.LI_();
                            h.DIV_("uk-card uk-card-default uk-flex");
                            h.QRCODE(MainApp.WwwUrl + "/lot/x-" + i, css: "uk-width-1-5");
                            h.DIV_("uk-width-expand uk-padding-small").P(src.name).T(i + 1)._DIV();
                            h._DIV();
                            h._LI();
                        }
                        h._UL();
                    });
                }
            }
            else // POST
            {
                if (cmd == 1)
                {
                    var f = await wc.ReadAsync<Form>();
                    int nstart = f[nameof(nstart)];
                    int nend = f[nameof(nend)];

                    // update
                    using var dc = NewDbContext();
                    dc.Sql("UPDATE lots SET nstart = @1, nend = @2, state = 2, status = 2, adapted = @3, adapter = @4 WHERE id = @5");
                    await dc.ExecuteAsync(p => p.Set(nstart).Set(nend).Set(DateTime.Now).Set(prin.name).Set(lotid));
                }
                else if (cmd == 2)
                {
                }

                wc.GivePane(200); // close
            }
        }


        [OrglyAuthorize(Org.TYP_SRC, User.ROL_EXT)]
        [Ui("上线", "上线投入使用", icon: "cloud-upload"), Tool(ButtonConfirm, status: STU_ADAPTED)]
        public async Task ok(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("UPDATE lots SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND srcid = @4");
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

            wc.Give(200);
        }

        [OrglyAuthorize(Org.TYP_SRC, User.ROL_EXT)]
        [Ui("下线", "下线以便修改", icon: "cloud-download"), Tool(ButtonConfirm, status: STU_OKED)]
        public async Task unok(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("UPDATE lots SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND srcid = @2")._MEET_(wc);
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.Give(200);
        }
    }
}