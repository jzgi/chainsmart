using System;
using SkyChain;

namespace Revital
{
    /// <summary>
    /// The data model for a particular product supply.
    /// </summary>
    public class Product : _Art, IKeyable<int>
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


        internal int id;

        internal DateTime fillon;
        internal short postg;
        internal decimal postprice;
        internal short rank;

        public override void Read(ISource s, byte proj = 15)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            s.Get(nameof(fillon), ref fillon);
            s.Get(nameof(postg), ref postg);
            s.Get(nameof(postprice), ref postprice);
            s.Get(nameof(rank), ref rank);
        }

        public override void Write(ISink s, byte proj = 15)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(fillon), fillon);
            s.Put(nameof(postg), postg);
            s.Put(nameof(postprice), postprice);
            s.Put(nameof(rank), rank);
        }

        public int Key => id;
    }
}