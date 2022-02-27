using SkyChain;

namespace Revital
{
    public class Reg : Info, IKeyable<short>
    {
        public static readonly Reg Empty = new Reg();

        public const short
            TYP_PROV = 1,
            TYP_DIST = 2,
            TYP_SECT = 3;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_PROV, "省份"},
            {TYP_DIST, "地区"},
            {TYP_SECT, "场地"},
        };

        internal short id;
        internal short idx;

        public override void Read(ISource s, short mask = 0xff)
        {
            base.Read(s, mask);

            if ((mask & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(idx), ref idx);
        }

        public override void Write(ISink s, short mask = 0xff)
        {
            base.Write(s, mask);

            if ((mask & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(idx), idx);
        }

        public short Key => id;

        public bool IsProv => typ == TYP_PROV;

        public bool IsDist => typ == TYP_DIST;

        public bool IsSect => typ == TYP_SECT;

        public override string ToString() => name;
    }
}