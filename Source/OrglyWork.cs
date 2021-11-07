using SkyChain.Web;

namespace Revital
{
    public abstract class OrglyWork : WebWork
    {
    }

    public class MrtlyWork : OrglyWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MrtlyVarWork>(prin => ((User) prin).orgid);
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