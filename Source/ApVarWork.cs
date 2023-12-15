using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Nodality;

namespace ChainSmart;

public abstract class ApVarWork : WebWork
{
}

public class AdmlyBuyApVarWork : ApVarWork
{
    public async Task @default(WebContext wc)
    {
        int xorgid = wc[0];
        DateTime dt = wc.Query[nameof(dt)];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM buyaps WHERE level = 1 AND dt = @1 AND xorgid = @2");
        var arr = await dc.QueryAsync<Ap>(p => p.Set(dt).Set(xorgid));

        var xorg = GrabTwin<int, Org>(xorgid);
        var orgs = GrabTwinArray<int, Org>(xorgid).AddOf(xorg, first: true);

        await wc.GiveXls(200, false, xorgid, dt, arr, orgs.ToMap<int, Org>());
    }
}

public class AdmlyPurApVarWork : ApVarWork
{
}

public class OrglyApVarWork : ApVarWork
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