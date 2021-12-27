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
            TYP_SPR = 0b1000, // super
            TYP_BIZ = 0b0001, // business
            TYP_SRC = 0b0010, // source
            TYP_CTR = 0b0100, // sector
            TYP_MRT = TYP_SPR | TYP_BIZ, // market
            TYP_PRV = TYP_SPR | TYP_SRC; // provisioning

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_BIZ, "商户"},
            {TYP_SRC, "供源"},
            {TYP_CTR, "中心"},
#if ZHNT
            {TYP_MRT, "综合市场"},
#else
            {TYP_MRT, "驿站"},
#endif
            {TYP_PRV, "供应板块"},
        };


        public static readonly Map<short, string> Ranks = new Map<short, string>
        {
            {0, "普通"},
            {1, "银牌"},
            {2, "金牌"},
        };


        internal int id;
        internal short fork;
        internal int sprid;
        internal short rank;
        internal string license;
        internal bool trust;
        internal short regid;
        internal string addr;
        internal double x;
        internal double y;

        internal int mgrid;
        internal string mgrname;
        internal string mgrtel;
        internal string mgrim;
        internal bool icon;

        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(fork), ref fork);
            s.Get(nameof(sprid), ref sprid);
            s.Get(nameof(rank), ref rank);
            s.Get(nameof(license), ref license);
            s.Get(nameof(trust), ref trust);
            s.Get(nameof(regid), ref regid);
            s.Get(nameof(addr), ref addr);
            s.Get(nameof(x), ref x);
            s.Get(nameof(y), ref y);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(mgrid), ref mgrid);
                s.Get(nameof(mgrname), ref mgrname);
                s.Get(nameof(mgrtel), ref mgrtel);
                s.Get(nameof(mgrim), ref mgrim);
                s.Get(nameof(icon), ref icon);
            }
        }

        public override void Write(ISink s, byte proj = 0x0f)
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

            s.Put(nameof(rank), rank);
            s.Put(nameof(license), license);
            s.Put(nameof(trust), trust);

            if (regid > 0) s.Put(nameof(regid), regid);
            else s.PutNull(nameof(regid));

            s.Put(nameof(addr), addr);
            s.Put(nameof(x), x);
            s.Put(nameof(y), y);

            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(mgrid), mgrid);
                s.Put(nameof(mgrname), mgrname);
                s.Put(nameof(mgrtel), mgrtel);
                s.Put(nameof(mgrim), mgrim);
                s.Put(nameof(icon), icon);
            }
        }

        public int Key => id;

        public short Fork => fork;

        public string Tel => mgrtel;

        public string Im => mgrim;

        public bool IsSpr => (typ & TYP_SPR) == TYP_SPR;

        public bool IsSrc => typ == TYP_PRV;

        public bool IsPrd => typ == TYP_SRC;

        public bool IsBiz => typ == TYP_BIZ;

        public bool IsOfBiz => (typ & TYP_BIZ) == TYP_BIZ;

        public bool IsMrt => typ == TYP_MRT;

        public bool IsCtr => typ == TYP_CTR;

        public bool IsOfCtr => (typ & TYP_CTR) == TYP_CTR;

        public override string ToString() => name;
    }
}