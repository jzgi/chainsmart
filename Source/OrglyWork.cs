using SkyChain.Web;

namespace Revital
{
    public abstract class OrglyWork : WebWork
    {
    }

    /// <summary>
    /// source and farm
    /// </summary>
    public class SrclyWork : OrglyWork
    {
        protected override void OnMake()
        {
            MakeVarWork<SrclyVarWork>(prin =>
                {
                    var orgid = ((User) prin).orgid;
                    return Obtain<int, Org>(orgid);
                }
            );
        }
    }


    /// <summary>
    /// center
    /// </summary>
    public class CtrlyWork : OrglyWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyVarWork>(prin =>
                {
                    var orgid = ((User) prin).orgid;
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
            MakeVarWork<MrtlyVarWork>(prin =>
                {
                    var orgid = ((User) prin).orgid;
                    return Obtain<int, Org>(orgid);
                }
            );
        }
    }
}