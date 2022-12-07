using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;
using static ChainFx.Web.ToolAttribute;

namespace ChainMart
{
    public abstract class LotWork<V> : WebWork where V : LotVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    public class PublyLotWork : LotWork<PublyLotVarWork>
    {
        public async Task @default(WebContext wc, int lotid)
        {
            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1");
            var lot = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

            wc.GivePage(200, h =>
            {
                if (lot == null)
                {
                    h.ALERT("没有找到产品");
                    return;
                }

                var item = GrabMap<int, int, Item>(lot.srcid)[lot.itemid];

                var src = GrabObject<int, Org>(lot.srcid);

                h.TOPBARXL_();
                h.PIC("/item/", lot.itemid, "/icon", circle: true, css: "uk-width-small");
                h.DIV_("uk-width-expand uk-col uk-padding-small-left").H2(item.name)._DIV();
                h._TOPBARXL();

                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("品名", item.name)._LI();
                h.LI_().FIELD("描述", item.tip)._LI();
                h.LI_().FIELD("产源", src.name)._LI();
                h.LI_().FIELD("批次号码", lot.id)._LI();
                h.LI_().FIELD("批次创建", lot.created)._LI();
                h.LI_().FIELD2("批次供量", lot.cap, lot.unit, true)._LI();
                h._UL();
            }, true, 3600, title: "产品溯源信息");
        }

        public async Task tag(WebContext wc, int tagid)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE nend >= @1 AND nstart <= @1");
            var lot = await dc.QueryTopAsync<Lot>(p => p.Set(tagid));

            wc.GivePage(200, h =>
            {
                if (lot == null)
                {
                    h.ALERT("没有找到产品");
                    return;
                }

                var item = GrabMap<int, int, Item>(lot.srcid)[lot.itemid];

                var src = GrabObject<int, Org>(lot.srcid);

                h.TOPBARXL_();
                h.PIC("/item/", lot.itemid, "/icon", circle: true, css: "uk-width-small");
                h.DIV_("uk-width-expand uk-col uk-padding-small-left").H2(item.name)._DIV();
                h._TOPBARXL();

                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("品名", item.name)._LI();
                h.LI_().FIELD("描述", item.tip)._LI();
                h.LI_().FIELD("产源", src.name)._LI();
                h.LI_().FIELD("批次号码", lot.id)._LI();
                h.LI_().FIELD("批次创建", lot.created)._LI();
                h.LI_().FIELD2("批次供量", lot.cap, lot.unit, true)._LI();
                h._UL();
            }, true, 3600, title: "产品溯源信息");
        }
    }


    [UserAuthorize(Org.TYP_SRC, 1)]
    [Ui("产品批次管理", "产源")]
    public class SrclyLotWork : LotWork<SrclyLotVarWork>
    {
        [Ui("产品批次", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE srcid = @1 AND status > 0 ORDER BY status DESC, id DESC");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("暂无产品批次");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN("")._HEADER();
                    h.P(o.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            });
        }

        [Ui(icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE srcid = @1 AND status = 8 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN("")._HEADER();
                    h.P(o.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            });
        }

        [Ui("新建", "新建产品批次", icon: "plus", group: 3), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var topOrgs = Grab<int, Org>();
            var items = GrabMap<int, int, Item>(org.id);

            var zon = org.prtid == 0 ? org : topOrgs[org.prtid];

            var o = new Lot
            {
                status = Entity.STU_CREATED,
                srcid = org.id,
                srcname = org.name,
                zonid = zon.id,
                created = DateTime.Now,
                creator = prin.name,
                min = 1, max = 200, step = 1,
            };

            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本资料");

                    h.LI_().SELECT("产品", nameof(o.itemid), o.itemid, items, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 30)._LI();
                    h.LI_().SELECT("投放市场", nameof(o.ctrid), o.ctrid, topOrgs, filter: (k, v) => v.IsCenter, tip: true, required: true, alias: true)._LI();
                    h.LI_().SELECT("状态", nameof(o.state), o.state, Entity.States, filter: (k, v) => k > 0, required: true)._LI();
                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("每包装含有", nameof(o.unitx), o.unitx, min: 1, required: true)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step)._LI();
                    h.LI_().NUMBER("总量", nameof(o.cap), o.cap).NUMBER("剩余量", nameof(o.remain), o.remain)._LI();

                    h._FIELDSUL();

                    h.BOTTOM_BUTTON("确认", nameof(@new));

                    h._FORM();
                });
            }
            else // POST
            {
                // populate 
                const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
                await wc.ReadObjectAsync(msk, instance: o);

                var item = items[o.itemid];
                o.name = item.name;
                o.typ = item.typ;

                // db insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO lots ").colset(Lot.Empty, msk)._VALUES_(Lot.Empty, msk);
                await dc.ExecuteAsync(p => o.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }
    }
}