using System.Threading.Tasks;
using SkyChain.Web;
using static Rev.Supply.User_;
using static SkyChain.Web.Modal;

namespace Rev.Supply
{
    [UserAuthorize(admly: ADMLY_SUPLLY_MGT)]
    [Ui("地理区域")]
    public class AdmlyRegWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyRegVarWork>();
        }

        public void @default(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs ORDER BY id, status DESC");
                var arr = dc.Query<Reg>();
                h.TABLE(arr,
                    o =>
                    {
                        h.TDCHECK(o.Key);
                        h.TD_().T(o.name);
                        if (o.typ == Reg.TYP_METROPOLIS)
                        {
                            h.T('（').T(Reg.Typs[o.typ]).T('）');
                        }
                        h._TD();
                        h.TD(Art_.Statuses[o.status]);
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
                var o = new Reg
                {
                    status = Art_.STA_ENABLED
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("地区属性");
                    h.LI_().NUMBER("编号", nameof(o.id), o.id, min: 1, max: 99, required: true)._LI();
                    h.LI_().SELECT("类型", nameof(o.typ), o.typ, Reg.Typs)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                    h.LI_().NUMBER("排序", nameof(o.idx), o.idx, min: 1, max: 99)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, Art_.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Reg>();
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO regs ").colset(Reg.Empty)._VALUES_(Item.Empty);
                await dc.ExecuteAsync(p => o.Write(p));

                wc.GivePane(200); // close dialog
            }
        }
    }
}