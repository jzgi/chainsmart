using System.Threading.Tasks;
using SkyChain.Web;

namespace Zhnt.Supply
{
    [UserAuthorize(admly: 1)]
    [Ui("交易清算")]
    public class AdmlyClearWork : WebWork
    {
        protected override void OnMake()
        {
        }

        public async Task @default(WebContext wc)
        {
        }
    }

    [Ui("清算报表")]
    public class OrglyClearWork : WebWork
    {
        protected override void OnMake()
        {
        }

        public async Task @default(WebContext wc)
        {
        }
    }
}