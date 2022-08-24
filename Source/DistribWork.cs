using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public class DistribWork : WebWork
    {
    }

    public class PublyDistribWork : DistribWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyDistribVarWork>();
        }
    }


    [UserAuthorize(Org.TYP_CTR, 1)]
    [Ui("产源批发管理")]
    public class SrclyDistribWork : DistribWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyDistribVarWork>();
        }

        [Ui("中控批", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Distrib.Empty).T(" FROM distribs WHERE srcid = @1 AND typ = 1 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Distrib>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Distrib.TYP_CTR);
                if (arr == null) return;
                h.GRID(arr, o =>
                {
                    h.HEADER_("uk-card-header").AVAR(o.Key, o.name)._HEADER();
                    h.SECTION_("uk-card-body");
                    h._SECTION();
                    h.FOOTER_("uk-card-footer").TOOLGROUPVAR(o.Key)._FOOTER();
                });
            });
        }

        [Ui("⌹⌹", group: 2), Tool(Anchor)]
        public async Task ctrpast(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Distrib.Empty).T(" FROM distribs WHERE srcid = @1 AND typ = 1 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Distrib>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Distrib.TYP_CTR);
                if (arr == null) return;
                h.GRID(arr, o =>
                {
                    h.HEADER_("uk-card-header").AVAR(o.Key, o.name)._HEADER();
                    h.SECTION_("uk-card-body");
                    h._SECTION();
                    h.FOOTER_("uk-card-footer").TOOLGROUPVAR(o.Key)._FOOTER();
                });
            });
        }

        [Ui("自达批", group: 4), Tool(Anchor)]
        public async Task free(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Distrib.Empty).T(" FROM distribs WHERE srcid = @1 AND typ = 2 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Distrib>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Distrib.TYP_SELF);
                if (arr == null)
                {
                    return;
                }
                h.GRID(arr, o =>
                {
                    h.HEADER_("uk-card-header").AVAR(o.Key, o.name)._HEADER();
                    h.SECTION_("uk-card-body");
                    h._SECTION();
                    h.FOOTER_("uk-card-footer").TOOLGROUPVAR(o.Key)._FOOTER();
                });
            });
        }

        [Ui("⌹", group: 8), Tool(Anchor)]
        public async Task freepast(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Distrib.Empty).T(" FROM distribs WHERE srcid = @1 AND typ = 1 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Distrib>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Distrib.TYP_CTR);
                if (arr == null) return;
                h.GRID(arr, o =>
                {
                    h.HEADER_("uk-card-header").AVAR(o.Key, o.name)._HEADER();
                    h.SECTION_("uk-card-body");
                    h._SECTION();
                    h.FOOTER_("uk-card-footer").TOOLGROUPVAR(o.Key)._FOOTER();
                });
            });
        }

        [Ui("✛", "新建批发", group: 7), Tool(ButtonShow)]
        public async Task @new(WebContext wc, int distribTyp)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var orgs = Grab<int, Org>();

            var m = new Distrib
            {
                typ = (short) distribTyp,
                status = Entity.STA_DISABLED,
                srcid = org.id,
                created = DateTime.Now,
                creator = prin.name,
                min = 1, max = 200, step = 1,
            };

            if (wc.IsGet)
            {
                // selection of products
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Product.Empty).T(" FROM products WHERE srcid = @1 AND status > 0");
                var products = await dc.QueryAsync<int, Product>(p => p.Set(org.id));

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("新建" + Distrib.Typs[(short) distribTyp]);

                    h.LI_().SELECT("产品", nameof(m.productid), m.productid, products, required: true)._LI();
                    h.LI_().SELECT(
                        distribTyp == Distrib.TYP_CTR ? "经由中控" : "投放市场",
                        nameof(m.ctrid), m.ctrid, orgs, filter: (k, v) => v.IsCenter, required: true, spec: (short) distribTyp
                    )._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Entity.States, filter: (k, v) => k > 0, required: true)._LI();

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

                dc.Sql("SELECT ").collst(Product.Empty).T(" FROM products WHERE id = @1");
                var product = await dc.QueryTopAsync<Product>(p => p.Set(m.productid));
                m.name = product.name;
                m.tip = product.name;

                dc.Sql("INSERT INTO distribs ").colset(Distrib.Empty, msk)._VALUES_(Distrib.Empty, msk);
                await dc.ExecuteAsync(p => m.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }
    }

    [UserAuthorize(Org.TYP_CTR, 1)]
    [Ui("中枢产品批次管理")]
    public class CtrlyDistribWork : DistribWork
    {
        public void @default(WebContext wc)
        {
        }
    }
}