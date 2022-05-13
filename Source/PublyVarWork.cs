using System.Threading.Tasks;
using Chainly.Web;
using static Chainly.Nodal.Store;

namespace Revital
{
    public class PublyVarWork : WebWork
    {
    }


    /// <summary>
    /// The market home page.
    /// </summary>
    public class PublyMrtVarWork : PublyVarWork
    {
        public async Task @default(WebContext wc)
        {
            int mrtid = wc[0];
            var mrt = GrabObject<int, Org>(mrtid);
            var regs = Grab<short, Reg>();

            if (mrt.IsMrt)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE sprid = @1 AND status > 0 ORDER BY addr");
                var bizs = await dc.QueryAsync<Org>(p => p.Set(mrtid));
                wc.GivePage(200, h =>
                {
                    // h.TOPBAR_().T(org.name);
                    // // h.ADIALOG_(nameof(search), "-", sect, 8, false, Appear.Full).ICON("search", "uk-icon-button")._A();
                    // h._TOPBAR();
                    h.GRID(bizs, o =>
                    {
                        h.SECTION_("uk-flex uk-card-body");
                        h.SPAN(o.ShopLabel, "uk-circle-label").SP();
                        h.ADIALOG_("/biz/", o.id, "/", 8, false, Appear.Large, css: "uk-button-link").T(o.Shop)._A();
                        h._SECTION();
                    }, min: 2);
                }, title: mrt.name);
            }
            else if (mrt.IsBiz)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM pieces WHERE orgid = @1 AND status > 0 ORDER BY status DESC");
                var posts = await dc.QueryAsync<Purch>(p => p.Set(mrt.id));
                wc.GivePage(200, h =>
                {
                    h.TOPBAR_();
                    h.T(mrt.name);
                    h._TOPBAR();

                    h.GRID(posts, o =>
                    {
                        h.HEADER_().T(o.name)._HEADER();
                        h.A_("/piece/", o.id, "/", end: true).T(o.name)._A();
                    });
                }, title: mrt.name);
            }
        }
    }

    public class PublyBizVarWork : PublyVarWork
    {
        public async Task @default(WebContext wc)
        {
            int bizid = wc[0];
            var biz = GrabObject<int, Org>(bizid);
            var regs = Grab<short, Reg>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE bizid = @1 AND status > 0 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Purch>(p => p.Set(biz.id));
            wc.GivePage(200, h =>
            {
                h.GRID(arr, o =>
                {
                    h.HEADER_().T(o.name)._HEADER();
                    h.A_("/piece/", o.id, "/", end: true).T(o.name)._A();
                });
                h.BOTTOMBAR_().BUTTON("")._BOTTOMBAR();
            }, title: biz.name);
        }
    }
}