using SkyChain;

namespace Revital
{
    /// <summary>
    /// The data modal for an standard item of product or service.
    /// </summary>
    public class Item : _Doc, IKeyable<short>
    {
        public static readonly Item Empty = new Item();

        // types
        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            // agri
            {1, "蔬菜"},
            {2, "瓜果"},
            {3, "粮油"},
            {5, "禽产"},
            {6, "畜产"},
            {7, "水产"},
            // dietary
            {11, "健康管理"},
            {12, "餐料"},
            // factory
            {21, "家居厨具"},
            {22, "农资绿植"},
            // care
            {41, "家政"},
            {42, "陪护"},
            // ad
            {61, "广告"},
            {62, "宣传"},
            // charity
            {71, "志愿服务"},
            {72, "公益活动"},
        };


        internal short id;
        internal string unit; // basic unit
        internal string unitip;

        // must have an icon

        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitip), ref unitip);
        }

        public override void Write(ISink s, byte proj = 0x0f)
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