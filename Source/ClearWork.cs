using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    [UserAuthorize(admly: 1)]
    [Ui("平台业务结算管理", "table")]
    public class AdmlyClearWork : WebWork
    {
        protected override void OnMake()
        {
        }

        public async Task @default(WebContext wc)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [Ui("账户业务结算", "credit-card")]
    public class OrglyClearWork : WebWork
    {
        protected override void OnMake()
        {
        }

        public async Task @default(WebContext wc)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }
}