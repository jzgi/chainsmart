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
            TYP_SRVC = 2, // service
            TYP_FACT = 4; // factory


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_AGRI, "农副产品"},
            {TYP_SRVC, "加工服务"},
            {TYP_FACT, "工业制品"},
        };

        // categories
        public static readonly Map<short, string> Cats = new Map<short, string>
        {
            // agriculture 1-10
            {1, "蔬菜"},
            {2, "瓜果"},
            {3, "粮油"},
            {4, "肉蛋"},
            {5, "水产"},
            {6, "农资"},
            {9, "其它"},
            // process & service 11-20
            {11, "调养"},
            {12, "餐品"},
            {13, "家政"},
            {14, "养老"},
            {15, "公益"},
            {16, "广告"},
            {19, "其它"},
            // factory 21-30
            {21, "日用"},
            {22, "家居"},
            {23, "服装"},
            {24, "电器"},
            {29, "其它"},
        };

        public const short
            OP_INSERT = TYP | STATUS | LABEL | CREATE | BASIC,
            OP_UPDATE = STATUS | LABEL | ADAPT | BASIC;


        internal short id;
        internal short cat;
        internal string unit; // standard unit
        internal string unitip;

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
            }
        }

        public short Key => id;

        public override string ToString() => name;
    }
}