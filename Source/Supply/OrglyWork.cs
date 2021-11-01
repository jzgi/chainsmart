using SkyChain.Web;

namespace Revital.Supply
{
    public class CtrlyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyVarWork>(prin => ((User) prin).orgid);
        }
    }

    public class SrclyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<SrclyVarWork>(prin => ((User) prin).orgid);
        }
    }
}