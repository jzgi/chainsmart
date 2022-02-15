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

        public static readonly Map<short, string> Mrtgs = new Map<short, string>
        {
            {0, "不管市场价"},
            {1, "建议市场价"},
            {2, "市场价上限"},
            {3, "市场价下限"},
            {4, "统一市场价"},
        };

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