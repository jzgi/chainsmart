using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

public class SrcGraph : TwinGraph<int, Src>
{
    public override bool TryGetTwinSetKey(DbContext dc, int key, out int setkey)
    {
        dc.Sql("SELECT orgid FROM srcs_vw WHERE id = @1 AND status > 0");
        if (dc.QueryTop(p => p.Set(key)))
        {
            dc.Let(out setkey);

            return true;
        }

        setkey = default;
        return false;
    }

    public override Map<int, Src> LoadTwinSet(DbContext dc, int setkey)
    {
        dc.Sql("SELECT ").collst(Src.Empty).T(" FROM srcs_vw WHERE orgid = @1 AND status > 0 ORDER BY id");
        return dc.Query<int, Src>(p => p.Set(setkey));
    }
}