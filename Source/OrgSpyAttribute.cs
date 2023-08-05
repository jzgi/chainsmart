using ChainFx.Nodal;

namespace ChainSmart;

public class OrgSpyAttribute : TwinSpyAttribute
{
    public OrgSpyAttribute(short slot) : base(slot)
    {
    }

    public override int Do(int orgid, bool clear = false)
    {
        var org = Nodality.GrabTwin<int, Org>(orgid);

        return org.NoticePack.Check(slot, clear);
    }
}