using System;
using CoChain;

namespace CoSupply
{
    /// <summary>
    /// A product lot that is for transfering & selling.
    /// </summary>
    public class Distrib : Entity, IKeyable<int>
    {
        public static readonly Distrib Empty = new Distrib();

        public const short
            TYP_SELL_N_MOVE = 1, // sell and move center
            TYP_MOVE_N_SELL = 2, // move to center and sell
            TYP_TRANSFER_MOVE = 3, // transfer and move to another source
            TYP_DIRECT_SALE = 4; // direct

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_SELL_N_MOVE, "先售后运"},
            {TYP_MOVE_N_SELL, "先运后售"},
            {TYP_TRANSFER_MOVE, "转移"},
            {TYP_DIRECT_SALE, "直售直运"},
        };


        public new static readonly Map<short, string> States = new Map<short, string>
        {
            {STA_DISABLED, "停售"},
            {STA_ENABLED, "在售"},
            {STA_HOT, "冲量"},
        };

        internal int id;

        internal int productid;
        internal int srcid;
        internal string srcop;
        internal DateTime srcon;
        internal int prvid;
        internal string prvop;
        internal DateTime prvon;
        internal int ctrid;
        internal string ctrop;
        internal DateTime ctron;

        // individual order relevant

        internal int ownid;
        internal decimal price;
        internal decimal off;
        internal int cap;
        internal int remain;
        internal short min;
        internal short max;
        internal short step;
        internal short status;

        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            if ((msk & BORN) == BORN)
            {
                s.Get(nameof(productid), ref productid);
            }
            s.Get(nameof(srcid), ref srcid);
            s.Get(nameof(srcop), ref srcop);
            s.Get(nameof(srcon), ref srcon);
            s.Get(nameof(prvid), ref prvid);
            s.Get(nameof(prvop), ref prvop);
            s.Get(nameof(prvon), ref prvon);
            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(ctrop), ref ctrop);
            s.Get(nameof(ctron), ref ctron);

            s.Get(nameof(ownid), ref ownid);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(cap), ref cap);
            s.Get(nameof(remain), ref remain);

            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
            s.Get(nameof(step), ref step);

            s.Get(nameof(status), ref status);
        }

        public override void Write(ISink s, short msk = 0xff)
        {
            base.Write(s, msk);

            if ((msk & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            if ((msk & BORN) == BORN)
            {
                s.Put(nameof(productid), productid);
            }
            s.Put(nameof(srcid), srcid);
            s.Put(nameof(srcop), srcop);
            s.Put(nameof(srcon), srcon);
            s.Put(nameof(prvid), prvid);
            s.Put(nameof(prvop), prvop);
            s.Put(nameof(prvon), prvon);
            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(ctrop), ctrop);
            s.Put(nameof(ctron), ctron);

            s.Put(nameof(ownid), ownid);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(cap), cap);
            s.Put(nameof(remain), remain);

            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(step), step);

            s.Put(nameof(status), status);
        }

        public int Key => id;
    }
}