using System.Text;
using ChainFx;

namespace ChainSmart;

public struct Notice : IKeyable<short>
{
    internal readonly short typ;

    internal int count;

    internal decimal sum;

    public int SpyCount;

    internal bool IsEmpty => typ == 0 || count == 0;

    internal bool IsStuffed => typ != 0 && count != 0;

    public Notice(short slot, int num, decimal amt)
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

    internal void PutToBuffer(StringBuilder sb)
    {
        sb.Append(OrgNoticePack.Typs[typ]).Append(' ').Append(count).Append(' ').Append('￥').Append(sum);
    }

    public short Key => typ;
}