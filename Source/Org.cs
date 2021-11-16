using SkyChain;
using SkyChain.Source.Web;

namespace Revital
{
    /// <summary>
    /// The data model for an organizational unit.
    /// </summary>
    public class Org : _Article, IKeyable<int>, IForkable
    {
        public static readonly Org Empty = new Org();

        public const short
            TYP_BIZ = 0b000001,
            TYP_FRM = 0b000010, // farm
            TYP_SPR = 0b000100, // supervisor
            TYP_MRT = TYP_SPR | TYP_BIZ, // mart
            TYP_SRC = TYP_SPR | TYP_FRM, // source
            TYP_CHL = TYP_SPR | TYP_BIZ | TYP_FRM, // channel
            TYP_SUB = 0b001000, // subscrib
            TYP_NED = 0b010000, // need
            TYP_CTR = 0b100000 | TYP_BIZ, // center with distrib
            TYP_CTR_PLUS = TYP_CTR | TYP_NED, // center with distrib + need 
            TYP_CTR_FUL = TYP_CTR | TYP_NED | TYP_SUB; // center with distrib + need + subscrib

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_BIZ, "商户／服务点"},
            {TYP_FRM, "生产户／地块"},
            {TYP_MRT, "市场／驿站"},
            {TYP_SRC, "产源／产地"},
            {TYP_CHL, "供销渠道"},
            {TYP_CTR, "供应中心（单步）"},
            {TYP_CTR_PLUS, "供应中心（半链）"},
            {TYP_CTR_FUL, "供应中心（全链）"},
        };


        internal int id;
        internal short forkie;
        internal int sprid;
        internal int ctrid;
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
            s.Get(nameof(forkie), ref forkie);
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
            s.Put(nameof(forkie), forkie);

            if (sprid > 0) s.Put(nameof(sprid), sprid);
            else s.PutNull(nameof(sprid));

            if (ctrid > 0) s.Put(nameof(ctrid), ctrid);
            else s.PutNull(nameof(ctrid));

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

        public short Forkie => forkie;

        public string Tel => mgrtel;

        public string Im => mgrim;

        public bool IsSrc => (typ & TYP_SRC) == TYP_SRC;

        public bool IsFarm => (typ & TYP_FRM) == TYP_FRM;

        public bool IsBiz => (typ & TYP_BIZ) == TYP_BIZ;

        public bool IsMart => (typ & TYP_MRT) == TYP_MRT;

        public bool IsOfCenter => (typ & TYP_CTR) == TYP_CTR;

        public bool IsCenter => typ == TYP_CTR;

        public bool IsTruster => IsBiz || IsSrc || IsCenter;

        public bool WithNeed => (typ & TYP_NED) == TYP_NED;

        public bool WithSubsrib => (typ & TYP_SUB) == TYP_SUB;

        public override string ToString() => name;
    }
}