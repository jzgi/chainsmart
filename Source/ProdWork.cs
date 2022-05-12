using System;
using System.Threading.Tasks;
using Chainly.Web;
using static Chainly.Web.Modal;
using static Chainly.Nodal.Store;

namespace Revital
{
    public abstract class ProdWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_SRC, User.ORGLY_OPN)]
    [Ui("产源货架设置", "thumbnails")]
    public class SrclyProdWork : ProdWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyProdVarWork>();
        }

        public void @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            var items = Grab<short, Item>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Prod.Empty).T(" FROM prods WHERE orgid = @1 ORDER BY status DESC");
            var arr = dc.Query<Prod>(p => p.Set(org.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: "产品和货架");

                if (arr == null) return;

                h.GRID(arr, o =>
                {
                    var item = items[o.itemid];

                    h.HEADER_("uk-card-header").PIC("/item/icon", circle: true).AVAR(o.Key, o.name)._HEADER();
                    h.SECTION_("uk-card-body");
                    h.SPAN_().CNY(o.price).T(" 每").T(o.unit).T('（').T(o.unitx).T(item.unit).T('）')._SPAN();
                    h._SECTION();
                    h.FOOTER_("uk-card-footer").TOOLGROUPVAR(o.Key)._FOOTER();
                });

                // h.TABLE(arr, o =>
                // {
                //     h.TDAVAR(o.Key, o.name);
                //     h.TD_("uk-visible@l").T(o.tip)._TD();
                //     h.TD_().CNY(o.price, true).T("／").T(o.unit)._TD();
                //     h.TD(Info.Statuses[o.status]);
                //     h.TDFORM(() => h.TOOLGROUPVAR(o.Key));
                // });

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
                var o = new Prod
                {
                    unitx = 1,
                    min = 1, max = 1, step = 1, cap = 5000,
                    status = Info.STA_DISABLED,
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("品目名", nameof(o.itemid), o.itemid, items, Item.Typs, required: true).TEXT("附加名", nameof(o.ext), o.ext, max: 10)._LI();
                    h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().SELECT("贮藏方法", nameof(o.store), o.store, Prod.Stores, required: true).SELECT("贮藏天数", nameof(o.duration), o.duration, Prod.Durations, required: true)._LI();
                    h.LI_().CHECKBOX("只供给代理", nameof(o.toagt), o.toagt).SELECT("状态", nameof(o.status), o.status, Info.Statuses, filter: (k, v) => k > 0, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("规格参数");

                    h.LI_().TEXT("销售单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("单位倍比", nameof(o.unitx), o.unitx, min: 1, max: 1000, required: true)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step).NUMBER("总存量", nameof(o.cap), o.cap)._LI();

                    h._FIELDSUL();
                    h._FORM();
                });
            }
            else // POST
            {
                const short msk = Info.BORN;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Prod
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
                dc.Sql("INSERT INTO prods ").colset(Prod.Empty, msk)._VALUES_(Prod.Empty, msk);
                await dc.ExecuteAsync(p => m.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }
    }
}