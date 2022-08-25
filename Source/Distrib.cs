using System;
using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A product lot that is for transfering & selling.
    /// </summary>
    public class Distrib : Entity, IKeyable<int>
    {
        public static readonly Distrib Empty = new Distrib();

        public const short
            TYP_CTR = 1,
            TYP_SELF = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_CTR, "中控批（运往指定的中控做品控分发）"},
            {TYP_SELF, "自达批（自行发货到市场商户）"},
        };


        internal int id;

        internal int productid;
        internal int srcid;
        internal int prvid;
        internal int ctrid;

        // individual order relevant

        internal decimal price;
        internal decimal off;
        internal int cap;
        internal int remain;
        internal short min;
        internal short max;
        internal short step;

        public override void Read(ISource s, short msk = 0xff)
        {
            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }

            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(typ), ref typ);
                s.Get(nameof(name), ref name);
                s.Get(nameof(tip), ref tip);
                s.Get(nameof(created), ref created);
                s.Get(nameof(creator), ref creator);
                s.Get(nameof(productid), ref productid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(status), ref status);

                s.Get(nameof(price), ref price);
                s.Get(nameof(off), ref off);
                s.Get(nameof(min), ref min);
                s.Get(nameof(max), ref max);
                s.Get(nameof(step), ref step);
                s.Get(nameof(cap), ref cap);
                s.Get(nameof(remain), ref remain);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(adapted), ref adapted);
                s.Get(nameof(adapter), ref adapter);
            }
            if ((msk & MSK_PROCESS) == MSK_PROCESS)
            {
                s.Get(nameof(srcid), ref srcid);
                s.Get(nameof(prvid), ref prvid);
                s.Get(nameof(ctrid), ref ctrid);
            }
        }

        public override void Write(ISink s, short msk = 0xff)
        {
            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }

            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Put(nameof(typ), typ);
                s.Put(nameof(name), name);
                s.Put(nameof(tip), tip);
                s.Put(nameof(created), created);
                s.Put(nameof(creator), creator);
                s.Put(nameof(productid), productid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(status), status);

                s.Put(nameof(price), price);
                s.Put(nameof(off), off);
                s.Put(nameof(min), min);
                s.Put(nameof(max), max);
                s.Put(nameof(step), step);
                s.Put(nameof(cap), cap);
                s.Put(nameof(remain), remain);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(adapted), adapted);
                s.Put(nameof(adapter), adapter);
            }
            if ((msk & MSK_PROCESS) == MSK_PROCESS)
            {
                s.Put(nameof(srcid), srcid);
                s.Put(nameof(prvid), prvid);
                s.Put(nameof(ctrid), ctrid);
            }
        }

        public int Key => id;
    }
}