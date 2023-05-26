using System;
using System.Text;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

public class OrgGraph : TwinGraph<int, Org>
{
    public override bool TryGetGroupKey(DbContext dc, int key, out int setkey)
    {
        dc.Sql("SELECT prtid FROM orgs_vw WHERE id = @1 AND status > 0");

        if (dc.QueryTop(p => p.Set(key)))
        {
            dc.Let(out setkey);
            return true;
        }
        setkey = -1;
        return false;
    }

    public override Map<int, Org> LoadGroup(DbContext dc, int setkey)
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

    protected override async Task<int> DischargeGroupAsync(int setkey, Map<int, Org> set)
    {
        // use same builder for each and every sent notice
        var sb = new StringBuilder();
        var nowStr = DateTime.Now.ToString("yyyy-MM-dd HH mm");

        int num = 0;

        foreach (var ety in set)
        {
            var org = ety.Value;
            var box = org.Box;
            if (box.HasToPush)
            {
                box.PushToBuffer(sb);

                // send
                await WeixinUtility.SendNotifSmsAsync(org.Tel,
                    org.Name,
                    nowStr,
                    sb.ToString()
                );

                num++;
            }

            // reset buffer
            sb.Clear();
        }
        
        return num;
    }
}