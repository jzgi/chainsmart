using SkyChain.Web;

namespace Rev.Supply
{
    public class MyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MyVarWork>(x => ((User_) x).id);
        }
    }
}