using SkyChain;

namespace Revital
{
    /// <summary>
    /// The data modal for an standard item of product or service.
    /// </summary>
    public class Item : _Info, IKeyable<short>
    {
        public static readonly Item Empty = new Item();

        public const short
            TYP_AGRI = 1, // agriculture
            TYP_FACT = 2, // factory
            TYP_SRVC = 3; // service


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_AGRI, "农副产"},
            {TYP_FACT, "制造品"},
            {TYP_SRVC, "服务业"},
        };

        // categories
        public static readonly Map<short, string> Cats = new Map<short, string>
        {
            // agriculture 1-10
            {1, "蔬菜"},
            {2, "瓜果"},
            {3, "粮油"},
            {4, "畜产"},
            {5, "禽产"},
            {6, "水产"},
            {9, "其它"},

            // factory 21-30
            {11, "日用"},
            {12, "家居"},
            {13, "服装"},
            {14, "电器"},
            {19, "其它"},

            // process & service 11-20
            {21, "调养"},
            {22, "餐品"},
            {23, "家政"},
            {24, "养老"},
            {25, "公益"},
            {26, "广告"},
            {29, "其它"},
        };

        internal short id;
        internal short cat;
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
            s.Get(nameof(cat), ref cat);
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
            s.Put(nameof(cat), cat);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitip), unitip);
        }

        public short Key => id;

        public static bool IsCatOfTyp(short c, short t) => c < t * 10 && c > (t - 1) * 10;

        public override string ToString() => name;
    }
}