using System.Text;
using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

/// <summary>
/// A notice pertaining to a particular org.
/// </summary>
public class OrgBox : TwinBox
{
    public const short
        PUR_CREATED = 1,
        PUR_ADAPTED = 2,
        PUR_OKED = 3,
        PUR_VOID = 4,
        BUY_CREATED = 5,
        BUY_ADAPTED = 6,
        BUY_OKED = 7,
        BUY_VOID = 8;

    public static readonly Map<short, string> Slots = new()
    {
        { PUR_CREATED, "供应新单" },
        { PUR_ADAPTED, "供应发货" },
        { PUR_OKED, "供应收货" },
        { PUR_VOID, "供应撤单" },
        { BUY_CREATED, "消费新单" },
        { BUY_ADAPTED, "消费发货" },
        { BUY_OKED, "消费收货" },
        { BUY_VOID, "消费撤单" },
    };


    const int CAPACITY = 12;

    private static readonly char[] NUMS =
    {
        '①', '②', '③', '④', '⑤', '⑥', '⑦', '⑧', '⑨', '⑩', '⑪', '⑫'
    };


    // entries for push
    readonly Entry[] pushy = new Entry[CAPACITY];

    private int toPush;


    public void Put(short slot, int num, decimal amt)
    {
        if (slot > CAPACITY)
        {
            return;
        }

        var idx = slot - 1;

        lock (this)
        {
            // add push
            pushy[idx].Feed(slot, num, amt);
            toPush += num;
        }
    }


    public void PushToBuffer(StringBuilder sb)
    {
        lock (this)
        {
            var ord = 0;
            for (var i = 0; i < pushy.Length; i++)
            {
                if (pushy[i].IsStuffed)
                {
                    sb.Append(NUMS[ord++]).Append(' ');

                    pushy[i].PutToBuffer(sb);

                    pushy[i].Reset();
                }
            }

            toPush = 0;
        }
    }

    public int CheckPully(short slot, bool clear = false)
    {
        if (slot > CAPACITY)
        {
            return 0;
        }

        var idx = slot - 1;

        lock (this)
        {
            var ret = pushy[idx].spyCount;

            if (clear)
            {
                pushy[idx].spyCount = 0;
            }

            return ret;
        }
    }

    public bool HasToPush
    {
        get
        {
            lock (this)
            {
                return toPush > 0;
            }
        }
    }


    public struct Entry
    {
        internal short typ;

        internal int count;

        internal decimal sum;

        internal int spyCount;

        internal bool IsEmpty => typ == 0 || count == 0;

        internal bool IsStuffed => typ != 0 && count != 0;

        internal void Feed(short slot, int num, decimal amt)
        {
            typ = slot;
            count += num;
            spyCount += num;
            sum += amt;
        }

        internal void Reset()
        {
            count = 0;
            sum = 0;
        }

        internal void PutToBuffer(StringBuilder sb)
        {
            sb.Append(Slots[typ]).Append(' ').Append(count).Append(' ').Append('￥').Append(sum);
        }
    }
}