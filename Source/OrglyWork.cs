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
    public class SrclyWork : OrglyWork
    {
        protected override void OnMake()
        {
            // id of either current user or the specified
            MakeVarWork<SrclyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).orgid;
                    return Obtain<int, Org>(orgid);
                }
            );
        }
    }


    /// <summary>
    /// supply
    /// </summary>
    public class CtrlyWork : OrglyWork
    {
        protected override void OnMake()
        {
            // id of either current user or the specified
            MakeVarWork<CtrlyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).orgid;
                    return Obtain<int, Org>(orgid);
                }
            );
        }
    }


    /// <summary>
    /// mart and biz
    /// </summary>
    public class MrtlyWork : OrglyWork
    {
        protected override void OnMake()
        {
            // id of either current user or the specified
            MakeVarWork<MrtlyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).orgid;
                    return Obtain<int, Org>(orgid);
                }
            );
        }
    }
}