using System.Threading.Tasks;
using SkyChain.Web;

namespace Zhnt.Supply
{
    public class AdmlyClearWork : WebWork
    {
        protected override void OnMake()
        {
        }

        public async Task @default(WebContext wc)
        {
        }
    }

    [UserAuthorize(orgtyp: Org.TYP_BIZGRP, orgly: 1)]
    [Ui("image","团绩效")]
    public class BizGrplyKpiWork : WebWork
    {
        protected override void OnMake()
        {
        }

        public async Task @default(WebContext wc)
        {
        }
    }

    [UserAuthorize(orgtyp: Org.TYP_SRCGRP, orgly: 1)]
    [Ui("团绩效")]
    public class SrcGrplyKpiWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MrtlyBizVarWork>();
        }

        public async Task @default(WebContext wc)
        {
        }
    }
}