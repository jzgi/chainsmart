using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    /// <summary>
    /// The home page for mart.
    /// </summary>
    public class PublyVarWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PublyBizVarWork>();
        }

        public async Task @default(WebContext wc, int cur)
        {
            // mart id
            int mrtid = wc[0];
            var mrt = Obtain<int, Org>(mrtid);
            var regs = ObtainMap<short, Reg>();

            // get biz list under the mart
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE sprid = @1 AND regid = @2 AND status > 0 ORDER BY addr");
            var arr = await dc.QueryAsync<Org>(p => p.Set(mrtid).Set(cur));

            wc.GivePage(200, h =>
            {
                h.TOPBAR_().SUBNAV(regs, string.Empty, cur, filter: (k, v) => v.typ == Reg.TYP_INDOOR);
                h.T("<button class=\"uk-icon-button uk-circle uk-margin-left-auto\" formaction=\"search\" onclick=\"return dialog(this,8,false,4,'&#x1f6d2; 按厨坊下单')\"><span uk-icon=\"search\"></span></button>");
                h._TOPBAR();
                h.GRID(arr, o => { h.HEADER_("uk-card-header").T(o.name)._HEADER(); }, width: 2);
            }, title: mrt.name);
        }

        public async Task search(WebContext wc, int cur)
        {
        }
    }
}