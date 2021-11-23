using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class PlanVarWork : WebWork
    {
    }


    public class CtrlyPlanVarWork : PlanVarWork
    {
        [Ui("✎", "✎ 修改", group: 2), Tool(AnchorOpen)]
        public async Task upd(WebContext wc)
        {
            short id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;
            var items = ObtainMap<short, Item>();
            short finalg = wc.Query[nameof(finalg)];
            if (wc.IsGet)
            {
                Plan o;
                if (finalg == 0)
                {
                    using var dc = NewDbContext();
                    dc.Sql("SELECT ").collst(Plan.Empty).T(" FROM plans WHERE id = @1");
                    o = dc.QueryTop<Plan>(p => p.Set(id));
                }
                else
                {
                    o = new Plan
                    {
                    };
                    o.Read(wc.Query);
                }
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("标准品目", nameof(o.itemid), o.itemid, items, Item.Cats, filter: x => x.typ == org.forkie, required: true)._LI();
                    h.LI_().TEXT("附加名", nameof(o.ext), o.ext, max: 10)._LI();
                    h.LI_().TEXTAREA("特色描述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().DATE("生效日", nameof(o.starton), o.starton)._LI();
                    h.LI_().DATE("截止日", nameof(o.endon), o.endon)._LI();
                    h.LI_().SELECT("交付模式", nameof(o.fillg), o.fillg, Plan.Fillgs)._LI();
                    h.LI_().DATE("远期交付", nameof(o.fillon), o.fillon)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Article.Statuses, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("规格参数");
                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, max: 10, required: true).NUMBER("标准比", nameof(o.unitx), o.unitx, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step)._LI();
                    h.LI_().NUMBER("价格", nameof(o.price), o.price, min: 0.00M, max: 10000.00M).NUMBER("优惠", nameof(o.off), o.off)._LI();
                    h._FIELDSUL();

                    h._FIELDSUL().FIELDSUL_("末端规格");
                    h.LI_().SELECT("设置", nameof(o.finalg), o.finalg, Plan.Finalgs, refresh: true)._LI();
                    if (finalg > 0)
                    {
                        h.LI_().TEXT("单位", nameof(o.funit), o.funit, max: 10, required: true).NUMBER("标准比", nameof(o.funitx), o.funitx, min: 1, max: 1000)._LI();
                        h.LI_().NUMBER("起订量", nameof(o.fmin), o.fmin).NUMBER("限订量", nameof(o.fmax), o.fmax, min: 1, max: 1000)._LI();
                        h.LI_().NUMBER("递增量", nameof(o.fstep), o.fstep)._LI();
                        h.LI_().NUMBER("价格", nameof(o.fprice), o.fprice, min: 0.00M, max: 10000.00M).NUMBER("优惠", nameof(o.foff), o.foff)._LI();
                    }
                    h._FIELDSUL();

                    h.BOTTOM_BUTTON("确定");

                    h._FORM();
                });
            }
            else // POST
            {
                // populate 
                var o = await wc.ReadObjectAsync(0, new Plan
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                    orgid = org.id,
                });
                var item = items[o.itemid];
                o.cat = item.cat;
                o.name = item.name + '（' + o.ext + '）';

                // insert
                using var dc = NewDbContext();
                dc.Sql("UPDATE plans ")._SET_(Plan.Empty, 0).T(" WHERE id = @1");
                await dc.ExecuteAsync(p =>
                {
                    o.Write(p, 0);
                    p.Set(id);
                });

                wc.GivePane(200); // close dialog
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