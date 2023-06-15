using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

public class FabGraph : TwinGraph<int, Fab>
{
    public override bool TryGetTwinSetKey(DbContext dc, int key, out int setkey)
    {
        dc.Sql("SELECT orgid FROM fabs_vw WHERE id = @1 AND status > 0");
        if (dc.QueryTop(p => p.Set(key)))
        {
            dc.Let(out setkey);

            return true;
        }

        setkey = default;
        return false;
    }

    public override Map<int, Fab> LoadTwinSet(DbContext dc, int setkey)
    {
        dc.Sql("SELECT ").collst(Fab.Empty).T(" FROM fabs_vw WHERE orgid = @1 AND status > 0 ORDER BY id");
        return dc.Query<int, Fab>(p => p.Set(setkey));
    }
}