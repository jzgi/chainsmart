using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

public class FabGraph : TwinGraph<Fab>
{
    public override Fab Load(DbContext dc, int key)
    {
        dc.Sql("SELECT ").collst(Fab.Empty).T(" FROM fabs_vw WHERE id = @1 AND status > 0");
        return dc.QueryTop<Fab>(p => p.Set(key));
    }

    public override Map<int, Fab> LoadMap(DbContext dc, int setkey)
    {
        dc.Sql("SELECT ").collst(Fab.Empty).T(" FROM fabs_vw WHERE orgid = @1 AND status > 0 ORDER BY id");
        return dc.Query<int, Fab>();
    }
}