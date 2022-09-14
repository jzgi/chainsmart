using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class OrglyWork : WebWork
    {
    }

    /// <summary>
    /// Works for markets and shops.
    /// </summary>
#if ZHNT
    [Ui("市场／商户操作")]
#else
    [Ui("驿站商户操作")]
#endif
    public class MrtlyWork : OrglyWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<MrtlyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).orgid;
                    return GrabObject<int, Org>(orgid);
                }
            );
        }
    }

    /// <summary>
    /// Works for sources and producers.
    /// </summary>
    [Ui("供源／生产户操作")]
    public class SrclyWork : OrglyWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<SrclyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).orgid;
                    return GrabObject<int, Org>(orgid);
                }
            );
        }
    }
}