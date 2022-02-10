using System;
using SkyChain;

namespace Revital
{
    /// <summary>
    /// The data model for a particular product supply.
    /// </summary>
    public class Product : _Ware, IKeyable<int>
    {
        public static readonly Product Empty = new Product();

        public const short
            TYP_SPOT = 1,
            TYP_FUTURE = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_SPOT, "现货供应"},
            {TYP_FUTURE, "预售供应"},
        };

        public static readonly Map<short, string> Postgs = new Map<short, string>
        {
            {0, "不管市场价"},
            {1, "建议市场价"},
            {2, "市场价上限"},
            {3, "市场价下限"},
            {4, "统一市场价"},
        };

        public const short
            INSERT = TYP | STATUS | LABEL | CREATE | ID | BASIC,
            UPDATE = STATUS | LABEL | ADAPT | BASIC;


        internal int id;

        internal DateTime fillon;
        internal short mrtg;
        internal decimal mrtprice;
        internal short rankg;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            s.Get(nameof(fillon), ref fillon);
            s.Get(nameof(mrtg), ref mrtg);
            s.Get(nameof(mrtprice), ref mrtprice);
            s.Get(nameof(rankg), ref rankg);
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(fillon), fillon);
            s.Put(nameof(mrtg), mrtg);
            s.Put(nameof(mrtprice), mrtprice);
            s.Put(nameof(rankg), rankg);
        }

        public int Key => id;
    }
}