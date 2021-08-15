using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Zhnt
{
    public class AdmlyDItemVarWork : ItemVarWork
    {
        [Ui("✎", "✎ 修改", group: 2), Tool(AnchorShow)]
        public async Task upd(WebContext wc)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE id = @1");
                var o = dc.QueryTop<Item>(p => p.Set(id));
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("标品属性");
                    h.LI_().SELECT("类别", nameof(o.typ), o.typ, Item.Typs)._LI();
                    h.LI_().TEXT("品名", nameof(o.name), o.name, max: 10, required: true)._LI();
                    h.LI_().TEXT("亮点", nameof(o.tip), o.tip, max: 10)._LI();
                    h.LI_().SELECT("方案关联", nameof(o.progg), o.progg, Item.Progg)._LI();
                    h.LI_().NUMBER("价格", nameof(o.price), o.price, max: 500.00M, min: 0.00M, required: true)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, Item.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Item>(0);
                using var dc = NewDbContext();
                dc.Sql("UPDATE items")._SET_(Item.Empty, 0).T(" WHERE id = @1");
                dc.Execute(p =>
                {
                    o.Write(p, 0);
                    p.Set(id);
                });
                wc.GivePane(200); // close
            }
        }

        [Ui("☷", "☷ 成分"), Tool(ButtonOpen)]
        public async Task cnt(WebContext wc, int cmd)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.QueryTop("SELECT ingrs FROM items WHERE id = @1", p => p.Set(id));
                wc.GivePane(200, h => { });
            }
            else // POST
            {
                using var dc = NewDbContext();
                dc.QueryTop("SELECT ingrs FROM items WHERE id = @1", p => p.Set(id));
                if (cmd == 1) // new
                {
                }
                else if (cmd == 2) // remove
                {
                    short matid = (await wc.ReadAsync<Form>())[nameof(matid)];
                }
                dc.Sql("UPDATE items SET ingrs = @1 WHERE id = @2");

                // redirect to list
                wc.GiveRedirect(nameof(cnt));
            }
        }

        [Ui("✕", "删除"), Tool(ButtonShow, Appear.Small)]
        public async Task rm(WebContext wc)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.ALERT("删除标品？");
                    h.FORM_().HIDDEN(string.Empty, true)._FORM();
                });
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("DELETE FROM items WHERE id = @1");
                await dc.ExecuteAsync(p => p.Set(id));

                wc.GivePane(200);
            }
        }
    }
}