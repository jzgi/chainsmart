using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital.Site
{
    public class PublyItemVarWork : ItemVarWork
    {
        public void @default(WebContext wc)
        {
            short itemid = wc[0];
            var item = GrabObject<short, Item>(itemid);
            wc.GivePane(200, h =>
            {
                h.UL_("uk-card uk-card-default uk-card-body");

                h._UL();
            });
        }
    }

    public class AdmlyItemVarWork : ItemVarWork
    {
        public async Task @default(WebContext wc)
        {
            short id = wc[0];
            var prin = (User) wc.Principal;
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE id = @1");
                var m = dc.QueryTop<Item>(p => p.Set(id));
                wc.GivePane(200, h =>
                {
                    var typname = Item.Typs[m.typ];
                    h.FORM_().FIELDSUL_(typname + "品目信息");
                    h.LI_().SELECT("原类型", nameof(m.typ), m.typ, Item.Typs, required: true)._LI();
                    h.LI_().TEXT("品目名", nameof(m.name), m.name, max: 10, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().TEXT("基本单位", nameof(m.unit), m.unit, min: 1, max: 4, required: true).TEXT("单位脚注", nameof(m.unitip), m.unitip, max: 8)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Info.Statuses, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                const short proj = Info.TYP;
                var m = await wc.ReadObjectAsync(proj, new Item
                {
                    adapted = DateTime.Now,
                    adapter = prin.name
                });
                using var dc = NewDbContext();
                dc.Sql("UPDATE items")._SET_(Item.Empty, proj).T(" WHERE id = @1");
                dc.Execute(p =>
                {
                    m.Write(p, proj);
                    p.Set(id);
                });
                wc.GivePane(200); // close
            }
        }

        [Ui("◑", "图标"), Tool(ButtonCrop, Appear.Small)]
        public async Task icon(WebContext wc)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT icon FROM items WHERE id = @1");
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
                dc.Sql("UPDATE items SET icon = @1 WHERE id = @2");
                if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else wc.Give(500); // internal server error
            }
        }
    }
}