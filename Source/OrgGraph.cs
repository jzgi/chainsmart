using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

public class OrgGraph : TwinGraph<int, int, Org>
{
    public override Org Load(DbContext dc, int key)
    {
        dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1 AND status > 0");
        return dc.QueryTop<Org>(p => p.Set(key));
    }

    public override Map<int, Org> LoadGroup(DbContext dc, int gkey)
    {
        if (gkey == 0)
        {
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid IS NULL AND status > 0 ORDER BY regid, id");
            return dc.Query<int, Org>();
        }
        else
        {
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 AND status > 0 ORDER BY regid, id");
            return dc.Query<int, Org>(p => p.Set(gkey));
        }
    }

    public override bool Save(DbContext dc, Org v)
    {
        const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;

        dc.Sql("INSERT INTO orgs ").colset(Org.Empty, msk)._VALUES_(Org.Empty, msk);
        return dc.Execute(p => v.Write(p, msk)) == 1;
    }
}