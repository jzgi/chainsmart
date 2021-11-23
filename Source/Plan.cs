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

        public static readonly Map<short, string> Finalgs = new Map<short, string>
        {
            {0, "不作规定"},
            {1, "提供建议"},
            {2, "强制参数"},
        };

        internal int id;

        internal int productid;
        internal DateTime starton;
        internal DateTime endon;
        internal short fillg;
        internal DateTime fillon;

        internal short finalg;
        internal string funit; // downstream
        internal short funitx;
        internal short fmin;
        internal short fmax;
        internal short fstep;
        internal decimal fprice;
        internal decimal foff;

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

            s.Get(nameof(finalg), ref finalg);
            s.Get(nameof(funit), ref funit);
            s.Get(nameof(funitx), ref funitx);
            s.Get(nameof(fmin), ref fmin);
            s.Get(nameof(fmax), ref fmax);
            s.Get(nameof(fstep), ref fstep);
            s.Get(nameof(fprice), ref fprice);
            s.Get(nameof(foff), ref foff);
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

            s.Put(nameof(finalg), finalg);
            s.Put(nameof(funit), funit);
            s.Put(nameof(funitx), funitx);
            s.Put(nameof(fmin), fmin);
            s.Put(nameof(fmax), fmax);
            s.Put(nameof(fstep), fstep);
            s.Put(nameof(fprice), fprice);
            s.Put(nameof(foff), foff);
        }

        public int Key => id;
    }
}