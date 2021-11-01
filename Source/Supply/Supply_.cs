using System;
using SkyChain;

namespace Revital.Supply
{
    /// <summary>
    /// The data model for a particular supply of standard item.
    /// </summary>
    public class Supply_ : _Bean, IKeyable<int>
    {
        public static readonly Supply_ Empty = new Supply_();

        public static readonly Map<short, string> Modes = new Map<short, string>
        {
            {1, "现货"},
            {2, "预售"},
        };

        internal int id;
        internal int ctrid;
        internal int itemid;

        internal DateTime started;
        internal DateTime ended;
        internal DateTime filled;

        internal short rmode; // retail mode
        internal string runit;
        internal short rx;
        internal short rmin;
        internal short rmax;
        internal short rstep;
        internal decimal rprice;
        internal decimal roff;

        internal short tmode; // transfer mode
        internal string tunit;
        internal short tx;
        internal short tmin;
        internal short tmax;
        internal short tstep;
        internal decimal tprice;
        internal decimal toff;

        internal short gmode; // gain mode
        internal string gunit;
        internal short gx;
        internal short gmin;
        internal short gmax;
        internal short gstep;
        internal decimal gprice;
        internal decimal goff;

        public override void Read(ISource s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(started), ref started);
            s.Get(nameof(ended), ref ended);
            s.Get(nameof(filled), ref filled);

            s.Get(nameof(rmode), ref rmode);
            s.Get(nameof(runit), ref runit);
            s.Get(nameof(rx), ref rx);
            s.Get(nameof(rmin), ref rmin);
            s.Get(nameof(rmax), ref rmax);
            s.Get(nameof(rstep), ref rstep);
            s.Get(nameof(rprice), ref rprice);
            s.Get(nameof(roff), ref roff);

            s.Get(nameof(tmode), ref tmode);
            s.Get(nameof(tunit), ref tunit);
            s.Get(nameof(tx), ref tx);
            s.Get(nameof(tmin), ref tmin);
            s.Get(nameof(tmax), ref tmax);
            s.Get(nameof(tstep), ref tstep);
            s.Get(nameof(tprice), ref tprice);
            s.Get(nameof(toff), ref toff);

            s.Get(nameof(gmode), ref gmode);
            s.Get(nameof(gunit), ref gunit);
            s.Get(nameof(gx), ref gx);
            s.Get(nameof(gmin), ref gmin);
            s.Get(nameof(gmax), ref gmax);
            s.Get(nameof(gstep), ref gstep);
            s.Get(nameof(gprice), ref gprice);
            s.Get(nameof(goff), ref goff);
        }

        public override void Write(ISink s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(started), started);
            s.Put(nameof(ended), ended);
            s.Put(nameof(filled), filled);

            s.Put(nameof(rmode), rmode);
            s.Put(nameof(runit), runit);
            s.Put(nameof(rx), rx);
            s.Put(nameof(rmin), rmin);
            s.Put(nameof(rmax), rmax);
            s.Put(nameof(rstep), rstep);
            s.Put(nameof(rprice), rprice);
            s.Put(nameof(roff), roff);

            s.Put(nameof(tmode), tmode);
            s.Put(nameof(tunit), tunit);
            s.Put(nameof(tx), tx);
            s.Put(nameof(tmin), tmin);
            s.Put(nameof(tmax), tmax);
            s.Put(nameof(tstep), tstep);
            s.Put(nameof(tprice), tprice);
            s.Put(nameof(toff), toff);

            s.Put(nameof(gmode), gmode);
            s.Put(nameof(gunit), gunit);
            s.Put(nameof(gx), gx);
            s.Put(nameof(gmin), gmin);
            s.Put(nameof(gmax), gmax);
            s.Put(nameof(gstep), gstep);
            s.Put(nameof(gprice), gprice);
            s.Put(nameof(goff), goff);
        }

        public int Key => id;
    }
}