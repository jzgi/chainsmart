using SkyChain;
using SkyChain.Web;

namespace Revital.Site
{

    [Ui("控配中心操作")]
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
    [Ui("市场端操作")]
#else
    [Ui("驿站端操作")]
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