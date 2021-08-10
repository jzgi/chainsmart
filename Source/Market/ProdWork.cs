using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using Zhnt;
using static SkyChain.Web.Modal;

namespace Zhnt.Market
{
    [UserAuthorize(admly: User.ADMLY_PROD)]
    [Ui("货架")]
    public class AdmlyProdWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyProdVarWork>();
        }

        public void @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Prod.Empty).T(" FROM mats ORDER BY typ, status DESC, id LIMIT 40 OFFSET 40 * @1");
            var arr = dc.Query<Prod>(p => p.Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.typ != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 4).T(Prod.Typs[o.typ])._TD()._TR();
                    }
                    h.TR_();
                    h.TDCHECK(o.id);
                    h.TD_().VARTOOL(o.Key, nameof(AdmlyProdVarWork.upd), caption: o.name).SP().SUB(o.unit)._TD();
                    h.TD(_Art.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                    h._TR();
                    last = o.typ;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("新建"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            if (wc.IsGet)
            {
                var o = new Prod();
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("材料属性");
                    h.LI_().SELECT("材料类型", nameof(o.typ), o.typ, Prod.Typs)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, max: 10, required: true).TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 2, required: true)._LI();
                    h.LI_().NUMBER("热量", nameof(o.calory), o.calory, min: 0, required: true).NUMBER("碳水化合", nameof(o.carb), o.carb, min: 0, required: true)._LI();
                    h.LI_().NUMBER("脂肪", nameof(o.fat), o.fat, min: 0, required: true).NUMBER("蛋白质", nameof(o.protein), o.protein, min: 0, required: true)._LI();
                    h.LI_().NUMBER("糖", nameof(o.sugar), o.sugar, min: 0, required: true).NUMBER("维他命", nameof(o.vitamin), o.vitamin, min: 0, required: true)._LI();
                    h.LI_().NUMBER("无机盐", nameof(o.mineral), o.mineral, min: 0, required: true)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, Prod.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Prod>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO mats ").colset(Prod.Empty, 0)._VALUES_(Prod.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));
                wc.GivePane(200); // close dialog
            }
        }
    }

    [Ui("货架")]
    public class BizlyProdWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyProdVarWork>();
        }

        public void @default(WebContext wc, int page)
        {
        }
    }
}