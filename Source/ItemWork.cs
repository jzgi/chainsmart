﻿using System.Threading.Tasks;
using CoChain;
using CoChain.Web;
using static CoChain.Nodal.Store;

namespace CoBiz
{
    public class ItemWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_SHP, 1)]
#if ZHNT
    [Ui("商户商品设置", icon: "table")]
#else
    [Ui("驿站商品设置", icon: "table")]
#endif
    public class ShplyItemWork : ItemWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<ShplyItemVarWork>();
        }

        [Ui("在售", group: 1), Tool(Modal.Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var biz = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM stocks WHERE bizid = @1 AND state  >= 1 ORDER BY id");
            var arr = await dc.QueryAsync<Item>(p => p.Set(biz.id));

            var items = Grab<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    // h.TD(items[o.itemid].name);
                    h.TD(o.id);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("下架", group: 2), Tool(Modal.Anchor)]
        public async Task off(WebContext wc, int page)
        {
            var biz = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM stocks WHERE bizid = @1 AND state  >= 1 ORDER BY id");
            var arr = await dc.QueryAsync<Item>(p => p.Set(biz.id));

            var items = Grab<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    // h.TD(items[o.itemid].name);
                    h.TD(o.id);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("&#128931;", "添加商品", group: 1), Tool(Modal.ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var biz = wc[-1].As<Org>();

            var o = new Item();
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT DISTINCT wareid, typ, name FROM purchs WHERE bizid = @1 ORDER BY typ");
                await dc.QueryAsync(p => p.Set(biz.id));

                wc.GivePane(200, h =>
                {
                    h.FORM_();
                    h.FIELDSUL_("必须是采购过的产品");
                    h.LI_().SELECT_("产源产品", nameof(o.productid));
                    while (dc.Next())
                    {
                    }
                    h._SELECT()._LI();
                    h._FIELDSUL().FIELDSUL_("上架信息");
                    h.LI_().TEXT("销售单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).TEXT("单位倍比", nameof(o.unitip), o.unitip)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step).SELECT("状态", nameof(o.state), o.state, Entity.States, filter: (k, v) => k >= 0, required: true)._LI();
                    h._FIELDSUL();
                    h._FORM();
                });
            }
            else // POST 
            {
            }
        }
    }
}