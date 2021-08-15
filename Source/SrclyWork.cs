using SkyChain.Web;

namespace Zhnt
{
    public class SrclyWork : WebWork, IOrglyVar
    {
        protected override void OnMake()
        {
            MakeVarWork<SrclyVarWork>(prin => ((User) prin).orgid);
        }
    }
}