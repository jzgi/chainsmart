using System;
using SkyChain;

namespace Supply
{
    /// 
    /// A partucular supply.plan
    /// 
    public class Plan : Art_
    {
        public static readonly Plan Empty = new Plan();

        public static readonly Map<short, string> Schemes = new Map<short, string>
        {
            {1, "现货"},
            {2, "预售"},
        };

        internal short id;
        internal short itemid;
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


        public override void Read(ISource s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            base.Read(s, proj);

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

        public override void Write(ISink s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            base.Write(s, proj);

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
    }
}