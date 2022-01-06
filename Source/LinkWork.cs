using System;
using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    public class LinkWork : WebWork
    {
    }

    [Ui("［中心］市场关联")]
    public class CtrlyLinkWork : LinkWork
    {
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Link.Empty).T(" FROM links WHERE typ = ").T(Link.TYP_DOWN).T(" AND ctrid = @1 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Link>(p => p.Set(org.id), 0xff);
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

        [Ui("✚", "关联市场"), Tool(Modal.ButtonShow)]
        public async Task @new(WebContext wc, int typ)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var orgs = ObtainMap<int, Org>();
            var regs = ObtainMap<short, Reg>();
            if (wc.IsGet)
            {
                var o = new Link
                {
                    typ = Link.TYP_UP,
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
                var o = await wc.ReadObjectAsync(0, new Link
                {
                    typ = Link.TYP_DOWN,
                    created = DateTime.Now,
                    creator = prin.name,
                    ctrid = org.id
                });
                o.name = orgs[o.ptid].name;

                using var dc = NewDbContext();
                dc.Sql("INSERT INTO links ").colset(Link.Empty, 0)._VALUES_(Link.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }

    [Ui("供应－分拣中心关联")]
    public class PrvlyLinkWork : LinkWork
    {
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Link.Empty).T(" FROM links WHERE typ = ").T(Link.TYP_UP).T(" AND ptid = @1 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Link>(p => p.Set(org.id), 0xff);
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

        [Ui("✚", "关联分拣中心"), Tool(Modal.ButtonShow)]
        public async Task @new(WebContext wc, int typ)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var orgs = ObtainMap<int, Org>();
            var regs = ObtainMap<short, Reg>();
            if (wc.IsGet)
            {
                var o = new Link
                {
                    typ = Link.TYP_UP,
                    status = _Info.STA_ENABLED
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("关联信息");
                    h.LI_().SELECT_ORG("分拣中心", nameof(o.ctrid), o.ctrid, orgs, regs, filter: x => x.IsCtr, required: true)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Info.Statuses, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(0, new Link
                {
                    typ = Link.TYP_UP,
                    created = DateTime.Now,
                    creator = prin.name,
                    ptid = org.id
                });
                o.name = orgs[o.ctrid].name;

                using var dc = NewDbContext();
                dc.Sql("INSERT INTO links ").colset(Link.Empty, 0)._VALUES_(Link.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }
}