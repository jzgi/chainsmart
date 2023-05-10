using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

public class FabGraph : TwinGraph<int, int, Fab>
{
    public override Fab Load(DbContext dc, int key)
    {
        dc.Sql("SELECT ").collst(Fab.Empty).T(" FROM fabs_vw WHERE id = @1 AND status > 0");
        return dc.QueryTop<Fab>(p => p.Set(key));
    }

    public override Map<int, Fab> LoadGroup(DbContext dc, int gkey)
    {
        dc.Sql("SELECT ").collst(Fab.Empty).T(" FROM fabs_vw WHERE orgid = @1 AND status > 0 ORDER BY id");
        return dc.Query<int, Fab>(p => p.Set(gkey));
    }

    public override bool Save(DbContext dc, Fab v)
    {
        throw new System.NotImplementedException();
    }
}