using ChainFx;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart
{
    public abstract class OrglyWork : WebWork
    {
    }

    [Ui("市场操作")]
    [UserAuthenticate]
    public class ShplyWork : OrglyWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<ShplyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User)prin).shpid;
                    return GrabObject<int, Org>(orgid);
                }
            );
        }
    }

    [UserAuthenticate]
    [Ui("供应操作")]
    public class SrclyWork : OrglyWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<SrclyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User)prin).srcid;
                    return GrabObject<int, Org>(orgid);
                }
            );
        }
    }
}