using System;
using SkyChain;

namespace Rev.Supply
{
    /// <summary>
    /// The data model for a particular supply of standard item.
    /// </summary>
    public class Supply : IData, IKeyable<int>
    {
        public static readonly Supply Empty = new Supply();

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
        internal string bunit;
        internal short bunitx;
        internal short bmin;
        internal short bmax;
        internal short bstep;
        internal decimal bprice;
        internal decimal boff;

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
            s.Get(nameof(bunit), ref bunit);
            s.Get(nameof(bunitx), ref bunitx);
            s.Get(nameof(bmin), ref bmin);
            s.Get(nameof(bmax), ref bmax);
            s.Get(nameof(bstep), ref bstep);
            s.Get(nameof(bprice), ref bprice);
            s.Get(nameof(boff), ref boff);
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
            s.Put(nameof(bunit), bunit);
            s.Put(nameof(bunitx), bunitx);

            s.Put(nameof(bmin), bmin);
            s.Put(nameof(bmax), bmax);
            s.Put(nameof(bstep), bstep);
            s.Put(nameof(bprice), bprice);
            s.Put(nameof(boff), boff);
            s.Put(nameof(started), started);
            s.Put(nameof(ended), ended);
            s.Put(nameof(delivered), delivered);
            s.Put(nameof(pprice), pprice);
            s.Put(nameof(poff), poff);
        }

        public int Key => id;
    }
}