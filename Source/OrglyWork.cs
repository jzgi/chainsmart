using ChainFx;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public abstract class OrglyWork : WebWork
{
}

[Ui("市场操作")]
[UserAuthenticate]
public class RtllyWork : OrglyWork
{
    protected override void OnCreate()
    {
        // id of either current user or the specified
        CreateVarWork<RtllyVarWork>((prin, key) =>
            {
                var orgid = key?.ToInt() ?? ((User)prin).rtlid;
                return GrabTwin<int, int, Org>(orgid);
            }
        );
    }
}

[UserAuthenticate]
[Ui("供应操作")]
public class SuplyWork : OrglyWork
{
    protected override void OnCreate()
    {
        // id of either current user or the specified
        CreateVarWork<SuplyVarWork>((prin, key) =>
            {
                var orgid = key?.ToInt() ?? ((User)prin).supid;
                return GrabTwin<int, int, Org>(orgid);
            }
        );
    }
}