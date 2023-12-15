using ChainFX;
using ChainFX.Nodal;

namespace ChainSmart;

public class SrcCache : DbTwinCache<int, Src>
{
    public override bool IsAsync => false;

    public override bool TryGetForkKey(DbContext dc, int key, out int forkKey)
    {
        dc.Sql("SELECT orgid FROM srcs_vw WHERE id = @1 AND status > 0");
        if (dc.QueryTop(p => p.Set(key)))
        {
            dc.Let(out forkKey);

            return true;
        }

        forkKey = default;
        return false;
    }

    public override Map<int, Src> LoadFork(DbContext dc, int forkKey)
    {
        dc.Sql("SELECT ").collst(Src.Empty).T(" FROM srcs_vw WHERE orgid = @1 AND status > 0 ORDER BY id");
        return dc.Query<int, Src>(p => p.Set(forkKey));
    }
}