using SkyChain;
using SkyChain.Web;

namespace Revital
{
    public abstract class OrglyWork : WebWork
    {
    }

    /// <summary>
    /// source and producer
    /// </summary>
    public class PrvlyWork : OrglyWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<PrvlyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).orgid;
                    return GrabObject<int, Org>(orgid);
                }
            );
        }
    }


    /// <summary>
    /// supply
    /// </summary>
    public class CtrlyWork : OrglyWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<CtrlyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).orgid;
                    return GrabObject<int, Org>(orgid);
                }
            );
        }
    }


    /// <summary>
    /// mart and biz
    /// </summary>
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
}