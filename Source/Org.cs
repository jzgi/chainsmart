using SkyChain;

namespace Revital
{
    /// <summary>
    /// The data model for an organizational unit.
    /// </summary>
    public class Org : _Bean, IKeyable<int>
    {
        public static readonly Org Empty = new Org();

        public const short
            TYP_SPR = 0b00100, // supervisor
            TYP_BIZ = 0b00001,
            TYP_FRM = 0b00010, // farm
            TYP_TRD = 0b00100, // trader
            TYP_MRT = TYP_SPR | TYP_BIZ, // mart
            TYP_SRC = TYP_SPR | TYP_FRM, // source
            TYP_CHL = TYP_SPR | TYP_BIZ | TYP_FRM, // channel
            TYP_CTR = 0b01000, // center
            TYP_CTR_HLF = TYP_CTR | TYP_BIZ,
            TYP_CTR_FUL = TYP_CTR | TYP_TRD;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_BIZ, "商户／服务"},
            {TYP_FRM, "农场／地块"},
            {TYP_MRT, "市集／驿站"},
            {TYP_SRC, "产源／基地"},
            {TYP_CHL, "供销渠道"},
            {TYP_CTR, "供应中心（封闭）"},
            {TYP_CTR_HLF, "供应中心（半链）"},
            {TYP_CTR_FUL, "供应中心（全链）"},
        };

        public const short
            KIND_AGRICTR = 1,
            KIND_DIETARYCTR = 2,
            KIND_FACTORYCTR = 3,
            KIND_CARECTR = 4,
            KIND_POSTCTR = 5,
            KIND_ADCTR = 6,
            KIND_CHARITYCTR = 7;

        public static readonly Map<short, string> Tags = new Map<short, string>
        {
            {1, "特优区"},
            {2, "惠实区"},
        };


        internal int id;
        internal short kind;
        internal int sprid;
        internal int ctrid;
        internal string license;
        internal bool trust;
        internal short regid;
        internal string addr;
        internal short tag;
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
            s.Get(nameof(sprid), ref sprid);
            s.Get(nameof(ctrid), ref ctrid);
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
            
            if (sprid > 0) s.Put(nameof(sprid), sprid); else s.PutNull(nameof(sprid));
            
            if (ctrid > 0) s.Put(nameof(ctrid), ctrid); else s.PutNull(nameof(ctrid));
            
            s.Put(nameof(license), license);
            s.Put(nameof(trust), trust);
            
            if (regid > 0) s.Put(nameof(regid), regid); else s.PutNull(nameof(regid));
            
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

        public string Tel => mgrtel;

        public string Im => mgrim;

        public bool IsSrc => (typ & TYP_SRC) == TYP_SRC;

        public bool IsSrcCo => (typ & TYP_SRC) == TYP_SRC;

        public bool IsBiz => (typ & TYP_BIZ) == TYP_BIZ;

        public bool IsBizCo => (typ & TYP_MRT) == TYP_MRT;

        public bool IsCenter => (typ & TYP_CTR_HLF) == TYP_CTR_HLF;

        public bool IsTruster => IsBiz || IsSrc || IsCenter;

        public override string ToString() => name;
    }
}