using SkyChain;

namespace Revital
{
    /// <summary>
    /// The data modal for an standard item of product or service.
    /// </summary>
    public class Item : _Art, IKeyable<short>
    {
        public static readonly Item Empty = new Item();

        public const short
            TYP_AGRI = 0b00000001, // agriculture
            TYP_FACT = 0b00000010, // factory
            TYP_SRVC = 0b00000100; // service


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_AGRI, "农副产"},
            {TYP_FACT, "制造品"},
            {TYP_SRVC, "泛服务"},
        };

        // categories
        public static readonly Map<short, string> Cats = new Map<short, string>
        {
            // agriculture 1-20
            {1, "蔬菜"},
            {2, "瓜果"},
            {3, "粮油"},
            {4, "绿植"},
            {5, "禽产"},
            {6, "畜产"},
            {7, "水产"},
            {8, "副产"},
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


        internal short id;
        internal short cat;
        internal string unit; // standard unit
        internal string unitip;
        internal decimal unitfee;

        // must have an icon

        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(cat), ref cat);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitip), ref unitip);
            s.Get(nameof(unitfee), ref unitfee);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(cat), cat);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitip), unitip);
            s.Put(nameof(unitfee), unitfee);
        }

        public short Key => id;

        public override string ToString() => name;
    }
}