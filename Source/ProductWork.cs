using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class ProductWork : WebWork
    {
        public const string ERR = "table";
    }

    public class PublyProductWork : ProductWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyProductVarWork>();
        }
    }

    [UserAuthorize(Org.TYP_SRC, User.ORGLY_OPN)]
    [Ui("产源产品设置", icon: ERR)]
    public class SrclyProductWork : ProductWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyProductVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Product.Empty).T(" FROM products WHERE srcid = @1 ORDER BY state DESC");
            var arr = await dc.QueryAsync<Product>(p => p.Set(src.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
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


        [Ui("新建", "新建产品"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var cats = Grab<short, Cat>();
            if (wc.IsGet)
            {
                var tomorrow = DateTime.Today.AddDays(1);
                var o = new Product
                {
                    state = Entity.STA_DISABLED,
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT("品目名", nameof(o.typ), o.typ, cats, required: true).TEXT("附加名", nameof(o.name), o.name, max: 12)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().SELECT("贮藏方法", nameof(o.store), o.store, Product.Stores, required: true).SELECT("贮藏天数", nameof(o.duration), o.duration, Product.Durations, required: true)._LI();
                    h.LI_().CHECKBOX("只供给代理", nameof(o.agt), o.agt).SELECT("状态", nameof(o.state), o.state, Entity.States, filter: (k, v) => k > 0, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("规格参数");

                    h.LI_().TEXT("销售单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).TEXT("单位倍比", nameof(o.unitip), o.unitip)._LI();

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
}