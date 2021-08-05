using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Zhnt.Mart
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
            else wc.Give(404, shared: true, maxage: 3600 * 24); // not found
        }
    }

    public class PubItemVarWork : ItemVarWork
    {
        public void @default(WebContext wc)
        {
            short itemid = wc[0];
            var items = Fetch<Map<short, Item>>();
            var mats = Fetch<Map<short, Mat>>();
            var item = items[itemid];
            wc.GivePane(200, h =>
            {
                h.UL_("uk-card uk-card-default uk-card-body");
                for (int i = 0; i < item.ingrs.Length; i++)
                {
                    var ing = item.ingrs[i];
                    h.LI_();
                    h.T(mats[ing.id].name);
                    h._LI();
                }
                h._UL();
            });
        }
    }

    public class AdmlyItemVarWork : ItemVarWork
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
            var mats = Fetch<Map<short, Mat>>();
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.QueryTop("SELECT ingrs FROM items WHERE id = @1", p => p.Set(id));
                dc.Let(out Ingr[] ingrs);
                wc.GivePane(200, h =>
                {
                    h.TABLE(ingrs, o =>
                    {
                        var mat = mats[o.id];
                        h.TD_("uk-width-1-2").T(mat.name)._TD();
                        h.TD2(o.qty, mat.unit);
                        h.TDFORM(() =>
                        {
                            h.HIDDEN(nameof(o.id), o.id);
                            h.TOOL(nameof(cnt), 2, caption: "✕", css: "uk-button-secondary", tool: ToolAttribute.BUTTON_PICK_CONFIRM);
                        });
                    });

                    var ing = new Ingr();
                    h.FORM_().FIELDSUL_();
                    h.LI_().SELECT("食材", nameof(ing.id), ing.id, mats).NUMBER("克数", nameof(ing.qty), ing.qty)._LI();
                    h.LI_("uk-flex-center").BUTTON("添加", nameof(cnt), 1)._LI();
                    h._FIELDSUL();
                    h._FORM();
                });
            }
            else // POST
            {
                using var dc = NewDbContext();
                dc.QueryTop("SELECT ingrs FROM items WHERE id = @1", p => p.Set(id));
                dc.Let(out Ingr[] arr);
                if (cmd == 1) // new
                {
                    var o = await wc.ReadObjectAsync<Ingr>();
                    int idx = arr.IndexOf(x => x.id == o.id);
                    if (idx >= 0) // included already
                    {
                        arr[idx].qty = o.qty;
                    }
                    else
                    {
                        arr = arr.AddOf(o);
                    }
                }
                else if (cmd == 2) // remove
                {
                    short matid = (await wc.ReadAsync<Form>())[nameof(matid)];
                    arr = arr.RemovedOf(x => x.id == matid);
                }
                dc.Sql("UPDATE items SET ingrs = @1 WHERE id = @2");
                dc.Execute(p => p.Set(arr).Set(id));

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