using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    public abstract class ShopWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_BIZ, User.ORGLY_OPN)]
    [Ui("［商户］线下零售", "desktop")]
    public class BizlyShopWork : ShopWork
    {
        public async Task @default(WebContext wc)
        {
        }
    }
}