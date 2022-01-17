using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class ProductWork : WebWork
    {
    }


    [UserAuthorize(Org.TYP_SRC, User.ORGLY_OP)]
    [Ui("产源｜产品管理")]
    public class SrclyProductWork : ProductWork
    {
        protected override void OnMake()
        {
            MakeVarWork<SrclyProductVarWork>();
        }

        [Ui("现货", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Product.Empty).T(" FROM products WHERE typ = ").T(Product.TYP_SPOT).T(" AND orgid = @1 AND status > 0 ORDER BY cat, status DESC LIMIT 40 OFFSET 40 * @2");
            var arr = dc.Query<Product>(p => p.Set(org.id).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null) return;

                h.TABLE(arr, o =>
                {
                    h.TD_().VARTOOL(o.Key, nameof(SrclyProductVarWork.upd), caption: o.name).SP()._TD();
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TD_().CNY(o.price, true).T("／").T(o.unit)._TD();
                    h.TD(_Info.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                });

                h.PAGINATION(arr.Length == 40);
            });
        }


        [Ui("预售", group: 2), Tool(Anchor)]
        public void pre(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Product.Empty).T(" FROM products WHERE typ = ").T(Product.TYP_FUTURE).T(" AND orgid = @1 AND status > 0 ORDER BY cat, status DESC LIMIT 40 OFFSET 40 * @2");
            var arr = dc.Query<Product>(p => p.Set(org.id).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null) return;

                h.TABLE(arr, o =>
                {
                    h.TD_().VARTOOL(o.Key, nameof(SrclyProductVarWork.upd), caption: o.name).SP()._TD();
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TD_().CNY(o.price, true).T("／").T(o.unit)._TD();
                    h.TD(_Info.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                });

                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("✚", "新建现货供应", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var items = Grab<short, Item>();
            if (wc.IsGet)
            {
                var o = new Product
                {
                    typ = Product.TYP_SPOT,
                    status = _Info.STA_DISABLED,
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("品目名", nameof(o.itemid), o.itemid, items, Item.Cats, filter: x => x.typ == org.fork, required: true).TEXT("附加名", nameof(o.ext), o.ext, max: 10)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().SELECT("供应对象", nameof(o.rank), o.rank, Org.Ranks, required: true).SELECT("状态", nameof(o.status), o.status, _Info.Statuses, filter: (k, v) => k > 0, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("规格参数");

                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("标准比", nameof(o.unitx), o.unitx, min: 1, max: 1000, required: true)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step).NUMBER("现存量", nameof(o.cap), o.cap)._LI();
                    h.LI_().SELECT("市场约束", nameof(o.postg), o.postg, Product.Postgs, required: true).NUMBER("市场价", nameof(o.postprice), o.postprice, min: 0.00M, max: 10000.00M)._LI();

                    h._FIELDSUL();

                    h.BOTTOM_BUTTON("确定");

                    h._FORM();
                });
            }
            else // POST
            {
                // populate 
                var o = await wc.ReadObjectAsync(0, new Product
                {
                    typ = Product.TYP_SPOT,
                    created = DateTime.Now,
                    creator = prin.name,
                    orgid = org.id,
                });
                var item = items[o.itemid];
                o.cat = item.cat;
                o.name = item.name + '（' + o.ext + '）';

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO products ").colset(Product.Empty, 0)._VALUES_(Product.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }

        [Ui("✚", "新建预售供应", group: 2), Tool(ButtonOpen)]
        public async Task newpre(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var items = Grab<short, Item>();
            if (wc.IsGet)
            {
                var o = new Product
                {
                    typ = Product.TYP_FUTURE,
                    status = _Info.STA_DISABLED,
                    created = DateTime.Now,
                    creator = prin.name,
                    orgid = org.id,
                    fillon = DateTime.Today
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("品目名", nameof(o.itemid), o.itemid, items, Item.Cats, filter: x => x.typ == org.fork, required: true).TEXT("附加名", nameof(o.ext), o.ext, max: 10)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().DATE("发货日期", nameof(o.fillon), o.fillon)._LI();
                    h.LI_().SELECT("供应对象", nameof(o.rank), o.rank, Org.Ranks, required: true).SELECT("状态", nameof(o.status), o.status, _Info.Statuses, filter: (k, v) => k > 0, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("规格参数");

                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("标准比", nameof(o.unitx), o.unitx, min: 1, max: 1000, required: true)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step).NUMBER("现存量", nameof(o.cap), o.cap)._LI();
                    h.LI_().SELECT("市场约束", nameof(o.postg), o.postg, Product.Postgs, required: true).NUMBER("市场价", nameof(o.postprice), o.postprice, min: 0.00M, max: 10000.00M)._LI();

                    h._FIELDSUL();

                    h.BOTTOM_BUTTON("确定");

                    h._FORM();
                });
            }
            else // POST
            {
                // populate 
                var o = await wc.ReadObjectAsync(0, new Product
                {
                    typ = Product.TYP_FUTURE,
                    created = DateTime.Now,
                    creator = prin.name,
                    orgid = org.id,
                });
                var item = items[o.itemid];
                o.cat = item.cat;
                o.name = item.name + '（' + o.ext + '）';

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO plans ").colset(Product.Empty, 0)._VALUES_(Product.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }
}