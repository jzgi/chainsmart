using SkyChain.Web;

namespace Zhnt.Supply
{
    public class BizlyWork : WebWork, IOrglyVar
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyVarWork>(prin => ((User) prin).orgid);
        }
    }
}