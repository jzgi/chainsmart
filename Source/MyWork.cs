using SkyChain;
using SkyChain.Web;

namespace Revital
{
    public class MyWork : WebWork
    {
        protected override void OnMake()
        {
            // id of either current user or the specified
            MakeVarWork<MyVarWork>((prin, key) => key?.ToInt() ?? ((User) prin).id);
        }
    }
}