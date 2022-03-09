using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class ProductWork : WebWork
    {
    }


    [UserAuthorize(Org.TYP_FRM, User.ORGLY_OPN)]
    [Ui("大户线上货架设置", "thumbnails")]
    public class FrmlyProductWork : ProductWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyProductVarWork>();
        }

        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Product.Empty).T(" FROM products WHERE orgid = @1 ORDER BY status DESC, typ LIMIT 40 OFFSET 40 * @2");
            var arr = dc.Query<Product>(p => p.Set(org.id).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null) return;

                h.TABLE(arr, o =>
                {
                    h.TDAVAR(o.Key, o.name);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TD_().CNY(o.price, true).T("／").T(o.unit)._TD();
                    h.TD(Info.Statuses[o.status]);
                    h.TDFORM(() => h.TOOLGROUPVAR(o.Key));
                });

                h.PAGINATION(arr.Length == 40);
            });
        }


        [Ui("✚", "新建产品"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var items = Grab<short, Item>();
            if (wc.IsGet)
            {
                var tomorrow = DateTime.Today.AddDays(1);
                var o = new Product
                {
                    fillon = tomorrow,
                    status = Info.STA_DISABLED,
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("品目名", nameof(o.itemid), o.itemid, items, Item.Typs, required: true).TEXT("附加名", nameof(o.ext), o.ext, max: 10)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().SELECT("发货约定", nameof(o.fillg), o.fillg, Product.Fillgs, required: true).DATE("指定日期", nameof(o.fillon), o.fillon, min: tomorrow)._LI();
                    h.LI_().SELECT("商户订货", nameof(o.bookg), o.bookg, Product.Bookgs, required: true).SELECT("状态", nameof(o.status), o.status, Info.Statuses, filter: (k, v) => k > 0, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("规格参数");

                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("标准比", nameof(o.unitx), o.unitx, min: 1, max: 1000, required: true)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step).NUMBER("现存量", nameof(o.cap), o.cap)._LI();
                    h.LI_().SELECT("市场约束", nameof(o.mrtg), o.mrtg, Product.Mrtgs, required: true).NUMBER("市场价", nameof(o.mrtprice), o.mrtprice, min: 0.00M, max: 10000.00M)._LI();

                    h._FIELDSUL();
                    h._FORM();
                });
            }
            else // POST
            {
                const short proj = Info.BORN;
                // populate 
                var m = await wc.ReadObjectAsync(proj, new Product
                {
                    orgid = org.id,
                    created = DateTime.Now,
                    creator = prin.name,
                });
                var item = items[m.itemid];
                m.typ = item.typ;
                m.name = item.name + '（' + m.ext + '）';

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO products ").colset(Product.Empty, proj)._VALUES_(Product.Empty, proj);
                await dc.ExecuteAsync(p => m.Write(p, proj));

                wc.GivePane(200); // close dialog
            }
        }
    }
}