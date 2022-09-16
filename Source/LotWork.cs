using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public class LotWork : WebWork
    {
    }

    public class PublyLotWork : LotWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyLotVarWork>();
        }
    }


    [UserAuthorize(Org.TYP_CTR, 1)]
    [Ui("货品批次管理", "产源")]
    public class SrclyLotWork : LotWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyLotVarWork>();
        }

        [Ui("当前", "当前批次", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE srcid = @1 AND status = 1 ORDER BY status DESC");
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
                    h.FOOTER_("uk-card-footer").TOOLSVAR(o.Key)._FOOTER();
                });
            });
        }

        [Ui("历史", "历史批次", @group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE srcid = @1 AND typ = 1 ORDER BY status DESC");
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
                    h.FOOTER_("uk-card-footer").TOOLSVAR(o.Key)._FOOTER();
                });
            });
        }

        [Ui("✛", "新建批次", @group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var orgs = Grab<int, Org>();

            var m = new Lot
            {
                status = Entity.STA_DISABLED,
                prdid = org.id,
                created = DateTime.Now,
                creator = prin.name,
                min = 1, max = 200, step = 1,
            };

            if (wc.IsGet)
            {
                // selection of products
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Item.Empty).T(" FROM products WHERE srcid = @1 AND status > 0");
                var products = await dc.QueryAsync<int, Item>(p => p.Set(org.id));

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("填写不可更改");

                    h.LI_().SELECT("产品", nameof(m.productid), m.productid, products, required: true)._LI();
                    // h.LI_().SELECT(
                    //     org.fork == 1 ? "经由中控" : "投放市场",
                    //     nameof(m.ctrid), m.ctrid, orgs, filter: (k, v) => v.IsCenter, spec: org.fork, required: true
                    // )._LI();
                    h.LI_().CHECKBOX("中控", nameof(m.strict), m.strict)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Entity.Statuses, filter: (k, v) => k > 0, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("订货参数");
                    h.LI_().NUMBER("单价", nameof(m.price), m.price, min: 0.00M, max: 99999.99M).NUMBER("直降", nameof(m.off), m.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(m.min), m.min).NUMBER("限订量", nameof(m.max), m.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(m.step), m.step)._LI();
                    h.LI_().NUMBER("总量", nameof(m.cap), m.cap).NUMBER("剩余量", nameof(m.remain), m.remain)._LI();

                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                // populate 
                const short msk = Entity.MSK_BORN;
                await wc.ReadObjectAsync(msk, instance: m);

                // db insert
                using var dc = NewDbContext();

                dc.Sql("SELECT ").collst(Item.Empty).T(" FROM products WHERE id = @1");
                var product = await dc.QueryTopAsync<Item>(p => p.Set(m.productid));
                m.name = product.name;
                m.tip = product.name;

                dc.Sql("INSERT INTO distribs ").colset(Lot.Empty, msk)._VALUES_(Lot.Empty, msk);
                await dc.ExecuteAsync(p => m.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }

        [Ui("⨧", "新建批次", @group: 1), Tool(ButtonOpen)]
        public async Task @new2(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var orgs = Grab<int, Org>();

            var m = new Lot
            {
                status = Entity.STA_DISABLED,
                prdid = org.id,
                created = DateTime.Now,
                creator = prin.name,
                min = 1, max = 200, step = 1,
            };

            if (wc.IsGet)
            {
                // selection of products
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Item.Empty).T(" FROM products WHERE srcid = @1 AND status > 0");
                var products = await dc.QueryAsync<int, Item>(p => p.Set(org.id));

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("填写不可更改");

                    h.LI_().SELECT("产品", nameof(m.productid), m.productid, products, required: true)._LI();
                    // h.LI_().SELECT(
                    //     org.fork == 1 ? "经由中控" : "投放市场",
                    //     nameof(m.ctrid), m.ctrid, orgs, filter: (k, v) => v.IsCenter, spec: org.fork, required: true
                    // )._LI();
                    h.LI_().CHECKBOX("中控", nameof(m.strict), m.strict)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Entity.Statuses, filter: (k, v) => k > 0, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("订货参数");
                    h.LI_().NUMBER("单价", nameof(m.price), m.price, min: 0.00M, max: 99999.99M).NUMBER("直降", nameof(m.off), m.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(m.min), m.min).NUMBER("限订量", nameof(m.max), m.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(m.step), m.step)._LI();
                    h.LI_().NUMBER("总量", nameof(m.cap), m.cap).NUMBER("剩余量", nameof(m.remain), m.remain)._LI();

                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                // populate 
                const short msk = Entity.MSK_BORN;
                await wc.ReadObjectAsync(msk, instance: m);

                // db insert
                using var dc = NewDbContext();

                dc.Sql("SELECT ").collst(Item.Empty).T(" FROM products WHERE id = @1");
                var product = await dc.QueryTopAsync<Item>(p => p.Set(m.productid));
                m.name = product.name;
                m.tip = product.name;

                dc.Sql("INSERT INTO distribs ").colset(Lot.Empty, msk)._VALUES_(Lot.Empty, msk);
                await dc.ExecuteAsync(p => m.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }
    }

    [UserAuthorize(Org.TYP_CTR, 1)]
    [Ui("货品批次验证", "中控")]
    public class CtrlyLotWork : LotWork
    {
        [Ui("未验批次", @group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE ctrid = @1 AND status = ").T(Lot.STA_PUBLISHED).T(" AND strict = TRUE ORDER BY id DESC");
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
                    h.FOOTER_("uk-card-footer").TOOLSVAR(o.Key)._FOOTER();
                });
            });
        }

        [Ui("☰", "已验批次", @group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE ctrid = @1 AND status = ").T(Lot.STA_PUBLISHED).T(" AND strict = TRUE ORDER BY id DESC");
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
                    h.FOOTER_("uk-card-footer").TOOLSVAR(o.Key)._FOOTER();
                });
            });
        }
    }
}