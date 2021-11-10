using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    public class PublyVarWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PublyVarVarWork>();
        }

        public async Task @default(WebContext wc, int cur)
        {
            // mart id
            int mrtid = wc[0];
            var mrt = Obtain<int, Org>(mrtid);
            var regs = ObtainMap<short, Reg>();


            // get biz list under the mart
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE sprid = @1 ANd status > 0 ORDER BY addr");
            var arr = await dc.QueryAsync<Org>(p => p.Set(mrtid));

            wc.GivePage(200, h =>
                {
                    h.SUBNAV(regs, string.Empty, cur, filter: (k, v) => v.typ == Reg.TYP_INDOOR);
                    h.DIV_()._DIV();
                },
                title: mrt.name
            );
        }
    }
}