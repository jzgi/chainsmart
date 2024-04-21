using ChainFX.Nodal;

namespace ChainSmart;

public class OrgWatchAttribute : WatchAttribute
{
    public OrgWatchAttribute(short kind) : base(kind)
    {
    }

    public override int Peek(int orgid, bool clear = false)
    {
        var org = Storage.GrabTwin<int, Org>(orgid);

        return org.NoticePack.Peek(Kind, clear);
    }
}