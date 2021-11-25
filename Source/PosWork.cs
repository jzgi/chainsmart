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

    [Ui("零售计价终端", "desktop", fork: Item.TYP_AGRI)]
    public class BizlyAgriPosWork : BizlyPosWork
    {
    }

    [Ui("零售计价终端", "desktop", fork: Item.TYP_DIET)]
    public class BizlyDietPosWork : BizlyPosWork
    {
    }
}