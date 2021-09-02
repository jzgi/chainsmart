using SkyChain;

namespace Zhnt.Supply
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
            {1, "瓜果"},
            {2, "蔬菜"},
            {3, "粮油"},
            {4, "其他种植"},
            {5, "禽类"},
            {6, "畜类"},
            {7, "水产"},
            {8, "其他养殖"},
            {9, "杂类"},
        };

        // programs


        internal short id;

        internal string unit; // basic unit

        internal string unitip;

        internal short upc;

        // must have an icon

        public override void Read(ISource s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            base.Read(s, proj);

            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitip), ref unitip);
            s.Get(nameof(upc), ref upc);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            base.Write(s, proj);

            s.Put(nameof(unit), unit);
            s.Put(nameof(unitip), unitip);
            s.Put(nameof(upc), upc);
        }

        public short Key => id;

        public bool IsFor(short prog) => false;

        public override string ToString() => name;
    }
}