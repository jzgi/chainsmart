using System;
using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    /// 
    /// post
    /// 
    public class PublyPieceVarWork : PieceVarWork
    {
        public async Task @default(WebContext wc)
        {
            int id = wc[0];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Piece.Empty).T(" FROM posts WHERE id = @1");
            var o = await dc.QueryTopAsync<Piece>(p => p.Set(id));
            wc.GivePage(200, h =>
            {
                // org

                // item

                // buy
            });
        }

        public async Task buy(WebContext wc)
        {
        }
    }

    public class BizlyPieceVarWork : PieceVarWork
    {
        public async Task @default(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;
            var items = Grab<short, Item>();
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Piece.Empty).T(" FROM posts WHERE id = @1");
                var o = dc.QueryTop<Piece>(p => p.Set(id));
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("标准品目", nameof(o.itemid), o.itemid, items, Item.Typs, filter: x => x.typ == org.fork, required: true).TEXT("附加名", nameof(o.ext), o.ext, max: 10)._LI();
                    h.LI_().TEXTAREA("特色描述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Info.Statuses, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("规格参数");

                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("标准比", nameof(o.unitx), o.unitx, min: 1, max: 1000, required: true)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step).NUMBER("最大容量", nameof(o.cap), o.cap)._LI();

                    h._FIELDSUL();

                    h.BOTTOM_BUTTON("确定");

                    h._FORM();
                });
            }
            else // POST
            {
                // populate 
                var m = await wc.ReadObjectAsync(0, new Piece
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                    orgid = org.id,
                });
                var item = items[m.itemid];
                m.typ = item.typ;
                m.name = item.name + '（' + m.ext + '）';

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE posts ")._SET_(Piece.Empty, 0).T(" WHERE id = @1");
                await dc.ExecuteAsync(p =>
                {
                    m.Write(p, 0);
                    p.Set(id);
                });

                wc.GivePane(200); // close dialog
            }
        }
    }
}