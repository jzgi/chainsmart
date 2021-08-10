using SkyChain.Web;

namespace Zhnt.Market
{
    public class PubMartWork : WebWork, IOrglyVar
    {
        protected override void OnMake()
        {
            MakeVarWork<PubMartVarWork>();
        }
    }
}