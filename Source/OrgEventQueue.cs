using System.Text;
using ChainFx;
using ChainFx.Nodal;
using Microsoft.Extensions.Primitives;

namespace ChainSmart;

/// <summary>
/// The output event queue for an org which contains b 
/// </summary>
public class OrgEventQueue : TwinEdgiePack
{
    public const short
        DELIVERY = 1,
        PUR_ADAPTED = 2,
        BUY_VOID = 8;

    public static readonly Map<short, string> Slots = new()
    {
        { DELIVERY, "社区合单" },
        { PUR_ADAPTED, "供应发货" },


        { BUY_VOID, "消费撤单" },
    };


    const int CAPACITY = 12;

    private static readonly char[] NUMS =
    {
        '①', '②', '③', '④', '⑤', '⑥', '⑦', '⑧', '⑨', '⑩', '⑪', '⑫'
    };


    // entries of various types
    readonly Entry[] entries = new Entry[CAPACITY];

    int count;


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
            entries[idx].Feed(slot, num, amt);
            count += num;
        }
    }

    public void PutDeliveryAssign(string user, Buy[] arr)
    {
        lock (this)
        {
            // add push
        }
    }


    public void PushToBuffer(StringBuilder sb)
    {
        lock (this)
        {
            var ord = 0;
            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i].IsStuffed)
                {
                    sb.Append(NUMS[ord++]).Append(' ');

                    entries[i].PutToBuffer(sb);

                    entries[i].Reset();
                }
            }

            count = 0;
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
            var ret = entries[idx].spyCount;

            if (clear)
            {
                entries[idx].spyCount = 0;
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
                return count > 0;
            }
        }
    }


    public struct Entry
    {
        internal short typ;

        internal int count;

        internal StringValues sum;

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
        }

        internal void PutToBuffer(StringBuilder sb)
        {
            sb.Append(Slots[typ]).Append(' ').Append(count).Append(' ').Append('￥').Append(sum);
        }
    }
}