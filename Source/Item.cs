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
            TYP_DIET = 0b00000010, // dietary
            TYP_FACT = 0b00000100, // factory
            TYP_CARE = 0b00001000, // care
            TYP_CHAR = 0b00010000, // charity
            TYP_ADVT = 0b00100000; // advertising


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_AGRI, "农副产品"},
            {TYP_DIET, "调养饮食"},
            {TYP_FACT, "工业制品"},
            {TYP_CARE, "家政医养"},
            {TYP_CHAR, "公益志愿"},
            {TYP_ADVT, "广告宣传"},
            // composition
            {TYP_AGRI + TYP_DIET, "农副产品＋调养饮食"},
        };


        // types
        public static readonly Map<short, string> Cats = new Map<short, string>
        {
            // agri
            {11, "蔬菜"},
            {12, "瓜果"},
            {13, "粮油"},
            {14, "绿植"},
            {15, "禽产"},
            {16, "畜产"},
            {17, "水产"},
            {18, "副产"},
            {19, "农资"},
            // dietary
            {21, "调养"},
            {22, "餐品"},
            {23, "快餐"},
            // factory
            {31, "日用"},
            {31, "家居"},
            {31, "服装"},
            {31, "电器"},
            // care
            {41, "家政"},
            {42, "陪护"},
            {43, "养老"},
            // charity
            {51, "公益"},
            {52, "志愿"},
            // ad
            {61, "广告"},
            {62, "宣传"},
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