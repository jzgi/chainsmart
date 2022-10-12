using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public abstract class LotWork : WebWork
    {
    }

    public class PublyLotWork : LotWork
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
                h.LI_().FIELD2("批次供量", lot.cap, item.unitpkg, true)._LI();
                h._UL();
            }, true, 3600, title: "产品溯源信息");
        }

        public async Task tag(WebContext wc, int tagid)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE nstart <= @1 AND nend >= @1");
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
                h.LI_().FIELD2("批次供量", lot.cap, item.unitpkg, true)._LI();
                h._UL();
            }, true, 3600, title: "产品溯源信息");
        }
    }


    [UserAuthorize(Org.TYP_SRC, 1)]
    [Ui("设置产品批次", "产源")]
    public class SrclyLotWork : LotWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyLotVarWork>();
        }

        [Ui("当前批次", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE srcid = @1 AND status >= 1 ORDER BY status DESC, id DESC");
            var map = await dc.QueryAsync<int, Lot>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (map == null) return;
                h.GRIDA(map, o =>
                {
                    h.SECTION_("uk-card-body");
                    h.PIC_().T(ChainMartApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();
                    h.T(o.name);
                    h._SECTION();
                });
            });
        }

        [Ui(icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE srcid = @1 AND status <= 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null) return;
                h.GRID(arr, o =>
                {
                    h.HEADER_("uk-card-header").AVAR(o.Key, o.name)._HEADER();
                    h.SECTION_("uk-card-body");
                    h._SECTION();
                    h.FOOTER_("uk-card-footer").VARTOOLSET(o.Key)._FOOTER();
                });
            });
        }

        [Ui("新建", "新建批次", icon: "plus", group: 3), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var toporgs = Grab<int, Org>();
            var items = GrabMap<int, int, Item>(org.id);

            var now = DateTime.Now;

            var m = new Lot
            {
                status = Entity.STU_VOID,
                srcid = org.id,
                ctring = true,
                created = now,
                creator = prin.name,
                min = 1, max = 200, step = 1,
            };

            if (wc.IsGet)
            {
                // selection of products
                // using var dc = NewDbContext();
                // dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE srcid = @1 AND status > 0");
                // var items = await dc.QueryAsync<int, Item>(p => p.Set(org.id));

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本资料");

                    h.LI_().SELECT("产品", nameof(m.itemid), m.itemid, items, required: true)._LI();
                    h.LI_().SELECT("投放市场", nameof(m.ctrid), m.ctrid, toporgs, filter: (k, v) => v.IsCenter, tip: true, required: true)._LI();
                    h.LI_().CHECKBOX("物流经中控", nameof(m.ctring), m.ctring, check: m.ctring)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Entity.Statuses, filter: (k, v) => k > 0, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("销货参数");
                    h.LI_().NUMBER("单价", nameof(m.price), m.price, min: 0.00M, max: 99999.99M).NUMBER("直降", nameof(m.off), m.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(m.min), m.min).NUMBER("限订量", nameof(m.max), m.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(m.step), m.step)._LI();
                    h.LI_().NUMBER("总量", nameof(m.cap), m.cap).NUMBER("剩余量", nameof(m.remain), m.remain)._LI();

                    h._FIELDSUL();

                    h.BOTTOM_BUTTON("确认", nameof(@new));

                    h._FORM();
                });
            }
            else // POST
            {
                // populate 
                const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
                await wc.ReadObjectAsync(msk, instance: m);

                var item = items[m.itemid];
                m.name = item.name;
                m.typ = item.typ;

                // db insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO lots ").colset(Lot.Empty, msk)._VALUES_(Lot.Empty, msk);
                await dc.ExecuteAsync(p => m.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }
    }

    [UserAuthorize(Org.TYP_CTR, 1)]
    [Ui("验证产品批次", "中控")]
    public class CtrlyLotWork : LotWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<CtrlyLotVarWork>();
        }

        [Ui("待验批次", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE ctrid = @1 ORDER BY id DESC");
            var map = await dc.QueryAsync<int, Lot>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (map == null) return;
                h.GRIDA(map, o =>
                {
                    h.SECTION_("uk-card-body");
                    h.PIC_().T(ChainMartApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();
                    h.T(o.name);
                    h._SECTION();
                });
            });
        }

        [Ui(icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE ctrid = @1 AND status = ").T(Lot.STU_OKED).T(" ORDER BY id DESC");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null) return;
                h.GRID(arr, o =>
                {
                    h.HEADER_("uk-card-header").AVAR(o.Key, o.name)._HEADER();
                    h.SECTION_("uk-card-body");
                    h._SECTION();
                    h.FOOTER_("uk-card-footer").VARTOOLSET(o.Key)._FOOTER();
                });
            });
        }
    }
}