using System.Threading.Tasks;
using ChainFx.Web;

namespace ChainSmart;

public class AggVarWork : WebWork
{
}

public class AdmlyOrdAggVarWork : AggVarWork
{
}

public class AdmlyBuyAggVarWork : AggVarWork
{
}

public class RtllyBuyAggVarWork : AggVarWork
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

public class RtllyOrdAggVarWork : AggVarWork
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

public class SuplyOrdAggVarWork : AggVarWork
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