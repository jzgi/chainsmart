using System;
using SkyChain;

namespace Revital
{
    public class Book : _Deal, IKeyable<long>
    {
        public static readonly Book Empty = new Book();

        public const short
            TYP_SPOT = 1,
            TYP_PRESALE = 2;


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_SPOT, "现货"},
            {TYP_PRESALE, "预售"},
        };


        internal long id;
        internal int bizid;
        internal string bizname;
        internal int mrtid;
        internal string mrtname;
        internal int ctrid;
        internal string ctrname;
        internal int prvid;
        internal string prvname;
        internal int srcid;
        internal string srcname;

        internal short peerid_;
        internal long coid_;
        internal long seq_;
        internal string cs_;
        internal string blockcs_;

        public override void Read(ISource s, short proj = 0x0fff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(bizid), ref bizid);
            s.Get(nameof(bizname), ref bizname);
            s.Get(nameof(mrtid), ref mrtid);
            s.Get(nameof(mrtname), ref mrtname);
            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(ctrname), ref ctrname);
            s.Get(nameof(prvid), ref prvid);
            s.Get(nameof(prvname), ref prvname);
            s.Get(nameof(srcid), ref srcid);
            s.Get(nameof(srcname), ref srcname);

            s.Get(nameof(peerid_), ref peerid_);
            s.Get(nameof(coid_), ref coid_);
            s.Get(nameof(seq_), ref seq_);
            s.Get(nameof(cs_), ref cs_);
            s.Get(nameof(blockcs_), ref blockcs_);
        }

        public override void Write(ISink s, short proj = 0x0fff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(bizid), bizid);
            s.Put(nameof(bizname), bizname);
            s.Put(nameof(mrtid), mrtid);
            s.Put(nameof(mrtname), mrtname);
            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(ctrname), ctrname);
            s.Put(nameof(prvid), prvid);
            s.Put(nameof(prvname), prvname);
            s.Put(nameof(srcid), srcid);
            s.Put(nameof(srcname), srcname);

            s.Put(nameof(peerid_), peerid_);
            s.Put(nameof(coid_), coid_);
            s.Put(nameof(seq_), seq_);
            s.Put(nameof(cs_), cs_);
            s.Put(nameof(blockcs_), blockcs_);
        }

        public long Key => id;

        public bool IsOver(DateTime now) => false;
    }
}