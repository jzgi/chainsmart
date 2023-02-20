using System.Threading.Tasks;
using ChainFx;
using ChainFx.Nodal;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart
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