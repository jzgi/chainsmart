using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

public class VanGraph : TwinGraph<int, int, Van>
{
    public override bool TryGetGroupKey(DbContext dc, int key, out int gkey)
    {
        dc.Sql("SELECT orgid FROM vans_vw WHERE id = @1 AND status > 0");
        if (dc.QueryTop(p => p.Set(key)))
        {
            dc.Let(out gkey);

            return true;
        }

        gkey = default;
        return false;
    }

    public override Map<int, Van> LoadGroup(DbContext dc, int gkey)
    {
        dc.Sql("SELECT ").collst(Van.Empty).T(" FROM vans_vw WHERE orgid = @1 AND status > 0 ORDER BY id");
        return dc.Query<int, Van>(p => p.Set(gkey));
    }
}