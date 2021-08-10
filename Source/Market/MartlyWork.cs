using SkyChain.Web;

namespace Zhnt.Market
{
    public class MartlyWork : WebWork, IOrglyVar
    {
        protected override void OnMake()
        {
            MakeVarWork<MartlyVarWork>(prin => ((User) prin).orgid);
        }
    }
}