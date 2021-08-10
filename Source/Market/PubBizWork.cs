using SkyChain.Web;

namespace Zhnt.Market
{
    public class PubBizWork : WebWork, IOrglyVar
    {
        protected override void OnMake()
        {
            MakeVarWork<PubBizVarWork>();
        }
    }
}