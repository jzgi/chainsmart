using CoChain;
using CoChain.Web;
using static CoChain.Nodal.Store;

namespace Revital
{
    public abstract class OrglyWork : WebWork
    {
    }

    /// <summary>
    /// source and producer
    /// </summary>
    [Ui("供应产源操作")]
    public class PrvnlyWork : OrglyWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<PrvnlyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).orgid;
                    return GrabObject<int, Org>(orgid);
                }
            );
        }
    }

#if ZHNT
    [Ui("市场商户操作")]
#else
    [Ui("驿站商户操作")]
#endif
    public class MartlyWork : OrglyWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<MartlyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).orgid;
                    return GrabObject<int, Org>(orgid);
                }
            );
        }
    }
}