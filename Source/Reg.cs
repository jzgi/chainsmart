using SkyChain;

namespace Revital
{
    public class Reg : _Info, IKeyable<short>
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

        public const short
            OP_INSERT = TYP | STATUS | LABEL | CREATE | ID | BASIC,
            OP_UPDATE = STATUS | LABEL | ADAPT | BASIC,
            ID = 0x0020,
            BASIC = 0x0080;


        internal short id;
        internal short idx;

        public override void Read(ISource s, short proj = 0x0fff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((proj & BASIC) == BASIC)
            {
                s.Get(nameof(idx), ref idx);
            }
        }

        public override void Write(ISink s, short proj = 0x0fff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            if ((proj & BASIC) == BASIC)
            {
                s.Put(nameof(idx), idx);
            }
        }

        public short Key => id;

        public bool IsProvince => typ == TYP_PROV;

        public bool IsDistrict => typ == TYP_DIST;

        public bool IsSection => typ == TYP_SECT;

        public override string ToString() => name;
    }
}