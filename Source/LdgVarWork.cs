using System.Threading.Tasks;
using ChainFx.Web;

namespace ChainSmart;

public class LdgVarWork : WebWork
{
}

public class AdmlyPurLdgVarWork : LdgVarWork
{
}

public class AdmlyBuyLdgVarWork : LdgVarWork
{
}

public class RtllyBuyLdgVarWork : LdgVarWork
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

public class RtllyPurLdgVarWork : LdgVarWork
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

public class SuplyPurLdgVarWork : LdgVarWork
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