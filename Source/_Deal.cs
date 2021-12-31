using System;
using SkyChain;

namespace Revital
{
    /// <summary>
    /// A data model for workflow document.
    /// </summary>
    public abstract class _Deal : _Info
    {
        public const short
            STA_CREATED = 1,
            STA_ABORTED = 2,
            STA_ADAPTED = 3,
            STA_HANDLED = 4;

        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {0, null},
            {STA_CREATED, "已提交"},
            {STA_ABORTED, "已撤销"},
            {STA_ADAPTED, "已接受"},
            {STA_HANDLED, "已处理"},
        };

        internal short itemid;
        internal int artid;
        internal int artname;
        internal DateTime handled;
        internal string handler;
        internal decimal price;
        internal int qty;
        internal decimal pay;
        internal decimal refund;


        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);

            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(artid), ref artid);
            s.Get(nameof(artname), ref artname);
            s.Get(nameof(handled), ref handled);
            s.Get(nameof(handler), ref handler);
            s.Get(nameof(price), ref price);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(pay), ref pay);
            s.Get(nameof(refund), ref refund);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            base.Write(s, proj);

            s.Put(nameof(itemid), itemid);
            s.Put(nameof(artid), artid);
            s.Put(nameof(artname), artname);
            s.Put(nameof(handled), handled);
            s.Put(nameof(handler), handler);
            s.Put(nameof(price), price);
            s.Put(nameof(qty), qty);
            s.Put(nameof(pay), pay);
            s.Put(nameof(refund), refund);
        }
    }
}