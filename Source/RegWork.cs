using System.Threading.Tasks;
using SkyChain.Web;
using static Revital.User;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class RegWork : WebWork
    {
    }

    [UserAuthorize(admly: ADMLY_MGT)]
    [Ui("［平台］地区场区设置")]
    public class AdmlyRegWork : RegWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyRegVarWork>();
        }

        [Ui("地区", group: 1), Tool(Anchor)]
        public void @default(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: 1);
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE typ = 1 ORDER BY id, status DESC");
                var arr = dc.Query<Reg>();
                h.TABLE(arr,
                    o =>
                    {
                        h.TDCHECK(o.Key);
                        h.TD_();
                        h.T(o.name);
                        if (o.typ == Reg.TYP_DIST)
                        {
                            h.T('（').T(Reg.Typs[o.typ]).T('）');
                        }
                        h._TD();
                        h.TD(_Info.Statuses[o.status]);
                        h.TDFORM(() => h.VARTOOLS(o.Key));
                    }
                );
            });
        }

        [Ui("场区", group: 2), Tool(Anchor)]
        public void section(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: 2);
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE typ = 2 ORDER BY id, status DESC");
                var arr = dc.Query<Reg>();
                h.TABLE(arr,
                    o =>
                    {
                        h.TDCHECK(o.Key);
                        h.TD_();
                        h.T(o.name);
                        if (o.typ == Reg.TYP_DIST)
                        {
                            h.T('（').T(Reg.Typs[o.typ]).T('）');
                        }
                        h._TD();
                        h.TD(_Info.Statuses[o.status]);
                        h.TDFORM(() => h.VARTOOLS(o.Key));
                    }
                );
            });
        }

        [Ui("✚", "新建", group: 3), Tool(ButtonShow)]
        public async Task @new(WebContext wc, int typ)
        {
            if (wc.IsGet)
            {
                var o = new Reg
                {
                    status = _Info.STA_ENABLED
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("区域属性");
                    h.LI_().NUMBER("区域编号", nameof(o.id), o.id, min: 1, max: 99, required: true)._LI();
                    h.LI_().SELECT("类型", nameof(o.typ), o.typ, Reg.Typs, filter: (k, v) => k == typ, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                    h.LI_().NUMBER("排序", nameof(o.idx), o.idx, min: 1, max: 99)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Info.Statuses)._LI();
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