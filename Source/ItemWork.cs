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

    public class PublyItemWork : ItemWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyItemVarWork>();
        }
    }

    [UserAuthorize(Org.TYP_SRC, 1)]
    [Ui("登记产品资料", "产源")]
    public class SrclyItemWork : ItemWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyItemVarWork>();
        }

        [Ui("当前产品", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE srcid = @1 AND status > 0 ORDER BY created DESC");
            var map = await dc.QueryAsync<int, Item>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: STU_NORMAL);

                if (map == null)
                {
                    h.ALERT("尚无产品");
                    return;
                }

                h.GRIDA(map, o =>
                {
                    h.SECTION_("uk-card-body");
                    h.PIC_(css: "uk-width-1-5").T(ChainMartApp.WwwUrl).T("/item/").T(o.id).T("/icon")._PIC();
                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H4(o.name);
                    h.P(o.tip);
                    h._DIV();
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
                h.TOOLBAR(subscript: STU_VOID);

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
                    h.FORM_().FIELDSUL_("填写产品资料");

                    h.LI_().TEXT("产品名称", nameof(o.name), o.name, max: 12).SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().SELECT("贮藏方法", nameof(o.store), o.store, Item.Stores, required: true).NUMBER("保存周期", nameof(o.duration), o.duration, min: 1, required: true)._LI();
                    h.LI_().TEXT("基础单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).TEXT("包装单位", nameof(o.unitpkg), o.unitpkg)._LI();
                    h.LI_().TEXT("包装基础比", nameof(o.unitx), o.unitx, tip: "如有多个值要用空格分开", required: true)._LI();
                    h.LI_().CHECKBOX("只供代理", nameof(o.agt), o.agt).SELECT("状态", nameof(o.status), o.status, Statuses, filter: (k, v) => k >= STU_VOID, required: true)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认")._FORM();
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