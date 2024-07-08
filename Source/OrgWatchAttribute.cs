using ChainFX;
using ChainFX.Nodal;
using ChainFX.Web;

namespace ChainSmart;

public class OrgWatchAttribute : WatchAttribute
{
    public const short
        BUY_ADAPTED = 1,
        BUY_OKED = 2,
        BUY_VOID = 3,
        BUY_REFUND = 4,
        PUR_CREATED = 5,
        PUR_OKED = 6,
        PUR_VOID = 7,
        PUR_REFUND = 8;

    public static readonly Map<short, string> Typs = new()
    {
        { BUY_ADAPTED, "新单" },
        { BUY_OKED, "派发" },
        { BUY_VOID, "撤单" },
        { BUY_REFUND, "返现" },
        { PUR_CREATED, "新单" },
        { PUR_OKED, "发货" },
        { PUR_VOID, "撤单" },
        { PUR_REFUND, "返现" },
    };


    public OrgWatchAttribute(short kind) : base(kind)
    {
    }

    public override int Peek(int orgid, bool clear = false)
    {
        var org = Storage.GrabTwin<int, Org>(orgid);

        return org.WatchSet.Peek(Kind, clear);
    }
}