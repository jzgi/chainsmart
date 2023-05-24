using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

public class FabGraph : TwinGraph<int, int, Fab>
{
    public override bool TryGetGroupKey(DbContext dc, int key, out int gkey)
    {
        dc.Sql("SELECT orgid FROM fabs_vw WHERE id = @1 AND status > 0");
        if (dc.QueryTop(p => p.Set(key)))
        {
            dc.Let(out gkey);

            return true;
        }

        gkey = default;
        return false;
    }

    public override Map<int, Fab> LoadGroup(DbContext dc, int gkey)
    {
        dc.Sql("SELECT ").collst(Fab.Empty).T(" FROM fabs_vw WHERE orgid = @1 AND status > 0 ORDER BY id");
        return dc.Query<int, Fab>(p => p.Set(gkey));
    }
}