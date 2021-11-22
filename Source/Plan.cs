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
            FIL_ONE = 1,
            FIL_TWO = 2,
            FIL_THREE = 2,
            FIL_FAR = 7;

        public static readonly Map<short, string> Fillgs = new Map<short, string>
        {
            {FIL_ONE, "当日交付"},
            {FIL_TWO, "两日内交付"},
            {FIL_THREE, "三日内交付"},
            {FIL_FAR, "远期交付"},
        };

        internal int id;

        internal int productid;
        internal DateTime starton;
        internal DateTime endon;
        internal short fillg;
        internal DateTime fillon;

        internal string dunit; // downstream
        internal short dunitx;
        internal short dmin;
        internal short dmax;
        internal short dstep;
        internal decimal dprice;
        internal decimal doff;

        public override void Read(ISource s, byte proj = 15)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            s.Get(nameof(productid), ref productid);
            s.Get(nameof(starton), ref starton);
            s.Get(nameof(endon), ref endon);
            s.Get(nameof(fillg), ref fillg);
            s.Get(nameof(fillon), ref fillon);

            s.Get(nameof(dunit), ref dunit);
            s.Get(nameof(dunitx), ref dunitx);
            s.Get(nameof(dmin), ref dmin);
            s.Get(nameof(dmax), ref dmax);
            s.Get(nameof(dstep), ref dstep);
            s.Get(nameof(dprice), ref dprice);
            s.Get(nameof(doff), ref doff);
        }

        public override void Write(ISink s, byte proj = 15)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(productid), productid);
            s.Put(nameof(starton), starton);
            s.Put(nameof(endon), endon);
            s.Put(nameof(fillg), fillg);
            s.Put(nameof(fillon), fillon);

            s.Put(nameof(dunit), dunit);
            s.Put(nameof(dunitx), dunitx);
            s.Put(nameof(dmin), dmin);
            s.Put(nameof(dmax), dmax);
            s.Put(nameof(dstep), dstep);
            s.Put(nameof(dprice), dprice);
            s.Put(nameof(doff), doff);
        }

        public int Key => id;
    }
}