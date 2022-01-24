using SkyChain;

namespace Revital
{
    /// <summary>
    /// An online or offline retail order
    /// </summary>
    public class Buy : _Info, IKeyable<long>
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
        internal int bizid;
        internal string bizname;
        internal int mrtid;
        internal decimal mrtname;
        internal int uid;
        internal string uname;
        internal string utel;
        internal string uim;
        internal decimal totalp;
        internal decimal fee;
        internal decimal pay;
        internal BuyWare[] wares;

        public override void Read(ISource s, short proj = 0x0fff)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(bizid), ref bizid);
            s.Get(nameof(bizname), ref bizname);
            s.Get(nameof(mrtid), ref mrtid);
            s.Get(nameof(mrtname), ref mrtname);
            s.Get(nameof(uid), ref uid);
            s.Get(nameof(uname), ref uname);
            s.Get(nameof(utel), ref utel);
            s.Get(nameof(uim), ref uim);
            s.Get(nameof(totalp), ref totalp);
            s.Get(nameof(fee), ref fee);
            s.Get(nameof(pay), ref pay);
            s.Get(nameof(wares), ref wares);
        }

        public override void Write(ISink s, short proj = 0x0fff)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            s.Put(nameof(bizid), bizid);
            s.Put(nameof(bizname), bizname);
            s.Put(nameof(mrtid), mrtid);
            s.Put(nameof(mrtname), mrtname);
            s.Put(nameof(uid), uid);
            s.Put(nameof(uname), uname);
            s.Put(nameof(utel), utel);
            s.Put(nameof(uim), uim);
            s.Put(nameof(totalp), totalp);
            s.Put(nameof(fee), fee);
            s.Put(nameof(pay), pay);
            s.Put(nameof(wares), wares);
        }

        public long Key => id;
    }
}