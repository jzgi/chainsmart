using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public class CreditWork : WebWork
    {
    }

    [Ui("碳积分账户", "功能")]
    public class MyCreditWork : CreditWork
    {
        [Ui("碳积分账户"), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                // spr and rvr
            }, false, 6);
        }
    }
}