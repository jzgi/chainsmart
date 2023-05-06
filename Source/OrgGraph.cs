using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

public class OrgGraph : TwinGraph<Org>
{
    public override Org Load(DbContext dc, int key)
    {
        dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1 AND status > 0");
        return dc.QueryTop<Org>(p => p.Set(key));
    }

    public override Map<int, Org> LoadMap(DbContext dc, int setkey)
    {
        if (setkey == 0)
        {
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid IS NULL AND status > 0 ORDER BY regid, id");
            return dc.Query<int, Org>();
        }
        else
        {
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 AND status > 0 ORDER BY regid, id");
            return dc.Query<int, Org>(p => p.Set(setkey));
        }
    }
}