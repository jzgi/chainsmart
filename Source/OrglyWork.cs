using SkyChain.Web;

namespace Revital
{
    public class OrglyWork : WebWork
    {
    }

    public class BizlyWork : OrglyWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyVarWork>(prin => ((User) prin).orgid);
        }
    }

    public class CtrlyWork : OrglyWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyVarWork>(prin => ((User) prin).orgid);
        }
    }

    public class SrclyWork : OrglyWork
    {
        protected override void OnMake()
        {
            MakeVarWork<SrclyVarWork>(prin => ((User) prin).orgid);
        }
    }
}