using SkyChain;

namespace Supply
{
    public class Reg : Art_, IKeyable<short>
    {
        public static readonly Reg Empty = new Reg();

        public const short
            TYP_METROPOLIS = 1,
            TYP_CITY = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_METROPOLIS, "省会"},
            {TYP_CITY, "地市"},
        };

        internal short id;
        internal short idx;

        public override void Read(ISource s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            base.Read(s, proj);
            s.Get(nameof(idx), ref idx);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            base.Write(s, proj);
            s.Put(nameof(idx), idx);
        }

        public short Key => id;

        public bool IsMetropolis => typ == 1;

        public bool IsCity => typ == 2;

        public override string ToString() => name;
    }
}