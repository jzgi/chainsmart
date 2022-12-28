using ChainFx;
using ChainFx.Web;

namespace ChainMart
{
    [UserAuthenticate]
    public class MyWork : WebWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<MyVarWork>((prin, key) => key?.ToInt() ?? ((User) prin).id);
        }
    }
}