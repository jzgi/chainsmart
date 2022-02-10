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
            TYP_SPR = 0b01000, // super
            TYP_BIZ = 0b00001, // business
            TYP_SRC = 0b00010, // source
            TYP_CTR = 0b00100, // center
            TYP_MRT = TYP_SPR | TYP_BIZ, // market
            TYP_PRV = TYP_SPR | TYP_SRC; // federation provisioning

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_BIZ, "商户"},
            {TYP_SRC, "产源"},
            {TYP_CTR, "中枢"},
#if ZHNT
            {TYP_MRT, "市场"},
#else
            {TYP_MRT, "驿站"},
#endif
            {TYP_PRV, "供应"},
        };


        public static readonly Map<short, string> Marks = new Map<short, string>
        {
            {1, "Ａ段位"},
            {2, "Ｂ段位"},
            {4, "Ｃ段位"},
            {7, "全段位"},
        };


        // id
        internal int id;

        // super
        internal int sprid;

        internal short fork;
        internal short mark;
        internal string license;
        internal short regid;
        internal string addr;
        internal double x;
        internal double y;

        internal string tel;
        internal bool trust;

        // later
        internal int mgrid;
        internal string mgrname;
        internal string mgrtel;
        internal string mgrim;
        internal bool cert;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(sprid), ref sprid);
            s.Get(nameof(fork), ref fork);
            s.Get(nameof(mark), ref mark);
            s.Get(nameof(license), ref license);
            s.Get(nameof(regid), ref regid);
            s.Get(nameof(addr), ref addr);
            s.Get(nameof(x), ref x);
            s.Get(nameof(y), ref y);
            s.Get(nameof(tel), ref tel);
            s.Get(nameof(trust), ref trust);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(mgrid), ref mgrid);
                s.Get(nameof(mgrname), ref mgrname);
                s.Get(nameof(mgrtel), ref mgrtel);
                s.Get(nameof(mgrim), ref mgrim);
                s.Get(nameof(cert), ref cert);
            }
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            if (sprid > 0) s.Put(nameof(sprid), sprid);
            else s.PutNull(nameof(sprid));
            s.Put(nameof(fork), fork);
            s.Put(nameof(mark), mark);
            s.Put(nameof(license), license);
            if (regid > 0) s.Put(nameof(regid), regid);
            else s.PutNull(nameof(regid));
            s.Put(nameof(addr), addr);
            s.Put(nameof(x), x);
            s.Put(nameof(y), y);
            s.Put(nameof(tel), tel);
            s.Put(nameof(trust), trust);

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

        public string Tel => tel;

        public string Im => mgrim;

        public bool IsSpr => (typ & TYP_SPR) == TYP_SPR;

        public bool IsPrv => typ == TYP_PRV;

        public bool IsSrc => typ == TYP_SRC;

        public bool IsBiz => typ == TYP_BIZ;

        public bool IsOfBiz => (typ & TYP_BIZ) == TYP_BIZ;

        public bool IsMrt => typ == TYP_MRT;

        public bool IsCtr => typ == TYP_CTR;

        public bool IsOfCtr => (typ & TYP_CTR) == TYP_CTR;

        public string Shop => IsMrt ? tip : name;

        public string ShopLabel => IsMrt ? "体验" : addr;

        public override string ToString() => name;
    }
}