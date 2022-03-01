using SkyChain;

namespace Revital
{
    /// <summary>
    /// The data modal for an standard item of product or service.
    /// </summary>
    public class Item : Info, IKeyable<short>
    {
        public static readonly Item Empty = new Item();

        // categories
        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {1, "蔬菜"},
            {2, "瓜果"},
            {3, "粮油"},
            {4, "种植"},
            {5, "畜产"},
            {6, "禽产"},
            {7, "水产"},
            {8, "养殖"},

            {11, "餐品"},
            {12, "健管"},
            {13, "家政"},
            {14, "养老"},
            {15, "广告"},
            {16, "公益"},

            {21, "日用"},
            {22, "家居"},
            {23, "服装"},
            {24, "电器"},
        };

        internal short id;
        internal string unit; // standard unit
        internal string unitip;

        // must have an icon

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitip), ref unitip);
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitip), unitip);
        }

        public short Key => id;

        public override string ToString() => name;
    }
}