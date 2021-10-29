using SkyChain;

namespace Revital.Supply
{
    public class Reg_ : _Bean, IKeyable<string>
    {
        public static readonly Reg_ Empty = new Reg_();

        public const short
            TYP_PROVINCE = 1,
            TYP_CITY = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_PROVINCE, "省份／直辖市"},
            {TYP_CITY, "地市"},
        };

        internal string id;
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

        public string Key => id;

        public bool IsMetropolis => typ == 1;

        public bool IsCity => typ == 2;

        public override string ToString() => name;
    }
}