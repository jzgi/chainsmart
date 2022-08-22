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
        [Ui("自销", group: Distrib.TYP_SALE), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Distrib.Empty).T(" FROM distribs WHERE srcid = @1 ORDER BY state DESC");
            var arr = await dc.QueryAsync<Distrib>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Distrib.TYP_SALE);
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

        [Ui("转销", group: Distrib.TYP_TRANSFER), Tool(Anchor)]
        public void sell(WebContext wc)
        {
            wc.GivePane(200, h =>
            {
                //
                h.TOOLBAR(subscript: Distrib.TYP_TRANSFER);
            });
        }

        [Ui("自通", group: Distrib.TYP_DIRECT), Tool(Anchor)]
        public void direct(WebContext wc)
        {
            wc.GivePane(200, h =>
            {
                //
                h.TOOLBAR(subscript: Distrib.TYP_DIRECT);
            });
        }

        [Ui("✛", "新建批发", group: 7), Tool(ButtonShow)]
        public async Task @new(WebContext wc, int distribTyp)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var orgs = Grab<int, Org>();
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Product.Empty).T(" FROM products WHERE srcid = @1 AND state > 0");
                var products = await dc.QueryAsync<int, Product>(p => p.Set(org.id));

                var tomorrow = DateTime.Today.AddDays(1);
                var o = new Distrib
                {
                    typ = (short) distribTyp,
                    state = Entity.STA_DISABLED,
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("新建" + Distrib.Typs[(short) distribTyp]);

                    h.LI_().SELECT("产品", nameof(o.productid), o.productid, products, required: true)._LI();
                    h.LI_().SELECT("控运中枢", nameof(o.ctrid), o.ctrid, orgs, filter: (k, v) => v.IsCenter, required: true)._LI();
                    h.LI_().SELECT("状态", nameof(o.state), o.state, Entity.States, filter: (k, v) => k > 0, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("订货参数");
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("直降", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step).NUMBER("现存量", nameof(o.cap), o.cap)._LI();
                    h.LI_().NUMBER("现存量", nameof(o.cap), o.cap).NUMBER("剩余量", nameof(o.remain), o.remain)._LI();
                    h._FIELDSUL();
                    h._FORM();
                });
            }
            else // POST
            {
                const short msk = Entity.BORN;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Product
                {
                    srcid = org.id,
                    created = DateTime.Now,
                    creator = prin.name,
                });

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO prods ").colset(Product.Empty, msk)._VALUES_(Product.Empty, msk);
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