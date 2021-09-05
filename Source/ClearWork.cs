using System.Threading.Tasks;
using SkyChain.Web;

namespace Zhnt.Supply
{
    [UserAuthorize(admly: 1)]
    [Ui("交易清算")]
    public class AdmlyClearWork : WebWork
    {
        protected override void OnMake()
        {
        }

        public async Task @default(WebContext wc)
        {
        }
    }

    [UserAuthorize(orgtyp: Org.TYP_BIZ_CO, orgly: 1)]
    [Ui("image", "团绩效")]
    public class BizGrplyKpiWork : WebWork
    {
        protected override void OnMake()
        {
        }

        public async Task @default(WebContext wc)
        {
        }
    }

    [UserAuthorize(orgtyp: Org.TYP_SRC_CO, orgly: 1)]
    [Ui("团绩效")]
    public class SrcGrplyKpiWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizColyOrgVarWork>();
        }

        public async Task @default(WebContext wc)
        {
        }
    }
}