using System;
using SkyChain;

namespace Revital
{
    /// <summary>
    /// The data model for a particular supply of standard item.
    /// </summary>
    public class Plan : _Ware, IKeyable<int>
    {
        public static readonly Plan Empty = new Plan();

        public const short
            TYP_ONE = 1,
            TYP_TWO = 2,
            TYP_THREE = 3,
            TYP_FAR = 7;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_ONE, "（常规）当日交付"},
            {TYP_TWO, "（常规）两日内交付"},
            {TYP_THREE, "（常规）三日内交付"},
            {TYP_FAR, "（远期）指定交付日"},
        };

        public static readonly Map<short, string> Postgs = new Map<short, string>
        {
            {0, "不限市场价"},
            {1, "建议市场价"},
            {2, "市场价上限"},
            {3, "市场价下限"},
            {4, "统一市场价"},
        };

        internal int id;

        internal int pieceid;
        internal DateTime starton;
        internal DateTime endon;
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

            s.Get(nameof(pieceid), ref pieceid);
            s.Get(nameof(starton), ref starton);
            s.Get(nameof(endon), ref endon);
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
            s.Put(nameof(pieceid), pieceid);
            s.Put(nameof(starton), starton);
            s.Put(nameof(endon), endon);
            s.Put(nameof(fillon), fillon);
            s.Put(nameof(postg), postg);
            s.Put(nameof(postprice), postprice);
            s.Put(nameof(rank), rank);
        }

        public int Key => id;
    }
}