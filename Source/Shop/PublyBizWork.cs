using SkyChain.Web;

namespace Revital.Shop
{
    public class PublyBizWork : WebWork, IOrglyVar
    {
        protected override void OnMake()
        {
            MakeVarWork<PublyMartBizVarWork>();
        }
    }
}