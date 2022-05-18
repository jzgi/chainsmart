using Chainly;
using Chainly.Web;

namespace Revital
{
    /// <summary>
    /// A generic data model for organizational unit.
    /// </summary>
    public class Org : Info, IKeyable<int>, IForkable
    {
        public static readonly Org Empty = new Org();

        public const short
            TYP_SPR = 0b01000, // supervisor
            TYP_BIZ = 0b00001, // business
            TYP_SRC = 0b00010, // source
            TYP_DST = 0b00100, // distribution
            TYP_MRT = TYP_SPR | TYP_BIZ, // market
            TYP_PRV = TYP_SPR | TYP_SRC, // provision sector
            TYP_CTR = TYP_SPR | TYP_SRC | TYP_DST; // provision center

        public const short
            FRK_BY_CTR = 1, // center 
            FRK_ON_OWN = 2; // own

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
#if ZHNT
            {TYP_BIZ, "商户"},
#else
            {TYP_BIZ, "驿站"},
#endif
            {TYP_SRC, "产源"},
            {TYP_MRT, "市场"},
            {TYP_PRV, "版块"},
            {TYP_CTR, "中枢"},
        };

        public static readonly Map<short, string> Forks = new Map<short, string>
        {
            {FRK_BY_CTR, "经过中枢"},
            {FRK_ON_OWN, "自行配运"},
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
        internal int[] ctrties; // tied centers 
        internal bool img;

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
            s.Get(nameof(ctrties), ref ctrties);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(mgrid), ref mgrid);
                s.Get(nameof(mgrname), ref mgrname);
                s.Get(nameof(mgrtel), ref mgrtel);
                s.Get(nameof(mgrim), ref mgrim);
                s.Get(nameof(img), ref img);
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
            s.Put(nameof(ctrties), ctrties);
            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(mgrid), mgrid);
                s.Put(nameof(mgrname), mgrname);
                s.Put(nameof(mgrtel), mgrtel);
                s.Put(nameof(mgrim), mgrim);
                s.Put(nameof(img), img);
            }
        }

        #region Properties

        public int Key => id;

        public short Fork => fork;

        public string Tel => tel;

        public string Im => mgrim;

        public bool IsSpr => (typ & TYP_SPR) == TYP_SPR;

        public bool IsPrv => typ == TYP_PRV;

        public bool IsOfPrv => (typ & TYP_PRV) == TYP_PRV;

        public bool IsPrvWith(int ctrid)
        {
            if (!IsPrv || ctrties == null)
            {
                return false;
            }
            for (int i = 0; i < ctrties.Length; i++)
            {
                if (ctrties[i] == ctrid) return true;
            }
            return false;
        }

        public bool IsOfSector => (typ & TYP_PRV) == TYP_PRV;

        public bool IsSource => typ == TYP_SRC;

        public bool IsBiz => typ == TYP_BIZ;

        public bool IsOfBiz => (typ & TYP_BIZ) == TYP_BIZ;

        public bool IsOfSource => (typ & TYP_SRC) == TYP_SRC;

        public bool IsMrt => typ == TYP_MRT;

        public bool IsCtr => typ == TYP_CTR;

        public bool HasXy => IsMrt || IsSource || IsCtr;

        public int ToCtrId => ctrties?[0] ?? 0;

        public bool MustTieToCtr => IsSpr && !IsCtr;

        public string Shop => IsMrt ? tip : name;

        public string ShopLabel => IsMrt ? "体验" : addr;

        public override string ToString() => name;

        #endregion
    }
}