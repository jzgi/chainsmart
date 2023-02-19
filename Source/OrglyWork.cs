using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Nodality;

namespace ChainSMart
{
    public abstract class OrglyWork : WebWork
    {
    }

    /// 
    /// Works for zones and sources.
    /// 
    [UserAuthenticate]
    [Ui("供区产源操作")]
    public class SrclyWork : OrglyWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<SrclyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).srcid;
                    return GrabObject<int, Org>(orgid);
                }
            );
        }
    }

    /// 
    /// Works for markets and shops.
    /// 
#if ZHNT
    [Ui("市场商户操作")]
#else
    [Ui("驿站商户操作")]
#endif
    [UserAuthenticate]
    public class ShplyWork : OrglyWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<ShplyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).shpid;
                    return GrabObject<int, Org>(orgid);
                }
            );
        }
    }
}