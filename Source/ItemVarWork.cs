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
            var item = FetchValue<short, Item>(itemid);
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
                    h.FORM_().FIELDSUL_("基本品信息");
                    h.LI_().SELECT("类别", nameof(o.typ), o.typ, Item.Typs, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, max: 10, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 10)._LI();
                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, tip: "基本单位", min: 1, max: 4, required: true)._LI();
                    h.LI_().TEXT("单位注释", nameof(o.unitip), o.unitip, max: 4)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Art.Statuses, required: true)._LI();
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

        [Ui("✿"), Tool(ButtonCrop, Appear.Small)]
        public async Task icon(WebContext wc)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                if (dc.QueryTop("SELECT icon FROM items WHERE id = @1", p => p.Set(id)))
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
                if (await dc.ExecuteAsync("UPDATE items SET icon = @1 WHERE id = @2", p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else
                    wc.Give(500); // internal server error
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