using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    /// <summary>
    /// The home page for markets and businesses therein..
    /// </summary>
    public class RevitalVarWork : WebWork
    {
        public async Task @default(WebContext wc, int cur)
        {
            int orgid = wc[0];
            var org = GrabObject<int, Org>(orgid);
            var regs = Grab<short, Reg>();
            if (org.IsMrt)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE sprid = @1 AND regid = @2 AND status > 0 ORDER BY addr");
                var bizs = await dc.QueryAsync<Org>(p => p.Set(orgid).Set(cur));
                wc.GivePage(200, h =>
                {
                    h.TOPBAR_().SUBNAV(regs, string.Empty, cur, filter: (k, v) => v.typ == Reg.TYP_SECT);
                    h.T("<button class=\"uk-icon-button uk-circle uk-margin-left-auto\" formaction=\"search\" onclick=\"return dialog(this,8,false,4,'&#x1f6d2; 按厨坊下单')\"><span uk-icon=\"search\"></span></button>");
                    h._TOPBAR();
                    if (bizs != null)
                    {
                        h.GRID(bizs, o =>
                        {
                            h.HEADER_("uk-card-header").T(o.name)._HEADER();
                            h.A_("/", o.id, "/", end: true).T(o.name)._A();
                        }, width: 2);
                    }
                }, title: org.name);
            }
            else if (org.IsBiz)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Piece.Empty).T(" FROM pieces WHERE orgid = @1 AND status > 0 ORDER BY status DESC");
                var posts = await dc.QueryAsync<Piece>(p => p.Set(org.id));
                wc.GivePage(200, h =>
                {
                    h.TOPBAR_();
                    h.T(org.name);
                    h._TOPBAR();

                    h.GRID(posts, o =>
                    {
                        h.HEADER_().T(o.name)._HEADER();
                        h.A_("/piece/", o.id, "/", end: true).T(o.name)._A();
                    });
                }, title: org.name);
            }
        }

        public async Task search(WebContext wc, int cur)
        {
        }
    }
}