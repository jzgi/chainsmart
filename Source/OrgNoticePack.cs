using System;
using System.Text;
using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

/// <summary>
/// The notice pack pertaining to a particular org. 
/// </summary>
public class OrgNoticePack : IPack<StringBuilder>
{
    public const short
        BUY_CREATED = 1,
        BUY_OKED = 2,
        BUY_VOID = 3,
        BUY_REFUND = 4,
        PUR_CREATED = 5,
        PUR_OKED = 6,
        PUR_VOID = 7,
        PUR_REFUND = 8;

    public static readonly Map<short, string> Typs = new()
    {
        { BUY_CREATED, "新单" },
        { BUY_OKED, "派发" },
        { BUY_VOID, "撤单" },
        { BUY_REFUND, "返现" },
        { PUR_CREATED, "新单" },
        { PUR_OKED, "发货" },
        { PUR_VOID, "撤单" },
        { PUR_REFUND, "返现" },
    };


    private static readonly char[] NUMS =
    {
        '①', '②', '③', '④', '⑤', '⑥', '⑦', '⑧', '⑨', '⑩', '⑪', '⑫'
    };


    readonly Entry[] entries = new Entry[Typs.Count];

    private int toPush;

    private DateTime since = DateTime.Now;

    public void Put(short idx, int num, decimal amt)
    {
        lock (this)
        {
            entries[idx].AddUp(num, amt);

            toPush += num;
        }
    }

    public DateTime Since => since;

    public void Dump(StringBuilder bdr, DateTime now)
    {
        lock (this)
        {
            var ord = 0;
            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i].IsStuffed)
                {
                    bdr.Append(NUMS[ord++]).Append(' ');

                    entries[i].PutToBuilder(bdr);

                    entries[i].Reset();

                    since = now;
                }
            }

            toPush = 0;
        }
    }

    public int Check(short idx, bool clear = false)
    {
        lock (this)
        {
            var ret = entries[idx].SpyCount;

            if (clear)
            {
                entries[idx].SpyCount = 0;
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


    internal struct Entry : IKeyable<short>
    {
        internal readonly short typ;

        internal int count;

        internal decimal sum;

        public int SpyCount;

        internal bool IsEmpty => typ == 0 || count == 0;

        internal bool IsStuffed => typ != 0 && count != 0;

        public Entry(short slot, int num, decimal amt)
        {
            typ = slot;
            count = num;
            sum = amt;

            SpyCount = num;
        }

        internal void AddUp(int num, decimal amt)
        {
            count += num;
            sum += amt;

            SpyCount += num;
        }

        internal void Reset()
        {
            count = 0;
            sum = 0;
        }

        internal void PutToBuilder(StringBuilder sb)
        {
            sb.Append(Typs[typ]).Append(' ').Append(count).Append(' ').Append('￥').Append(sum);
        }

        public short Key => typ;
    }
}