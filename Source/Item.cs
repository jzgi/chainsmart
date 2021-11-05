using System;
using SkyChain;

namespace Revital
{
    /// <summary>
    /// The data modal for an standard item of product or service.
    /// </summary>
    public class Item : IData, IKeyable<short>
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
            // agri
            {1, "瓜果"},
            {2, "蔬菜"},
            {3, "粮油"},
            {4, "泛种植"},
            {5, "禽类"},
            {6, "畜类"},
            {7, "水产"},
            {8, "泛养殖"},
            // dietary
            {11, "健康管理"},
            {12, "餐料"},
            // home
            {21, "家政"},
            {22, "陪护"},
            {23, "婚介"},
            // post
            {31, "包裹快递"},
            // ad
            {41, "广告"},
            {42, "宣传"},
        };

        public const short
            STA_DISABLED = 0,
            STA_SHOWED = 1,
            STA_ENABLED = 2,
            STA_PREFERED = 3;

        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STA_DISABLED, "禁用"},
            {STA_SHOWED, "展示"},
            {STA_ENABLED, "可用"},
            {STA_PREFERED, "优先"},
        };

        public const byte ID = 1, LATER = 2, PRIVACY = 4;

        internal short typ;
        internal short status;
        internal string name;
        internal string tip;
        internal DateTime created;
        internal string creator;

        internal short id;
        internal string unit; // basic unit
        internal string unitip;

        // must have an icon

        public void Read(ISource s, byte proj = 0x0f)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(name), ref name);
            s.Get(nameof(tip), ref tip);
            s.Get(nameof(created), ref created);
            s.Get(nameof(creator), ref creator);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitip), ref unitip);
        }

        public void Write(ISink s, byte proj = 0x0f)
        {
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);
            s.Put(nameof(name), name);
            s.Put(nameof(tip), tip);
            s.Put(nameof(created), created);
            s.Put(nameof(creator), creator);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitip), unitip);
        }

        public short Key => id;

        public bool IsFor(short prog) => false;

        public override string ToString() => name;
    }
}