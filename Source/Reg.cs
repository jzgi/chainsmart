using CoChain;

namespace Revital
{
    public class Reg : Entity, IKeyable<short>
    {
        public static readonly Reg Empty = new Reg();

        public const short
            TYP_PROV = 1,
            TYP_MRT_DIV = 2,
            TYP_STK_SEC = 3;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_PROV, "省份"},
            {TYP_MRT_DIV, "市场区划"},
            {TYP_STK_SEC, "商品分组"},
        };

        internal short id;
        internal short idx;
        internal short num; // number of resources contained

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(idx), ref idx);
            s.Get(nameof(num), ref num);
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(idx), idx);
            s.Put(nameof(num), num);
        }

        public short Key => id;

        public bool IsProv => typ == TYP_PROV;

        public bool IsMrtDiv => typ == TYP_MRT_DIV;

        public bool IsStkGrp => typ == TYP_STK_SEC;

        public override string ToString() => name;
    }
}