using SkyChain.Web;

namespace Zhnt.Supply
{
    public class SrclyWork : WebWork, IOrglyVar
    {
        protected override void OnMake()
        {
            MakeVarWork<SrclyVarWork>(prin => ((User) prin).orgid);
        }
    }
}