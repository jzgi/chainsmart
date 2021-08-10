using SkyChain;

namespace Zhnt
{
    /// <summary>
    /// An item of standard product.
    /// </summary>
    public class Item : _Art, IKeyable<short>
    {
        public static readonly Item Empty = new Item();

        public const short
            TYP_PRIME = 1,
            TYP_SIDE = 2,
            TYP_SOUP = 3,
            TYP_NUT = 4,
            TYP_STAPLE = 5,
            TYP_BREAKFAST = 6,
            TYP_JUICE = 7;

        // types
        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_PRIME, "主菜"},
            {TYP_SIDE, "副菜"},
            {TYP_SOUP, "汤饮"},
            {TYP_NUT, "坚果"},
            {TYP_STAPLE, "主食"},
            {TYP_BREAKFAST, "早餐料"},
            {TYP_JUICE, "调养汁"},
        };

        // programs
        public static readonly Map<short, string> Progg = new Map<short, string>
        {
            {0, null},
            {1, "瘦身"},
            {2, "降压"},
            {4, "养生"},
            {8, "降糖"},
            {5, "瘦身＋养生"},
            {7, "瘦身＋降压＋养生"},
        };


        internal short id;

        internal short progg; // programing

        internal decimal price;


        public override void Read(ISource s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            base.Read(s, proj);

            s.Get(nameof(progg), ref progg);
            s.Get(nameof(price), ref price);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            base.Write(s, proj);

            s.Put(nameof(progg), progg);
            s.Put(nameof(price), price);
        }

        public short Key => id;

        public bool IsFor(short prog) => progg == 0 || (progg & prog) == prog;

        public override string ToString() => name;
    }
}