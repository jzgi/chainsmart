using SkyChain.Web;

namespace Zhnt.Supply
{
    public class CtrlyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyVarWork>(prin => ((User) prin).orgid);
        }
    }

    public class BizlyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyVarWork>(prin => ((User) prin).orgid);
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