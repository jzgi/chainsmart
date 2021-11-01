using SkyChain.Web;

namespace Revital.Mart
{
    public class PublyBizWork : WebWork, IOrglyVar
    {
        protected override void OnMake()
        {
            MakeVarWork<PublyBizVarWork>();
        }
    }
}