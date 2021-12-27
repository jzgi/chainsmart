using SkyChain;

namespace Revital
{
    public class Reg : _Info, IKeyable<short>
    {
        public static readonly Reg Empty = new Reg();

        public const short
            TYP_GEOGRAPHIC = 1,
            TYP_SECTIONAL = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_GEOGRAPHIC, "地区"},
            {TYP_SECTIONAL, "场区"},
        };

        internal short id;
        internal short idx;

        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(idx), ref idx);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            base.Write(s, proj);
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(idx), idx);
        }

        public short Key => id;

        public bool IsGeographic => typ == TYP_GEOGRAPHIC;

        public bool IsSectional => typ == TYP_SECTIONAL;

        public override string ToString() => name;
    }
}