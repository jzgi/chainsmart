using SkyChain;

namespace Revital
{
    /// <summary>
    /// A retail order, for online and offline
    /// </summary>
    public class Buy : _Doc, IKeyable<int>
    {
        public static readonly Buy Empty = new Buy();

        internal int id;
        internal int postid;
        internal int itemid;
        internal int uid;
        internal string uname;
        internal string utel;
        internal string uim;
        internal decimal price;
        internal decimal pay;
        internal decimal refund;

        public override void Read(ISource s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            s.Get(nameof(postid), ref postid);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(uid), ref uid);
            s.Get(nameof(uname), ref uname);
            s.Get(nameof(utel), ref utel);
            s.Get(nameof(uim), ref uim);
            s.Get(nameof(price), ref price);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(pay), ref pay);
                s.Get(nameof(refund), ref refund);
            }
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            s.Put(nameof(postid), postid);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(uid), uid);
            s.Put(nameof(uname), uname);
            s.Put(nameof(utel), utel);
            s.Put(nameof(uim), uim);
            s.Put(nameof(price), price);
            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(pay), pay);
                s.Put(nameof(refund), refund);
            }
        }

        public int Key => id;
    }
}