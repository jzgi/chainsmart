using System.Threading.Tasks;
using SkyChain.Web;

namespace Zhnt
{
    [UserAuthorize(orgly: 1, typ: Org.TYP_BIZGRP)]
    [Ui("团绩效")]
    public class BizGrplyKpiWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MrtlyBizVarWork>();
        }

        public async Task @default(WebContext wc)
        {
        }
    }

    [UserAuthorize(orgly: 1, typ: Org.TYP_SRCGRP)]
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