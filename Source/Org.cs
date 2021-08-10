using System.Text;
using SkyChain;

namespace Zhnt
{
    /// <summary>
    /// An organizational unit, that can be base, center, point, market or booth.
    /// </summary>
    public class Org : _Art, IKeyable<int>
    {
        public static readonly Org Empty = new Org();

        public const short
            TYP_SPR = 1,
            TYP_CTR = 2,
            TYP_BIZ = 4,
            TYP_MART = 8;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_SPR, "供应基地"},
            {TYP_CTR, "分拣中心"},
            {TYP_BIZ, "商户"},
            {TYP_MART, "市场"},
        };

        internal int id;
        internal short regid;

        internal string addr;
        internal double x;
        internal double y;

        // the associated center (for pt)
        internal short parent;

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

        public int Key => id;

        public string Tel => ctttel ?? mgrtel;

        public string Im => cttim ?? mgrim;

        public bool IsShop => (typ & TYP_SPR) == TYP_SPR;

        public bool IsPt => (typ & TYP_BIZ) == TYP_BIZ;

        public bool IsInternal => !@extern;

        public bool IsMerchant => (typ & TYP_CTR) == TYP_CTR;

        public bool IsSocial => (typ & TYP_MART) == TYP_MART;

        public bool IsProvider => IsMerchant || IsSocial;

        public bool IsMerchantTo(Reg reg) => IsMerchant && (regid == reg.id || @extern);

        public bool IsSocialTo(Reg reg) => IsSocial && (regid == reg.id);

        public override string ToString() => name;

        // credit account number
        string acct;

        public string Acct => acct ??= GetAcct(id);

        public string Name => name;

        public static string GetAcct(int orgid)
        {
            var sb = new StringBuilder();
            if (orgid < 10000)
            {
                sb.Append('0');
            }
            if (orgid < 1000)
            {
                sb.Append('0');
            }
            if (orgid < 100)
            {
                sb.Append('0');
            }
            if (orgid < 10)
            {
                sb.Append('0');
            }
            sb.Append(orgid);

            return sb.ToString();
        }
    }
}