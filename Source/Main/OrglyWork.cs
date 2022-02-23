using SkyChain;
using SkyChain.Web;

namespace Revital.Main
{
    /// <summary>
    /// source and producer
    /// </summary>
    [Ui("供给端操作")]
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
}