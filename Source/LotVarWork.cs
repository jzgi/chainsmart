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
        public async Task @default(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var items = GrabMap<int, int, Item>(org.id);
            var topOrgs = Grab<int, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1 AND srcid = @2");
            var o = await dc.QueryTopAsync<Lot>(p => p.Set(lotid).Set(org.id));

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品", items[o.itemid].ToString())._LI();
                h.LI_().FIELD("投放市场", topOrgs[o.ctrid].name)._LI();
                if (o.IsSelfTransport)
                {
                    h.LI_().FIELD("自运到市场", o.mktids, topOrgs)._LI();
                }
                h.LI_().FIELD("状态", States[o.state])._LI();
                h.LI_().FIELD("单价", o.price).FIELD("立减", o.off)._LI();
                h.LI_().FIELD("起订量", o.min).FIELD("限订量", o.max)._LI();
                h.LI_().FIELD("递增量", o.step)._LI();
                h.LI_().FIELD("总量", o.cap).FIELD("剩余量", o.remain)._LI();
                h.LI_().FIELD("起始号", o.nstart).FIELD("截至号", o.nend)._LI();
                h._UL();

                h.TOOLBAR(bottom: true, status: o.status);
            });
        }
    }

    public class PublyLotVarWork : LotVarWork
    {
    }

    public class SrclyLotVarWork : LotVarWork
    {
        [Ui("修改", "修改产品资料", icon: "pencil"), Tool(ButtonShow)]
        public async Task edit(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var topOrgs = Grab<int, Org>();
            var items = GrabMap<int, int, Item>(org.id);
            var prin = (User) wc.Principal;

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1 AND srcid = @2");
                var o = dc.QueryTop<Lot>(p => p.Set(lotid).Set(org.id));

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("批次信息");

                    h.LI_().SELECT("产品", nameof(o.itemid), o.itemid, items, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 30)._LI();
                    h.LI_().SELECT("投放市场", nameof(o.ctrid), o.ctrid, topOrgs, filter: (k, v) => v.IsCenter, tip: true, required: true, alias: true)._LI();
                    h.LI_().SELECT("状态", nameof(o.state), o.state, States, filter: (k, v) => k > 0, required: true)._LI();
                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("每包装含量", nameof(o.unitx), o.unitx, required: true)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step)._LI();
                    h.LI_().NUMBER("总量", nameof(o.cap), o.cap).NUMBER("剩余量", nameof(o.remain), o.remain)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_EDIT;
                // populate 
                var o = await wc.ReadObjectAsync(0, new Lot
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                });

                var item = items[o.itemid];
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

        [Ui("自运", "指定自运达市场", icon: "rss"), Tool(ButtonShow)]
        public async Task direct(WebContext wc)
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
                    h.FORM_().FIELDSUL_("设置为自运批次，指定目标市场");

                    h.LI_().SELECT("自运达市场", nameof(mktids), mktids, topOrgs, filter: (k, v) => v.IsMarket && v.ctrid == ctrid, size: 12, required: true)._LI();

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

        [Ui(tip: "删除该产品批次", icon: "trash"), Tool(ButtonConfirm)]
        public async Task rm(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("DELETE FROM lots WHERE id = @1 AND srcid = @2");
            await dc.ExecuteAsync(p => p.Set(lotid).Set(org.id));

            wc.GivePane(200);
        }

        [UserAuthorize(Org.TYP_SRC, User.ROLE_RVW)]
        [Ui("上线", "上线投入使用", icon: "cloud-upload"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
        public async Task ok(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("UPDATE lots SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND srcid = @4");
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

            wc.GivePane(200);
        }

        [UserAuthorize(Org.TYP_SRC, User.ROLE_RVW)]
        [Ui("下线", "下线以便修改", icon: "cloud-download"), Tool(ButtonConfirm, status: STU_OKED)]
        public async Task unok(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("UPDATE lots SET status = 2 WHERE id = @1 AND srcid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.GivePane(200);
        }

        [UserAuthorize(Org.TYP_SRC, User.ROLE_RVW)]
        [Ui("标牌", "标牌号码绑定", icon: "tag"), Tool(ButtonShow)]
        public async Task tag(WebContext wc)
        {
            int lotid = wc[0];
            var ctr = wc[-2].As<Org>();

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1 AND ctrid = @2");
                var m = dc.QueryTop<Lot>(p => p.Set(lotid).Set(ctr.id));

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("输入连续的标牌号码");

                    h.LI_().NUMBER("起始号", nameof(m.nstart), m.nstart)._LI();
                    h.LI_().NUMBER("截至号", nameof(m.nend), m.nend)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(tag))._FORM();
                });
            }
            else
            {
                var f = await wc.ReadAsync<Form>();
                int nstart = f[nameof(nstart)];
                int nend = f[nameof(nend)];

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE lots SET nstart = @1, nend = @2 WHERE id = @3 AND ctrid = @4");
                await dc.ExecuteAsync(p => p.Set(nstart).Set(nend).Set(lotid).Set(ctr.id));

                wc.GivePane(200); // close dialog
            }
        }

        [UserAuthorize(Org.TYP_SRC, User.ROLE_RVW)]
        [Ui("贴标", "打印本批次的贴标", icon: "print"), Tool(ButtonShow)]
        public async Task label(WebContext wc)
        {
            int lotid = wc[0];
            var ctr = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1 AND ctrid = @2");
            var m = dc.QueryTop<Lot>(p => p.Set(lotid).Set(ctr.id));

            var src = GrabObject<int, Org>(m.srcid);

            wc.GivePane(200, h =>
            {
                int count = m.remain;
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
}