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
        dc.Sql("SELECT parentid FROM orgs_vw WHERE id = @1 AND status > 0");

        if (dc.QueryTop(p => p.Set(key)))
        {
            dc.Let(out setkey);
            if (setkey == 0)
            {
                setkey = key;
            }
            return true;
        }
        setkey = -1;
        return false;
    }

    public override Map<int, Org> LoadTwinSet(DbContext dc, int setkey)
    {
        if (setkey == 0)
        {
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE parentid IS NULL AND status > 0 ORDER BY regid, id");
            return dc.Query<int, Org>();
        }
        else
        {
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE (id = @1 OR parentid = @1) AND status > 0 ORDER BY regid, id");
            return dc.Query<int, Org>(p => p.Set(setkey));
        }
    }

    protected override async Task<int> TwinSetIoCycleAsync(int setkey, Map<int, Org> set)
    {
        // use same builder for each and every sent notice
        var sb = new StringBuilder();
        var nowStr = DateTime.Now.ToString("yyyy-MM-dd HH mm");

        int num = 0;

        foreach (var ety in set)
        {
            var org = ety.Value;
            var box = org.Notices;
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