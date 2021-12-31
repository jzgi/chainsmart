using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    public abstract class StationWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_BIZ, User.ORGLY_OP)]
    [Ui("［商户］线下零售", "desktop")]
    public class BizlyShopWork : StationWork
    {
        public async Task @default(WebContext wc)
        {
        }
    }
}