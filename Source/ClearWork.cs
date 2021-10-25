using System.Threading.Tasks;
using SkyChain.Web;

namespace Rev.Supply
{
    [UserAuthorize(admly: 1)]
    [Ui("结算管理")]
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

    [Ui("业务结算")]
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