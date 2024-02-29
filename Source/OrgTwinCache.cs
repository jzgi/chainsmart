using System;
using System.Text;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Nodal;

namespace ChainSmart;

public class OrgTwinCache : DbTwinCache<int, Org>
{
    public override bool IsAsync => false;

    public override bool TryGetForkKey(DbContext dc, int key, out int forkKey)
    {
        if (dc.QueryTop("SELECT parentid FROM orgs_vw WHERE id = @1", p => p.Set(key)))
        {
            dc.Let(out forkKey);
            return true;
        }
        forkKey = -1;
        return false;
    }

    public override Map<int, Org> LoadFork(DbContext dc, int forkKey)
    {
        if (forkKey == 0)
        {
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE parentid IS NULL ORDER BY regid, id");
            return dc.Query<int, Org>();
        }

        dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE parentid = @1 ORDER BY regid, id");
        return dc.Query<int, Org>(p => p.Set(forkKey));
    }

    protected override async Task<int> ForkIoCycleAsync(int forkKey, Map<int, Org> fork)
    {
        // use same builder for each and every sent notice
        var now = DateTime.Now;
        var nowStr = now.ToString("yyyy-MM-dd HH mm");

        int num = 0;
        var sb = new StringBuilder();
        foreach (var ety in fork)
        {
            var org = ety.Value;
            var pack = org.NoticePack;
            if (pack.HasToPush)
            {
                pack.Dump(sb, now);

                // send sms
                await WeChatUtility.SendNotifSmsAsync(org.Tel, org.Name, nowStr, sb.ToString());

                num++;
            }

            // reset buffer
            sb.Clear();
        }

        return num;
    }
}