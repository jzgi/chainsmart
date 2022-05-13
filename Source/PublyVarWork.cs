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

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE sprid = @1 AND status > 0 ORDER BY addr");
            var bizs = await dc.QueryAsync<Org>(p => p.Set(mrtid));
            wc.GivePage(200, h =>
            {
                h.GRID(bizs, o =>
                {
                    h.SECTION_("uk-flex uk-card-body");
                    h.SPAN(o.ShopLabel, "uk-circle-label").SP();
                    h.A_("/biz/", o.id, "/", css: "uk-button-link").T(o.Shop).T(o.name)._A();
                    h._SECTION();
                }, min: 2);
            }, title: mrt.name);
        }
    }

    public class PublyBizVarWork : PublyVarWork
    {
        public async Task @default(WebContext wc)
        {
            int bizid = wc[0];
            var biz = GrabObject<int, Org>(bizid);
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purch.Empty).T(" FROM purchs WHERE bizid = @1 AND status > 0 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Purch>(p => p.Set(biz.id));
            wc.GivePage(200, h =>
            {
                h.FORM_().FIELDSUL_();
                foreach (var o in arr)
                {
                    h.LI_("uk-card uk-card-default");
                    h.HEADER_("uk-card-header").T(o.name)._HEADER();
                    h.DIV_();
                    h.SELECT(null, nameof(o.name), 1, new int[] {1, 2, 3});
                    h._DIV();
                    h._LI();
                }
                h._FIELDSUL();
                h.BOTTOMBAR_().BUTTON("付款")._BOTTOMBAR();
                h._FORM();
            }, title: biz.name);
        }
    }
}