using SkyChain.Web;

namespace Revital.Mart
{
    public class BizlyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyVarWork>(prin => ((User) prin).orgid);
        }
    }
}