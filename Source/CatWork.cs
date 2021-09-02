using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Zhnt.Supply
{
    [UserAuthorize(admly: User.ADMLY_PUR)]
    [Ui("商品类别")]
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
                        h.TD_("uk-text-right uk-width-small").T(o.id).T('（').T(o.sort).T('）')._TD();
                        h.TD_().T(o.name)._TD();
                        h.TD(_Art.Statuses[o.status]);
                        h.TDFORM(() => h.VARTOOLS(o.Key));
                    }
                );
            });
        }

        [UserAuthorize(admly: User.ADMLY_MGT)]
        [Ui("新建"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            if (wc.IsGet)
            {
                var o = new Cat();
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("属性");
                    h.LI_().NUMBER("编号", nameof(o.id), o.id, min: 1, max: 99, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, max: 10, required: true)._LI();
                    h.LI_().NUMBER("排序", nameof(o.sort), o.sort, min: 1, max: 9)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Art.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Cat>();
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO cats ").colset(Cat.Empty)._VALUES_(Cat.Empty);
                await dc.ExecuteAsync(p => o.Write(p));
                wc.GivePane(200); // close dialog
            }
        }
    }
}