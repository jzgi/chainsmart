using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

public class VanGraph : TwinGraph<Van>
{
    public override Van Load(DbContext dc, int key)
    {
        dc.Sql("SELECT ").collst(Van.Empty).T(" FROM vans_vw WHERE id = @1 AND status > 0");
        return dc.QueryTop<Van>(p => p.Set(key));
    }

    public override Map<int, Van> LoadMap(DbContext dc, int setkey)
    {
        dc.Sql("SELECT ").collst(Van.Empty).T(" FROM vans_vw WHERE orgid = @1 AND status > 0 ORDER BY id");
        return dc.Query<int, Van>(p => p.Set(setkey));
    }
}