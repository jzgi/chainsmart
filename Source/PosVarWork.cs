using System.Threading.Tasks;
using ChainFX.Web;

namespace ChainSMart
{
    public abstract class PosVarWork : WebWork
    {
    }

    public class ShplyPosVarWork : PosVarWork
    {
        public async Task @default(WebContext wc)
        {
        }
    }
}