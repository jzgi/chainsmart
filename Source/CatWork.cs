using System.Threading.Tasks;
using SkyChain.Web;
using static Revital.User;
using static SkyChain.Web.Modal;

namespace Revital
{
    [UserAuthorize(admly: ADMLY_SYS)]
    [Ui("商品分类")]
    public class AdmlyCatWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyCatVarWork>();
        }

        public void @default(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Cat.Empty).T(" FROM cats ORDER BY id, status DESC");
                var arr = dc.Query<Cat>();
                h.TABLE(arr,
                    o =>
                    {
                        h.TDCHECK(o.Key);
                        h.TD_().T(o.name);
                        if (o.typ == Cat.TYP_PROVINCE)
                        {
                            h.T('（').T(Cat.Typs[o.typ]).T('）');
                        }
                        h._TD();
                        h.TD(_Bean.Statuses[o.status]);
                        h.TDFORM(() => h.VARTOOLS(o.Key));
                    }
                );
            });
        }

        [Ui("新建"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            if (wc.IsGet)
            {
                var o = new Cat
                {
                    status = _Bean.STA_WORKABLE
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("地区属性");
                    h.LI_().TEXT("编号", nameof(o.id), o.id, min: 1, max: 99, required: true)._LI();
                    h.LI_().SELECT("类型", nameof(o.typ), o.typ, Cat.Typs)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                    h.LI_().NUMBER("排序", nameof(o.idx), o.idx, min: 1, max: 99)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Bean.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Cat>();
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO regs ").colset(Cat.Empty)._VALUES_(Cat.Empty);
                await dc.ExecuteAsync(p => o.Write(p));

                wc.GivePane(200); // close dialog
            }
        }
    }
}