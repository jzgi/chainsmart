﻿using CoChain;

namespace CoBiz
{
    /// <summary>
    /// A geographic or spatial region.
    /// </summary>
    public class Reg : Entity, IKeyable<short>, IDirectory
    {
        public static readonly Reg Empty = new Reg();

        public const short
            TYP_PROVINCE = 1,
            TYP_CITY = 2,
            TYP_SECTION = 3;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_PROVINCE, "省份"},
            {TYP_CITY, "地市"},
            {TYP_SECTION, "分区"},
        };

        internal short id;

        internal short idx;

        internal short num; // number of sub-resources

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(idx), ref idx);
            s.Get(nameof(num), ref num);
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(idx), idx);
            s.Put(nameof(num), num);
        }

        public short Key => id;

        public short Idx => idx;

        public short Num => num;

        public bool IsProvince => typ == TYP_PROVINCE;

        public bool IsCity => typ == TYP_CITY;

        public bool IsSection => typ == TYP_SECTION;

        public override string ToString() => name;
    }
}