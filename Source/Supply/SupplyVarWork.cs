using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital.Supply
{
    public class CtrlySupplyVarWork : WebWork
    {
        [Ui("✎", "✎ 修改", @group: 2), Tool(AnchorShow)]
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
                    // h.LI_().SELECT("方案关联", nameof(o.unit), o.unit, Item.Progg)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, Supply.Statuses)._LI();
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

        [Ui("图片"), Tool(ButtonCrop, Appear.Small)]
        public async Task img(WebContext wc)
        {
            await doimg(wc, nameof(img));
        }

        [Ui("图片"), Tool(ButtonCrop, Appear.Small)]
        public async Task testa(WebContext wc)
        {
            await doimg(wc, nameof(testa));
        }

        [Ui("图片"), Tool(ButtonCrop, Appear.Small)]
        public async Task testb(WebContext wc)
        {
            await doimg(wc, nameof(testb));
        }

        public async Task doimg(WebContext wc, string col)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").T(col).T(" FROM prods WHERE id = @1");
                if (dc.QueryTop(p => p.Set(id)))
                {
                    dc.Let(out byte[] bytes);
                    if (bytes == null) wc.Give(204); // no content 
                    else wc.Give(200, new StaticContent(bytes), shared: false, 60);
                }
                else
                    wc.Give(404, shared: true, maxage: 3600 * 24); // not found
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ArraySegment<byte> img = f[nameof(img)];
                using var dc = NewDbContext();
                dc.Sql("UPDATE prods SET ").T(col).T(" = @1 WHERE id = @2");
                if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else wc.Give(500); // internal server error
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