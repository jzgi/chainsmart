using SkyChain;
using SkyChain.Source.Web;

namespace Revital
{
    /// <summary>
    /// The data model for an organizational unit.
    /// </summary>
    public class Org : _Info, IKeyable<int>, IForkable
    {
        public static readonly Org Empty = new Org();

        public const short
            TYP_FED = 0b10000, // federal
            TYP_SPR = 0b01000, // super
            TYP_BIZ = 0b00001, // business
            TYP_SRC = 0b00010, // source
            TYP_CTR = 0b00100, // center
            TYP_MRT = TYP_SPR | TYP_BIZ, // market
            TYP_PRV = TYP_SPR | TYP_SRC, // provisioning
            TYP_PRV_X = TYP_FED | TYP_PRV; // federation provisioning

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_BIZ, "商户"},
            {TYP_SRC, "产源"},
            {TYP_CTR, "控配"},
#if ZHNT
            {TYP_MRT, "市场"},
#else
            {TYP_MRT, "驿站"},
#endif
            {TYP_PRV, "供应"},
            {TYP_PRV_X, "供应（联盟）"},
        };


        public static readonly Map<short, string> Ranks = new Map<short, string>
        {
            {0, "普通商户"},
            {1, "银牌商户"},
            {2, "金牌商户"},
        };


        public const short
            OP_INSERT = TYP | STATUS | LABEL | CREATE | SUPER | BASIC | OWN,
            OP_UPDATE = STATUS | LABEL | ADAPT | BASIC | OWN,
            OP_UPDATE_OWN = STATUS | OWN,
            SUPER = 0x0040,
            OWN = 0x0100;


        // id
        internal int id;

        // super
        internal int sprid;

        // basic
        internal short fork;
        internal short rank;
        internal string license;
        internal short regid;
        internal string addr;
        internal double x;
        internal double y;

        // own
        internal string tel;
        internal bool trust;
        internal string emblem;

        // later
        internal int mgrid;
        internal string mgrname;
        internal string mgrtel;
        internal string mgrim;
        internal bool cert;

        public override void Read(ISource s, short proj = 0x0fff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((proj & SUPER) == SUPER)
            {
                s.Get(nameof(sprid), ref sprid);
            }
            if ((proj & BASIC) == BASIC)
            {
                s.Get(nameof(fork), ref fork);
                s.Get(nameof(rank), ref rank);
                s.Get(nameof(license), ref license);
                s.Get(nameof(regid), ref regid);
                s.Get(nameof(addr), ref addr);
                s.Get(nameof(x), ref x);
                s.Get(nameof(y), ref y);
            }
            if ((proj & OWN) == OWN)
            {
                s.Get(nameof(tel), ref tel);
                s.Get(nameof(trust), ref trust);
                s.Get(nameof(emblem), ref emblem);
            }
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(mgrid), ref mgrid);
                s.Get(nameof(mgrname), ref mgrname);
                s.Get(nameof(mgrtel), ref mgrtel);
                s.Get(nameof(mgrim), ref mgrim);
                s.Get(nameof(cert), ref cert);
            }
        }

        public override void Write(ISink s, short proj = 0x0fff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            if (fork > 0) s.Put(nameof(fork), fork);
            else s.PutNull(nameof(fork));

            if (sprid > 0) s.Put(nameof(sprid), sprid);
            else s.PutNull(nameof(sprid));

            if ((proj & BASIC) == BASIC)
            {
                s.Put(nameof(fork), fork);
                s.Put(nameof(rank), rank);
                s.Put(nameof(license), license);
                if (regid > 0) s.Put(nameof(regid), regid);
                else s.PutNull(nameof(regid));
                s.Put(nameof(addr), addr);
                s.Put(nameof(x), x);
                s.Put(nameof(y), y);
            }
            if ((proj & OWN) == OWN)
            {
                s.Put(nameof(tel), tel);
                s.Put(nameof(trust), trust);
                s.Put(nameof(emblem), emblem);
            }
            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(mgrid), mgrid);
                s.Put(nameof(mgrname), mgrname);
                s.Put(nameof(mgrtel), mgrtel);
                s.Put(nameof(mgrim), mgrim);
                s.Put(nameof(cert), cert);
            }
        }

        public int Key => id;

        public short Fork => fork;

        public string Tel => mgrtel;

        public string Im => mgrim;

        public bool IsSpr => (typ & TYP_SPR) == TYP_SPR;

        public bool IsPrv => typ == TYP_PRV;

        public bool IsSrc => typ == TYP_SRC;

        public bool IsBiz => typ == TYP_BIZ;

        public bool IsOfBiz => (typ & TYP_BIZ) == TYP_BIZ;

        public bool IsMrt => typ == TYP_MRT;

        public bool IsCtr => typ == TYP_CTR;

        public bool IsOfCtr => (typ & TYP_CTR) == TYP_CTR;

        public override string ToString() => name;
    }
}