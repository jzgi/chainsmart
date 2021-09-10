using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Zhnt.Supply
{
    public abstract class ItemVarWork : WebWork
    {
        const int PIC_MAX_AGE = 3600 * 24;

        public void icon(WebContext wc, int forced = 0)
        {
            short id = wc[this];
            using var dc = NewDbContext();
            if (dc.QueryTop("SELECT icon FROM items WHERE id = @1", p => p.Set(id)))
            {
                dc.Let(out byte[] bytes);
                if (bytes == null) wc.Give(204); // no content 
                else wc.Give(200, new StaticContent(bytes), shared: (forced == 0) ? true : (bool?) null, PIC_MAX_AGE);
            }
            else
                wc.Give(404, shared: true, maxage: 3600 * 24); // not found
        }
    }

    public class PubItemVarWork : ItemVarWork
    {
        public void @default(WebContext wc)
        {
            short itemid = wc[0];
            var item = Obtain<short, Item>(itemid);
            wc.GivePane(200, h =>
            {
                h.UL_("uk-card uk-card-default uk-card-body");

                h._UL();
            });
        }
    }

    public class AdmlyItemVarWork : ItemVarWork
    {
        [Ui("修改", group: 2), Tool(AnchorShow)]
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
                    h.FORM_().FIELDSUL_("品类信息");
                    h.LI_().SELECT("大类", nameof(o.typ), o.typ, Item.Typs)._LI();
                    h.LI_().TEXT("品类名称", nameof(o.name), o.name, max: 10, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 10)._LI();
                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true)._LI();
                    h.LI_().TEXT("单位脚注", nameof(o.unitip), o.unitip, max: 8)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Art.Statuses, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(0, new Item
                {
                });
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

        [Ui("◐"), Tool(ButtonCrop, Appear.Small)]
        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon));
        }

        [Ui("◪"), Tool(ButtonCrop, Appear.Large)]
        public async Task img(WebContext wc)
        {
            await doimg(wc, nameof(img));
        }

        public async Task doimg(WebContext wc, string col)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").T(col).T(" FROM items WHERE id = @1");
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
                dc.Sql("UPDATE items SET ").T(col).T(" = @1 WHERE id = @2");
                if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else wc.Give(500); // internal server error
            }
        }


    }
}