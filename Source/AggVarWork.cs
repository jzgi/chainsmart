using System.Threading.Tasks;
using ChainFX.Web;

namespace ChainSMart
{
    public abstract class AggVarWork : WebWork
    {
    }

    public class AdmlyBookAggVarWork : AggVarWork
    {
    }

    public class AdmlyBuyAggVarWork : AggVarWork
    {
    }

    public class OrglyAggrVarWork : AggVarWork
    {
        [Ui("￥", "微信领款"), Tool(Modal.ButtonOpen)]
        public async Task rcv(WebContext wc, int dt)
        {
            int orderid = wc[0];
            if (wc.IsGet)
            {
            }
            else // POST
            {
                wc.GivePane(200); // close
            }
        }
    }
}