using System.Threading.Tasks;
using ChainFX;
using ChainFX.Nodal;
using ChainFX.Web;
using static ChainFX.Nodal.Nodality;

namespace ChainSMart
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