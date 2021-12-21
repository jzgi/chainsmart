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
            TYP_SRVC = 0b00001000; // service


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_AGRI, "农副产"},
            {TYP_FACT, "制造品"},
            {TYP_SRVC, "服务类"},
            {TYP_AGRI + TYP_SRVC, "农副产＋服务类"},
        };

        // categories
        public static readonly Map<short, string> Cats = new Map<short, string>
        {
            // agriculture
            {11, "蔬菜"},
            {12, "瓜果"},
            {13, "粮油"},
            {14, "绿植"},
            {15, "禽产"},
            {16, "畜产"},
            {17, "水产"},
            {18, "副产"},
            // factory
            {31, "日用"},
            {32, "家居"},
            {33, "服装"},
            {34, "电器"},
            {35, "农资"},
            // service
            {51, "调养"},
            {52, "餐品"},
            {53, "快餐"},
            {54, "家政"},
            {55, "陪护"},
            {56, "养老"},
            {57, "公益"},
            {58, "志愿"},
            {59, "广告"}
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