using SkyChain;
using Zhnt;

namespace Zhnt.Mart
{
    /// <summary>
    /// A business division in market.
    /// </summary>
    public class Biz : _Art, IKeyable<short>
    {
        public static readonly Biz Empty = new Biz();

        public const short
            TYP_MART = 1,
            TYP_BOOTH = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_MART, "市场"},
            {TYP_BOOTH, "摊位"},
        };

        internal short id;
        internal short regid;

        // the host market (for booth)
        internal short parent;

        internal string addr;
        internal double x;
        internal double y;

        internal bool @extern;

        internal int mgrid;
        internal string mgrname;
        internal string mgrtel;
        internal string mgrim;
        internal int cttid;
        internal string cttname;
        internal string ctttel;
        internal string cttim;

        internal bool icon;

        public override void Read(ISource s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            base.Read(s, proj);

            s.Get(nameof(regid), ref regid);
            s.Get(nameof(addr), ref addr);
            s.Get(nameof(x), ref x);
            s.Get(nameof(y), ref y);
            s.Get(nameof(parent), ref parent);
            s.Get(nameof(@extern), ref @extern);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(mgrid), ref mgrid);
                s.Get(nameof(mgrname), ref mgrname);
                s.Get(nameof(mgrtel), ref mgrtel);
                s.Get(nameof(mgrim), ref mgrim);
                s.Get(nameof(cttid), ref cttid);
                s.Get(nameof(cttname), ref cttname);
                s.Get(nameof(ctttel), ref ctttel);
                s.Get(nameof(cttim), ref cttim);
                s.Get(nameof(icon), ref icon);
            }
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            base.Write(s, proj);

            if (regid > 0) s.Put(nameof(regid), regid); // conditional
            else s.PutNull(nameof(regid));

            s.Put(nameof(addr), addr);
            s.Put(nameof(x), x);
            s.Put(nameof(y), y);

            if (parent > 0) s.Put(nameof(parent), parent); // conditional
            else s.PutNull(nameof(parent));

            s.Put(nameof(@extern), @extern);

            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(mgrid), mgrid);
                s.Put(nameof(mgrname), mgrname);
                s.Put(nameof(mgrtel), mgrtel);
                s.Put(nameof(mgrim), mgrim);
                s.Put(nameof(cttid), cttid);
                s.Put(nameof(cttname), cttname);
                s.Put(nameof(ctttel), ctttel);
                s.Put(nameof(cttim), cttim);
                s.Put(nameof(icon), icon);
            }
        }

        public short Key => id;

        public string Tel => ctttel ?? mgrtel;

        public string Im => cttim ?? mgrim;


        public bool IsSocial => (typ & TYP_MART) == TYP_MART;


        public bool IsSocialTo(Reg reg) => IsSocial && (regid == reg.id);

        public override string ToString() => name;

        // credit account number
        string acct;


        public string Name => name;
    }
}