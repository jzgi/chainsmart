using SkyChain.Web;

namespace Revital
{
    public class OrglyWork<V> : WebWork where V : OrglyVarWork, new()
    {
        protected override void OnMake()
        {
            MakeVarWork<V>(prin => ((User) prin).orgid);
        }
    }
}