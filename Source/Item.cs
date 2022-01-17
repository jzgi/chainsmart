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
            TYP_SRVC = 4; // service


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_AGRI, "农副产"},
            {TYP_FACT, "工业品"},
            {TYP_SRVC, "服务类"},
        };

        // categories
        public static readonly Map<short, string> Cats = new Map<short, string>
        {
            // agriculture 1-20
            {1, "蔬菜"},
            {2, "瓜果"},
            {3, "粮油"},
            {4, "副产"},
            {5, "禽产"},
            {6, "畜产"},
            {7, "水产"},
            {8, "其它"},
            // factory 21-40
            {21, "日用"},
            {22, "家居"},
            {23, "服装"},
            {24, "电器"},
            {25, "农资"},
            // service 61-80
            {61, "调养"},
            {62, "餐品"},
            {63, "快餐"},
            {64, "家政"},
            {65, "陪护"},
            {66, "养老"},
            {67, "公益"},
            {68, "志愿"},
            {69, "广告"}
        };

        public const short
            OP_INSERT = TYP | STATUS | LABEL | CREATE | BASIC,
            OP_UPDATE = STATUS | LABEL | ADAPT | BASIC;


        internal short id;
        internal short cat;
        internal string unit; // standard unit
        internal string unitip;
        internal decimal fee;

        // must have an icon

        public override void Read(ISource s, short proj = 0x0fff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((proj & BASIC) == BASIC)
            {
                s.Get(nameof(cat), ref cat);
                s.Get(nameof(unit), ref unit);
                s.Get(nameof(unitip), ref unitip);
                s.Get(nameof(fee), ref fee);
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
                s.Put(nameof(cat), cat);
                s.Put(nameof(unit), unit);
                s.Put(nameof(unitip), unitip);
                s.Put(nameof(fee), fee);
            }
        }

        public short Key => id;

        public override string ToString() => name;
    }
}