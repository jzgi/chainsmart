using System.Threading.Tasks;
using ChainFx.Web;

namespace ChainMart
{
    public abstract class PosWork : WebWork
    {
    }

    [Ui("零售智能终端", "商户")]
    public class ShplyPosWork : PosWork
    {
        public async Task @default(WebContext wc)
        {
        }
    }
}