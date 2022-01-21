using System;
using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    public abstract class RouteWork : WebWork
    {
    }

    [Ui("中转｜配送市场关联")]
    public class CtrlyRouteWork : RouteWork
    {
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Route.Empty).T(" FROM links WHERE typ = ").T(Route.TYP_TOMRT).T(" AND ctrid = @1 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Route>(p => p.Set(org.id), 0xff);
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
                var o = new Route
                {
                    typ = Route.TYP_TOCTR,
                    status = _Info.STA_ENABLED
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("关联信息");
                    h.LI_().SELECT_ORG("市场", nameof(o.ptid), o.ptid, orgs, regs, filter: x => x.IsMrt, required: true)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Info.Statuses, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(0, new Route
                {
                    typ = Route.TYP_TOMRT,
                    created = DateTime.Now,
                    creator = prin.name,
                    ctrid = org.id
                });
                o.name = orgs[o.ptid].name;

                using var dc = NewDbContext();
                dc.Sql("INSERT INTO links ").colset(Route.Empty, 0)._VALUES_(Route.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }

    [Ui("供应｜控配中心关联")]
    public class PrvlyRouteWork : RouteWork
    {
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Route.Empty).T(" FROM routes WHERE typ = ").T(Route.TYP_TOCTR).T(" AND ptid = @1 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Route>(p => p.Set(org.id), 0xff);
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

        [Ui("✚", "添加中转"), Tool(Modal.ButtonShow)]
        public async Task @new(WebContext wc, int typ)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var orgs = Grab<int, Org>();
            var regs = Grab<short, Reg>();
            if (wc.IsGet)
            {
                var o = new Route
                {
                    typ = Route.TYP_TOCTR,
                    status = _Info.STA_ENABLED
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("关联信息");
                    h.LI_().SELECT_ORG("中转站", nameof(o.ctrid), o.ctrid, orgs, regs, filter: x => x.IsCtr, required: true)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Info.Statuses, filter: (k, v) => k > 0, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(0, new Route
                {
                    typ = Route.TYP_TOCTR,
                    created = DateTime.Now,
                    creator = prin.name,
                    ptid = org.id
                });
                o.name = orgs[o.ctrid].name;

                using var dc = NewDbContext();
                dc.Sql("INSERT INTO links ").colset(Route.Empty, 0)._VALUES_(Route.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }
}