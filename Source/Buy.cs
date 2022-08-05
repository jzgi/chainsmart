using CoChain;

namespace Revital
{
    /// <summary>
    /// An online or offline retail order
    /// </summary>
    public class Buy : Entity, IKeyable<long>, IFlowable
    {
        public static readonly Buy Empty = new Buy();

        public const short
            TYP_ONLINE = 1,
            TYP_OFFLINE = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_ONLINE, "网上"},
            {TYP_OFFLINE, "线下"},
        };

        internal long id;
        internal int shpid;
        internal int mrtid;
        internal int uid;
        internal string uname;
        internal string utel;
        internal string uaddr;
        internal string uim;
        internal BuyLn[] lns;
        internal decimal pay;
        internal decimal payre; // pay refunded
        internal short status;

        public override void Read(ISource s, short proj = 0xff)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(shpid), ref shpid);
            s.Get(nameof(mrtid), ref mrtid);
            s.Get(nameof(uid), ref uid);
            s.Get(nameof(uname), ref uname);
            s.Get(nameof(utel), ref utel);
            s.Get(nameof(uaddr), ref uaddr);
            s.Get(nameof(uim), ref uim);
            s.Get(nameof(lns), ref lns);
            s.Get(nameof(pay), ref pay);
            s.Get(nameof(payre), ref payre);
            s.Get(nameof(status), ref status);
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(shpid), shpid);
            s.Put(nameof(mrtid), mrtid);
            s.Put(nameof(uid), uid);
            s.Put(nameof(uname), uname);
            s.Put(nameof(utel), utel);
            s.Put(nameof(uaddr), uaddr);
            s.Put(nameof(uim), uim);
            s.Put(nameof(lns), lns);
            s.Put(nameof(pay), pay);
            s.Put(nameof(payre), payre);
            s.Put(nameof(status), status);
        }

        public long Key => id;

        public short Status => status;
    }
}