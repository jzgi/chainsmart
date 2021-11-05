using SkyChain;

namespace Revital.Shop
{
    public class Offer : IData, IKeyable<short>
    {
        public static readonly Offer Empty = new Offer();

        // status
        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {1, "谷物"},
            {2, "薯类"},
            {3, "本草"},
            {4, "蔬菜"},
            {5, "菌藻"},
            {6, "水果"},
            // protain foods
            {7, "坚果"},
            {8, "豆制品"},
            {9, "鱼蜜奶蛋"},
            {10, "油盐糖"},
        };

        public const byte ID = 1, LATER = 4;

        internal short id;
        internal string unit;
        internal short calory;
        internal decimal carb;
        internal decimal fat;
        internal decimal protein;
        internal decimal sugar;
        internal decimal vitamin;
        internal short mineral;

        public void Read(ISource s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            s.Get(nameof(unit), ref unit);
            s.Get(nameof(calory), ref calory);
            s.Get(nameof(carb), ref carb);
            s.Get(nameof(fat), ref fat);
            s.Get(nameof(protein), ref protein);
            s.Get(nameof(sugar), ref sugar);
            s.Get(nameof(vitamin), ref vitamin);
            s.Get(nameof(mineral), ref mineral);
        }

        public void Write(ISink s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            s.Put(nameof(unit), unit);
            s.Put(nameof(calory), calory);
            s.Put(nameof(carb), carb);
            s.Put(nameof(fat), fat);
            s.Put(nameof(protein), protein);
            s.Put(nameof(sugar), sugar);
            s.Put(nameof(vitamin), vitamin);
            s.Put(nameof(mineral), mineral);
        }

        public short Key => id;
    }
}