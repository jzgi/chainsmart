using System.Threading.Tasks;
using ChainFx.Web;

namespace ChainMart
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