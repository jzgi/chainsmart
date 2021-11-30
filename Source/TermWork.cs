using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    public abstract class TermWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_BIZ, User.ORGLY_OP)]
    public abstract class BizlyTermWork : TermWork
    {
        public async Task @default(WebContext wc)
        {
        }
    }

    [Ui("零售计价终端", "desktop", fork: Item.TYP_AGRI)]
    public class BizlyAgriTermWork : BizlyTermWork
    {
    }

    [Ui("零售计价终端", "desktop", fork: Item.TYP_DIET)]
    public class BizlyDietTermWork : BizlyTermWork
    {
    }
}