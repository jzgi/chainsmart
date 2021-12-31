using System;
using SkyChain;

namespace Revital
{
    public class Book : _Deal, IKeyable<long>
    {
        public static readonly Book Empty = new Book();

        public const short
            TYP_PRODUCT = 1,
            TYP_SERVICE = 2,
            TYP_EVENT = 3;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {1, "现货"},
            {2, "预订"},
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

        internal long seq_;
        internal string cs_;
        internal string blockcs_;
        internal short peer_;
        internal long rec_;

        public override void Read(ISource s, byte proj = 15)
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

            s.Get(nameof(seq_), ref seq_);
            s.Get(nameof(cs_), ref cs_);
            s.Get(nameof(blockcs_), ref blockcs_);
            s.Get(nameof(peer_), ref peer_);
            s.Get(nameof(rec_), ref rec_);
        }

        public override void Write(ISink s, byte proj = 15)
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

            s.Put(nameof(seq_), seq_);
            s.Put(nameof(cs_), cs_);
            s.Put(nameof(blockcs_), blockcs_);
            s.Put(nameof(peer_), peer_);
            s.Put(nameof(rec_), rec_);
        }

        public long Key => id;

        public bool IsOver(DateTime now) => false;
    }
}