using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    public abstract class StationWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_BIZ, User.ORGLY_OP)]
    [Ui("零售工作台", "desktop")]
    public class BizlyStationWork : StationWork
    {
        public async Task @default(WebContext wc)
        {
        }
    }
}