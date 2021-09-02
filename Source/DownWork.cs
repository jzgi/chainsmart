using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Zhnt.Supply
{
    [UserAuthorize(admly: User.ADMLY_SAL)]
    [Ui("销售产品管理")]
    public class AdmlyDownWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyDownVarWork>();
        }

        public void @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Down.Empty).T(" FROM offers ORDER BY typ, status DESC LIMIT 40 OFFSET 40 * @1");
            var arr = dc.Query<Down>(p => p.Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.typ != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(Item.Typs[o.typ])._TD()._TR();
                    }
                    h.TR_();
                    h.TD(Item.Statuses[o.status]);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
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
                var o = new Item();
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("标品属性");
                    h.LI_().SELECT("类别", nameof(o.typ), o.typ, Item.Typs)._LI();
                    h.LI_().TEXT("标品名称", nameof(o.name), o.name, max: 10, required: true)._LI();
                    h.LI_().TEXT("亮点", nameof(o.tip), o.tip, max: 10)._LI();
                    // h.LI_().SELECT("方案关联", nameof(o.unit), o.unit, Item.Progg)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, Item.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Item>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO items ").colset(Item.Empty, 0)._VALUES_(Item.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));
                wc.GivePane(200); // close dialog
            }
        }
    }
}