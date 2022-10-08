using System;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class StockWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_SHP, 1)]
    [Ui("设置货架", "商户")]
    public class ShplyStockWork : StockWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyItemVarWork>();
        }

        [Ui("在售货品", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Stock.Empty).T(" FROM stocks WHERE shpid = @1 AND status > 0 ORDER BY status DESC, id");
            var map = await dc.QueryAsync<int, Stock>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: STU_NORMAL);
                if (map == null)
                {
                    h.ALERT("暂无货品");
                    return;
                }
                h.GRIDA(map, o =>
                {
                    h.PIC_().T(ChainMartApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();
                    h.SECTION_("uk-width-4-5");
                    h.T(o.name);
                    h._SECTION();
                });
            });
        }

        [Ui(icon: "ban", group: 2), Tool(Anchor)]
        public async Task off(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Stock.Empty).T(" FROM stocks WHERE shpid = @1 AND status = 0 ORDER BY id DESC");
            var map = await dc.QueryAsync<int, Stock>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: STU_VOID);
                if (map == null)
                {
                    h.ALERT("暂无货品");
                    return;
                }
                h.GRIDA(map, o =>
                {
                    h.PIC_().T(ChainMartApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();
                    h.SECTION_("uk-width-4-5");
                    h.T(o.name);
                    h._SECTION();
                });
            });
        }

        [Ui("新建", "新建自营货品", icon: "plus", group: 7), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int state)
        {
            var org = wc[-1].As<Org>();

            var prin = (User) wc.Principal;
            var cats = Grab<short, Cat>();

            if (wc.IsGet)
            {
                var o = new Stock
                {
                    created = DateTime.Now,
                    status = (short) state,
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("标准产品资料");

                    h.LI_().TEXT("产品名称", nameof(o.name), o.name, max: 12).SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).TEXT("单位提示", nameof(o.unitstd), o.unitstd)._LI();

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

        [Ui("引入", "从平台引入货品", icon: "reply", group: 7), Tool(ButtonOpen)]
        public async Task use(WebContext wc, int state)
        {
            var org = wc[-1].As<Org>();

            var prin = (User) wc.Principal;
            var cats = Grab<short, Cat>();

            if (wc.IsGet)
            {
                var o = new Stock
                {
                    created = DateTime.Now,
                    status = (short) state,
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("标准产品资料");

                    h.LI_().TEXT("产品名称", nameof(o.name), o.name, max: 12).SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).TEXT("单位提示", nameof(o.unitstd), o.unitstd)._LI();

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