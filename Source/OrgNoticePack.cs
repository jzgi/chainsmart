using System.Text;
using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

/**
 * The notice pack pertaining to a particular org.
 */
public class OrgNoticePack : TwinPack<Notice>
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
        { PUR_OKED, "输运" },
        { PUR_VOID, "撤单" },
        { PUR_REFUND, "返现" },
    };


    private static readonly char[] NUMS =
    {
        '①', '②', '③', '④', '⑤', '⑥', '⑦', '⑧', '⑨', '⑩', '⑪', '⑫'
    };


    private int toPush;


    public void Put(short slot, int num, decimal amt)
    {
        lock (this)
        {
            var idx = IndexOf(slot);
            if (idx == -1)
            {
                Add(slot, new Notice(slot, num, amt));
            }
            else
            {
                entries[idx].Value.AddUp(num, amt);
            }

            toPush += num;
        }
    }


    public void PushToBuffer(StringBuilder sb)
    {
        lock (this)
        {
            var ord = 0;
            for (var i = 0; i < Count; i++)
            {
                var v = ValueAt(i);
                if (v.IsStuffed)
                {
                    sb.Append(NUMS[ord++]).Append(' ');

                    v.PutToBuffer(sb);

                    v.Reset();
                }
            }

            toPush = 0;
        }
    }

    public int Check(short slot, bool clear = false)
    {
        lock (this)
        {
            var idx = IndexOf(slot);

            if (idx == -1) return 0;

            var ret = entries[idx].Value.SpyCount;

            if (clear)
            {
                entries[idx].Value.SpyCount = 0;
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
}