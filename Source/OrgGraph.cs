using System.Threading.Tasks;
using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

public class OrgGraph : TwinGraph<int, int, Org>
{
    public override bool TryGetGroupKey(DbContext dc, int key, out int gkey)
    {
        dc.Sql("SELECT prtid FROM orgs_vw WHERE id = @1 AND status > 0");

        if (dc.QueryTop(p => p.Set(key)))
        {
            dc.Let(out gkey);
            return true;
        }
        gkey = -1;
        return false;
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

    protected override Task<int> DischargeGroupAsync(int gkey, Map<int, Org> group)
    {
        return base.DischargeGroupAsync(gkey, group);
    }

    protected override void OnCreate()
    {
        base.OnCreate();
    }
}