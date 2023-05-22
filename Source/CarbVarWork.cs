using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public abstract class CarbVarWork : WebWork
{
}

public class MyCarbVarWork : CarbVarWork
{
    public async Task @default(WebContext wc, int typ)
    {
    }
}