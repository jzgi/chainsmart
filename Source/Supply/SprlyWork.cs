using SkyChain.Web;

namespace Zhnt.Supply
{
    public class SprlyWork : WebWork, IOrglyVar
    {
        protected override void OnMake()
        {
            MakeVarWork<SprlyVarWork>(prin => ((User) prin).orgid);
        }
    }
}