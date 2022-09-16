using System;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class WareWork : WebWork
    {
        public const string ERR = "table";
    }

    [UserAuthorize(Org.TYP_SRC, User.ORGLY_OPN)]
    [Ui("产品设置", "产源", icon: ERR)]
    public class ShplyWareWork : WareWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyItemVarWork>();
        }

        [Ui("在线", @group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM wares WHERE srcid = @1 AND status >= 1 ORDER BY created DESC");
            var arr = await dc.QueryAsync<Item>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: STA_ENABLED);

                if (arr == null)
                {
                    return;
                }
                h.GRID(arr, o =>
                {
                    h.HEADER_("uk-card-header").AVAR(o.Key, o.name)._HEADER();
                    h.SECTION_("uk-card-body");
                    h._SECTION();
                    h.FOOTER_("uk-card-footer uk-flex-right").TOOLSVAR(o.Key)._FOOTER();
                });
            });
        }

        [Ui("下线", "下线产品", @group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM products WHERE srcid = @1 AND status <= 0 ORDER BY created DESC");
            var arr = await dc.QueryAsync<Item>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: STA_DISABLED);

                if (arr == null)
                {
                    return;
                }
                h.GRID(arr, o =>
                {
                    h.HEADER_("uk-card-header").AVAR(o.Key, o.name)._HEADER();
                    h.SECTION_("uk-card-body");
                    h._SECTION();
                    h.FOOTER_("uk-card-footer uk-flex-right").TOOLSVAR(o.Key)._FOOTER();
                });
            });
        }

        [Ui("新建", "新建产品", "plus", group: 7), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int state)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var cats = Grab<short, Cat>();

            if (wc.IsGet)
            {
                var o = new Item
                {
                    created = DateTime.Now,
                    status = (short) state,
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("标准产品资料");

                    h.LI_().TEXT("产品名称", nameof(o.name), o.name, max: 12).SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().SELECT("贮藏方法", nameof(o.store), o.store, Item.Stores, required: true).SELECT("保存周期", nameof(o.duration), o.duration, Item.Durations, required: true)._LI();
                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).TEXT("单位提示", nameof(o.unitip), o.unitip)._LI();
                    h.LI_().CHECKBOX("只供代理", nameof(o.agt), o.agt).SELECT("状态", nameof(o.status), o.status, Statuses, filter: (k, v) => k >= STA_DISABLED, required: true)._LI();

                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_BORN;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Item
                {
                    prdid = org.id,
                    created = DateTime.Now,
                    creator = prin.name,
                });

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO prods ").colset(Item.Empty, msk)._VALUES_(Item.Empty, msk);
                await dc.ExecuteAsync(p => m.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }
    }
}