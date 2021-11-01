using SkyChain.Web;

namespace Revital
{
    public class MyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MyVarWork>(x => ((User) x).id);
        }
    }
}