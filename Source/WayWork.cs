using System;
using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    public abstract class WayWork : WebWork
    {
    }

    [Ui("控配｜配送市场关联")]
    public class CtrlyWayWork : WayWork
    {
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Way.Empty).T(" FROM ways WHERE typ = ").T(Way.TYP_NORM).T(" AND ctrid = @1 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Way>(p => p.Set(org.id), 0xff);
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

        [Ui("✚", "添加市场"), Tool(Modal.ButtonShow)]
        public async Task @new(WebContext wc, int typ)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var orgs = Grab<int, Org>();
            var regs = Grab<short, Reg>();
            if (wc.IsGet)
            {
                var o = new Way
                {
                    typ = Way.TYP_SPECIAL,
                    status = _Info.STA_ENABLED
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("关联信息");
                    h.LI_().SELECT_ORG("市场", nameof(o.mrtid), o.mrtid, orgs, regs, filter: x => x.IsMrt, required: true)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Info.Statuses, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(0, new Way
                {
                    typ = Way.TYP_NORM,
                    created = DateTime.Now,
                    creator = prin.name,
                    ctrid = org.id
                });
                o.name = orgs[o.mrtid].name;

                using var dc = NewDbContext();
                dc.Sql("INSERT INTO links ").colset(Way.Empty, 0)._VALUES_(Way.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }
}