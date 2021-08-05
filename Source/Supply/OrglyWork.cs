using SkyChain.Web;

namespace Zhnt.Supply
{
    public class OrglyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<OrglyVarWork>(prin => ((User) prin).orgid);
        }
    }
}