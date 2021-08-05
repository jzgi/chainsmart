using SkyChain.Web;

namespace Zhnt.Mart
{
    public class BizlyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyVarWork>(prin => ((User) prin).bizid);
        }
    }
}