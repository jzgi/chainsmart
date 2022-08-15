using CoChain;
using CoChain.Web;

namespace CoSupply
{
    /// <summary>
    /// An organizational unit.
    /// </summary>
    public class Org : Entity, IKeyable<int>, IForkable
    {
        public static readonly Org Empty = new Org();

        public const short
            TYP_SPR = 0b01000, // supervisor
            TYP_SHP = 0b00001, // shop
            TYP_SRC = 0b00010, // source
            TYP_DST = 0b00100, // distributor
            TYP_MRT = TYP_SPR | TYP_SHP, // market
            TYP_PRV = TYP_SPR | TYP_SRC, // provision sector
            TYP_CTR = TYP_SPR | TYP_SRC | TYP_DST; // provision center

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
#if ZHNT
            {TYP_SHP, "商户"},
#else
            {TYP_SHP, "驿站"},
#endif
            {TYP_SRC, "产源"},
            {TYP_MRT, "市场"},
            {TYP_PRV, "版块"},
            {TYP_CTR, "中枢"},
        };

        public const short
            FRK_STD = 1, // standard, center-based 
            FRK_OWN = 2; // on own

        public static readonly Map<short, string> Forks = new Map<short, string>
        {
            {FRK_STD, "标准运控"},
            {FRK_OWN, "自行安排"},
        };

        // id
        internal int id;

        // super id
        internal int sprid;

        internal short fork;
        internal string license;
        internal short regid;
        internal string addr;
        internal double x;
        internal double y;

        internal string tel;
        internal bool trust;

        internal int mgrid;
        internal string mgrname;
        internal string mgrtel;
        internal string mgrim;
        internal int ctrid; // tied center, can be null 
        internal bool icon;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((proj & BORN) == BORN)
            {
                s.Get(nameof(sprid), ref sprid);
            }
            s.Get(nameof(fork), ref fork);
            s.Get(nameof(license), ref license);
            s.Get(nameof(regid), ref regid);
            s.Get(nameof(addr), ref addr);
            s.Get(nameof(x), ref x);
            s.Get(nameof(y), ref y);
            s.Get(nameof(tel), ref tel);
            s.Get(nameof(trust), ref trust);
            s.Get(nameof(ctrid), ref ctrid);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(mgrid), ref mgrid);
                s.Get(nameof(mgrname), ref mgrname);
                s.Get(nameof(mgrtel), ref mgrtel);
                s.Get(nameof(mgrim), ref mgrim);
                s.Get(nameof(icon), ref icon);
            }
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            if ((proj & BORN) == BORN)
            {
                if (sprid > 0) s.Put(nameof(sprid), sprid);
                else s.PutNull(nameof(sprid));
            }
            s.Put(nameof(fork), fork);
            s.Put(nameof(license), license);
            if (regid > 0) s.Put(nameof(regid), regid);
            else s.PutNull(nameof(regid));
            s.Put(nameof(addr), addr);
            s.Put(nameof(x), x);
            s.Put(nameof(y), y);
            s.Put(nameof(tel), tel);
            s.Put(nameof(trust), trust);
            s.Put(nameof(ctrid), ctrid);
            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(mgrid), mgrid);
                s.Put(nameof(mgrname), mgrname);
                s.Put(nameof(mgrtel), mgrtel);
                s.Put(nameof(mgrim), mgrim);
                s.Put(nameof(icon), icon);
            }
        }

        #region Properties

        public int Key => id;

        public short Fork => fork;

        public string Tel => tel;

        public string Im => mgrim;

        public bool HasSuper => (typ & TYP_SPR) == TYP_SPR;

        public bool IsProvision => typ == TYP_PRV;

        public bool HasProvision => (typ & TYP_PRV) == TYP_PRV;


        public bool IsSource => typ == TYP_SRC;

        public bool HasSource => (typ & TYP_SRC) == TYP_SRC;

        public bool IsShop => typ == TYP_SHP;

        public bool HasShop => (typ & TYP_SHP) == TYP_SHP;

        public bool IsMarket => typ == TYP_MRT;

        public bool IsCenter => typ == TYP_CTR;

        public bool HasXy => IsMarket || IsSource || IsCenter;

        public bool MustTieToCtr => HasSuper && !IsCenter;

        public string ShopName => IsMarket ? tip : name;

        public string ShopLabel => IsMarket ? "体验" : addr;

        public override string ToString() => name;

        #endregion
    }
}