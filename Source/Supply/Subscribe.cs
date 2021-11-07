using System;
using SkyChain;

namespace Revital.Supply
{
    /// 
    /// A upstream line of purchase.
    /// 
    public class Subscribe : IData, IKeyable<int>
    {
        public static readonly Subscribe Empty = new Subscribe();

        public const byte ID = 1, LATER = 2;

        public const short
            TYP_PRODUCT = 1,
            TYP_SERVICE = 2,
            TYP_EVENT = 3;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {1, "现货"},
            {2, "预订"},
        };

        public const short
            STATUS_CREATED = 0,
            STATUS_SUBMITTED = 1, // before processing
            STATUS_ABORTED = 2,
            STATUS_CONFIRMED = 3, // ready for distr center op 
            STATUS_SHIPPED = 4, //  
            STATUS_CLOSED = 5; // after clearing

        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STATUS_CREATED, "草稿中"},
            {STATUS_SUBMITTED, "已提交"},
            {STATUS_ABORTED, "已撤销"},
            {STATUS_CONFIRMED, "已确认"},
            {STATUS_SHIPPED, "已发货"},
            {STATUS_CLOSED, "已关闭"},
        };


        internal short typ;
        internal short status;
        internal short partyid;
        internal short ctrid;
        internal DateTime created;
        internal string creator;
        internal DateTime traded;
        internal string trader;
        internal DateTime settled;
        internal string settler;


        internal int id;

        // doc number
        internal int no;
        internal short prodid;
        internal short itemid;
        internal decimal price;
        internal decimal off;
        internal int qty;
        internal decimal pay;
        internal decimal refund;
        internal int codestart;
        internal short codes;

        public void Read(ISource s, byte proj = 15)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(partyid), ref partyid);
            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(created), ref created);
            s.Get(nameof(creator), ref creator);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(no), ref no);
            s.Get(nameof(prodid), ref prodid);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(pay), ref pay);
            s.Get(nameof(refund), ref refund);
            s.Get(nameof(codestart), ref codestart);
            s.Get(nameof(codes), ref codes);
        }

        public void Write(ISink s, byte proj = 15)
        {
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);
            s.Put(nameof(partyid), partyid);
            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(created), created);
            s.Put(nameof(creator), creator);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(no), no);
            s.Put(nameof(prodid), prodid);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(qty), qty);
            s.Put(nameof(pay), pay);
            s.Put(nameof(refund), refund);
            s.Put(nameof(codestart), codestart);
            s.Put(nameof(codes), codes);
        }

        public int Key => id;
    }
}