using System;
using SkyChain;

namespace Revital.Supply
{
    /// <summary>
    /// The data model for a particular supply of standard item.
    /// </summary>
    public class Supply_ : IData, IKeyable<int>
    {
        public static readonly Supply_ Empty = new Supply_();

        public static readonly Map<short, string> Schemes = new Map<short, string>
        {
            {1, "现货"},
            {2, "预售"},
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

        internal int id;
        internal int itemid;
        
        internal string runit;
        internal short runitx;
        internal short rmin;
        internal short rmax;
        internal short rstep;
        internal decimal rprice;
        internal decimal roff;

        internal string wunit;
        internal short wunitx;
        internal short wmin;
        internal short wmax;
        internal short wstep;
        internal decimal wprice;
        internal decimal woff;

        internal DateTime started;
        internal DateTime ended;
        internal DateTime delivered;
        internal string punit;
        internal short punitx;
        internal decimal pprice;
        internal decimal poff;


        public void Read(ISource s, byte proj = 15)
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
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(runit), ref runit);
            s.Get(nameof(runitx), ref runitx);
            s.Get(nameof(rmin), ref rmin);
            s.Get(nameof(rmax), ref rmax);
            s.Get(nameof(rstep), ref rstep);
            s.Get(nameof(rprice), ref rprice);
            s.Get(nameof(roff), ref roff);
            s.Get(nameof(started), ref started);
            s.Get(nameof(ended), ref ended);
            s.Get(nameof(delivered), ref delivered);
            s.Get(nameof(pprice), ref pprice);
            s.Get(nameof(poff), ref poff);
        }

        public void Write(ISink s, byte proj = 15)
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
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(runit), runit);
            s.Put(nameof(runitx), runitx);

            s.Put(nameof(rmin), rmin);
            s.Put(nameof(rmax), rmax);
            s.Put(nameof(rstep), rstep);
            s.Put(nameof(rprice), rprice);
            s.Put(nameof(roff), roff);
            s.Put(nameof(started), started);
            s.Put(nameof(ended), ended);
            s.Put(nameof(delivered), delivered);
            s.Put(nameof(pprice), pprice);
            s.Put(nameof(poff), poff);
        }

        public int Key => id;
    }
}