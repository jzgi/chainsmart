using SkyChain.Web;

namespace Zhnt
{
    public class BizlyWork : WebWork, IOrglyVar
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyVarWork>(prin => ((User) prin).orgid);
        }
    }
}