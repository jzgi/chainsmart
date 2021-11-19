using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    public abstract class PosWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_BIZ, User.ORGLY_OP)]
    public abstract class BizlyPosWork : PosWork
    {
        public async Task @default(WebContext wc)
        {
        }
    }

    [Ui("销售计价终端", "desktop", forkie: Item.TYP_AGRI)]
    public class AgriBizlyPosWork : BizlyPosWork
    {
    }
}