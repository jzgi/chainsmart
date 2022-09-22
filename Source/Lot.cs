using System;
using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A product lot for booking.
    /// </summary>
    public class Lot : Entity, IKeyable<int>, IFlowable
    {
        public static readonly Lot Empty = new Lot();

        public const short
            STA_CREATED = 0,
            STA_OFF = 1,
            STA_PUBLISHED = 3;

        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STA_CREATED, "新创建"},
            {STA_OFF, "已下线"},
            {STA_PUBLISHED, "已发布"},
        };

        internal int id;
        internal int itemid;
        internal int srcid;
        internal int ctrid;
        internal bool ctrg; // required centering
        internal DateTime starton;
        internal DateTime endon;

        // individual order relevant
        internal decimal price;
        internal decimal off;
        internal int cap;
        internal int remain;
        internal short min;
        internal short max;
        internal short step;

        internal string oker;
        internal DateTime oked;
        internal short state;

        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(itemid), ref itemid);
                s.Get(nameof(srcid), ref srcid);
                s.Get(nameof(ctrid), ref ctrid);
                s.Get(nameof(ctrg), ref ctrg);
                s.Get(nameof(starton), ref starton);
                s.Get(nameof(endon), ref endon);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
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
                s.Get(nameof(oker), ref oker);
                s.Get(nameof(oked), ref oked);
                s.Get(nameof(state), ref state);
            }
        }

        public override void Write(ISink s, short msk = 0xff)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Put(nameof(itemid), itemid);
                s.Put(nameof(srcid), srcid);
                s.Put(nameof(ctrid), ctrid);
                s.Put(nameof(ctrg), ctrg);
                s.Put(nameof(starton), starton);
                s.Put(nameof(endon), endon);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
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
                s.Put(nameof(oker), oker);
                s.Put(nameof(oked), oked);
                s.Put(nameof(state), state);
            }
        }

        public int Key => id;

        public override string ToString() => name;

        public string Oker => oker;

        public DateTime Oked => oked;

        public short State => state;
    }
}