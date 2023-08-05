using System;
using System.Text;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

public class OrgGraph : TwinGraph<int, Org>
{
    public override bool TryGetTwinSetKey(DbContext dc, int key, out int setkey)
    {
        if (dc.QueryTop("SELECT upperid FROM orgs_vw WHERE id = @1", p => p.Set(key)))
        {
            dc.Let(out setkey);
            return true;
        }
        setkey = -1;
        return false;
    }

    public override Map<int, Org> LoadTwinSet(DbContext dc, int setkey)
    {
        if (setkey == 0)
        {
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE upperid IS NULL ORDER BY regid, id");
            return dc.Query<int, Org>();
        }
        else
        {
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE upperid = @1 ORDER BY regid, id");
            return dc.Query<int, Org>(p => p.Set(setkey));
        }
    }

    protected override async Task<int> TwinSetIoCycleAsync(int setkey, Map<int, Org> set)
    {
        // use same builder for each and every sent notice
        var now = DateTime.Now;
        var nowStr = now.ToString("yyyy-MM-dd HH mm");

        int num = 0;
        var sb = new StringBuilder();
        foreach (var ety in set)
        {
            var org = ety.Value;
            var pack = org.NoticePack;
            if (pack.HasToPush)
            {
                pack.Dump(sb, now);

                // send sms
                await WeixinUtility.SendNotifSmsAsync(org.Tel, org.Name, nowStr, sb.ToString());

                num++;
            }

            // reset buffer
            sb.Clear();
        }

        return num;
    }
}