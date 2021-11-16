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
            TYP_ROUTINE = 1,
            TYP_FUTURE = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_ROUTINE, "常规"},
            {TYP_FUTURE, "预先"},
        };

        internal int id;

        internal int productid;
        internal DateTime started;
        internal DateTime ended;
        internal DateTime filled;

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
            s.Get(nameof(started), ref started);
            s.Get(nameof(ended), ref ended);
            s.Get(nameof(filled), ref filled);

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
            s.Put(nameof(started), started);
            s.Put(nameof(ended), ended);
            s.Put(nameof(filled), filled);

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