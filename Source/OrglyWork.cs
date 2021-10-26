using SkyChain.Web;

namespace Revital.Supply
{
    public class CtrlyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyVarWork>(prin => ((User_) prin).orgid);
        }
    }

    public class BizlyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyVarWork>(prin => ((User_) prin).orgid);
        }
    }

    public class SrclyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<SrclyVarWork>(prin => ((User_) prin).orgid);
        }
    }
}