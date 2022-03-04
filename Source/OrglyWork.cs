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
    [Ui("产源业务操作")]
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


    [Ui("中枢操作")]
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


#if ZHNT
    [Ui("市场业务操作")]
#else
    [Ui("驿站业务操作")]
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
}