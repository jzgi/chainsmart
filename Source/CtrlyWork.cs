using SkyChain.Web;

namespace Zhnt
{
    public class CtrlyWork : WebWork, IOrglyVar
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyVarWork>(prin => ((User) prin).orgid);
        }
    }
}