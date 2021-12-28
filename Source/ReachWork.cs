using System;
using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    public class ReachWork : WebWork
    {
    }

    [Ui("［中心］市场关联")]
    public class CtrlyReachWork : ReachWork
    {
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Reach.Empty).T(" FROM reachs WHERE ctrid = @1 ORDER BY typ");
            var arr = await dc.QueryAsync<Reach>(p => p.Set(org.id), 0xff);
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr != null)
                {
                    h.TABLE(arr, x => { h.TD(x.name); });
                }
                h.PAGINATION(arr?.Length == 30);
            }, false, 3);
        }

        [Ui("✚", "添加投放地"), Tool(Modal.ButtonShow)]
        public async Task @new(WebContext wc, int typ)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            if (wc.IsGet)
            {
                var o = new Reach
                {
                    status = _Info.STA_ENABLED
                };
                var orgs = ObtainMap<int, Org>();
                var regs = ObtainMap<short, Reg>();
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("投放信息");
                    h.LI_().SELECT("类型", nameof(o.typ), o.typ, Reach.Typs, filter: (k, v) => k == typ, required: true)._LI();
                    h.LI_().SELECT_MRT("市场", nameof(o.mrtid), o.mrtid, orgs, regs, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, max: 10, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 30)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Info.Statuses, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(0, new Reach
                {
                    created = DateTime.Now,
                    creator = prin.name
                });
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO items ").colset(Reach.Empty, 0)._VALUES_(Reach.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }
}