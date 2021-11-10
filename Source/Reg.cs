using SkyChain;

namespace Revital
{
    public class Reg : _Doc, IKeyable<short>
    {
        public static readonly Reg Empty = new Reg();

        public const short
            TYP_GEOGRAPHIC = 1,
            TYP_INDOOR = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_GEOGRAPHIC, "地理区域"},
            {TYP_INDOOR, "内部区域"},
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

        public bool IsMetropolis => typ == 1;

        public bool IsDistrict => typ == 2;

        public override string ToString() => name;
    }
}