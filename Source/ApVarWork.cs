using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public abstract class ApVarWork : WebWork
{
}

public class AdmlyPurApVarWork : ApVarWork
{
}

public class AdmlyBuyApVarWork : ApVarWork
{
    public async Task @default(WebContext wc)
    {
        int orgid = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Ap.Empty).T(" FROM buyaps WHERE xorgid = @1");
        var arr = await dc.QueryAsync<Ap>(p => p.Set(orgid));

        var orgs = GrabTwinSet<int, Org>(orgid);


        await wc.GiveXls(200, false, orgid, arr, orgs.ToMap<int, Org>());
    }
}

public class PtylyApVarWork : ApVarWork
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