using System;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class ItemWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_SRC, 1)]
    [Ui("设置产品", "产源")]
    public class SrclyItemWork : ItemWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyItemVarWork>();
        }

        [Ui("有效产品", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE srcid = @1 AND status > 0 ORDER BY created DESC");
            var map = await dc.QueryAsync<int, Item>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: STA_NORMAL);

                if (map == null)
                {
                    return;
                }
                h.GRIDA(map, o =>
                {
                    h.PIC("", o.id, css: "uk-width-1-5");
                    h.SECTION_("uk-width-4-5");
                    h.T(o.name);
                    h._SECTION();
                });
            });
        }

        [Ui(icon: "ban", group: 2), Tool(Anchor)]
        public async Task ban(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE srcid = @1 AND status <= 0 ORDER BY created DESC");
            var arr = await dc.QueryAsync<Item>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: STA_VOID);

                if (arr == null)
                {
                    return;
                }
                h.GRID(arr, o =>
                {
                    h.HEADER_("uk-card-header").AVAR(o.Key, o.name)._HEADER();
                    h.SECTION_("uk-card-body");
                    h._SECTION();
                    h.FOOTER_("uk-card-footer uk-flex-right").VARTOOLSET(o.Key)._FOOTER();
                });
            });
        }

        [Ui("新建", icon: "plus", group: 7), Tool(ButtonOpen)]
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
                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).TEXT("单位提示", nameof(o.unitstd), o.unitstd)._LI();
                    h.LI_().CHECKBOX("只供代理", nameof(o.agt), o.agt).SELECT("状态", nameof(o.status), o.status, Statuses, filter: (k, v) => k >= STA_VOID, required: true)._LI();

                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_BORN;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Item
                {
                    srcid = org.id,
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