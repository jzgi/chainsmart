using ChainFx.Nodal;

namespace ChainSmart;

public class OrgSpyAttribute : TwinSpyAttribute
{
    public OrgSpyAttribute(short slot) : base(slot)
    {
    }

    public override int DoSpy(int orgid, bool clear = false)
    {
        var org = Nodality.GrabTwin<int, Org>(orgid);

        return org.Notices.CheckPully(slot, clear);
    }
}