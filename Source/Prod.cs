using SkyChain;

namespace Zhnt.Supply
{
    /// 
    /// A partucular product supply.
    /// 
    public class Prod : _Art
    {
        public static readonly Prod Empty = new Prod();

        internal short id;
        internal short itemid;
        internal short srcid;
        internal short idx;

        internal short bmin;
        internal short bmax;
        internal short bstep;
        internal decimal bprice;
        internal decimal boff;
        internal int[] agts;

        internal short pmin;
        internal short pmax;
        internal short pstep;
        internal decimal pprice;
        internal decimal poff;

        internal bool img;
        internal bool testa;
        internal bool testb;


        public override void Read(ISource s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            base.Read(s, proj);

            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(srcid), ref srcid);
            s.Get(nameof(idx), ref idx);
            s.Get(nameof(bmin), ref bmin);
            s.Get(nameof(bmax), ref bmax);
            s.Get(nameof(bstep), ref bstep);
            s.Get(nameof(bprice), ref bprice);
            s.Get(nameof(boff), ref boff);
            s.Get(nameof(agts), ref agts);
            s.Get(nameof(pmin), ref pmin);
            s.Get(nameof(pmax), ref pmax);
            s.Get(nameof(pstep), ref pstep);
            s.Get(nameof(pprice), ref pprice);
            s.Get(nameof(poff), ref poff);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(img), ref img);
                s.Get(nameof(testa), ref testa);
                s.Get(nameof(testb), ref testb);
            }
        }

        public override void Write(ISink s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            base.Write(s, proj);

            s.Put(nameof(itemid), itemid);
            s.Put(nameof(srcid), srcid);
            s.Put(nameof(idx), idx);
            s.Put(nameof(bmin), bmin);
            s.Put(nameof(bmax), bmax);
            s.Put(nameof(bstep), bstep);
            s.Put(nameof(bprice), bprice);
            s.Put(nameof(boff), boff);
            s.Put(nameof(agts), agts);
            s.Put(nameof(pmin), pmin);
            s.Put(nameof(pmax), pmax);
            s.Put(nameof(pstep), pstep);
            s.Put(nameof(pprice), pprice);
            s.Put(nameof(poff), poff);
            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(img), img);
                s.Put(nameof(testa), testa);
                s.Put(nameof(testb), testb);
            }
        }
    }
}