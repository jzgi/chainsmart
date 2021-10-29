using System.Text;
using SkyChain;

namespace Revital.Supply
{
    /// <summary>
    /// The data model for an organizational unit.
    /// </summary>
    public class Org_ : _Bean, IKeyable<int>
    {
        public static readonly Org_ Empty = new Org_();

        public const short
            TYP_SUP = 1,
            TYP_BIZ = 2,
            TYP_BIZCO = 4,
            TYP_SRC = 8,
            TYP_SRCCO = 16;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_SUP + TYP_BIZ, "供应中心"},
            {TYP_BIZ, "商户／服务"},
            {TYP_BIZ + TYP_BIZCO, "市场／驿站"},
            {TYP_SRC, "产源／地块"},
            {TYP_SRC + TYP_SRCCO, "产源社／产地"},
        };

        internal int id;
        internal int sprid;
        internal int ctrid;
        internal string license;
        internal bool trust;
        internal string regid;
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
            if (sprid > 0) s.Put(nameof(sprid), sprid); // conditional
            else s.PutNull(nameof(sprid));
            if (ctrid > 0) s.Put(nameof(ctrid), ctrid); // conditional
            else s.PutNull(nameof(ctrid));
            s.Put(nameof(license), license);
            s.Put(nameof(trust), trust);
            s.PutNull(nameof(regid));
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

        public bool IsSrc => (typ & TYP_SRCCO) == TYP_SRCCO;

        public bool IsBiz => (typ & TYP_BIZ) == TYP_BIZ;

        public bool IsBizCo => (typ & TYP_BIZCO) == TYP_BIZCO;

        public bool IsInternal => false;

        public bool IsMerchant => (typ & TYP_SUP) == TYP_SUP;

        public bool IsSocial => (typ & TYP_BIZCO) == TYP_BIZCO;

        public bool IsProvider => IsMerchant || IsSocial;

        public bool IsMerchantTo(Reg_ reg) => true;

        public bool IsSocialTo(Reg_ reg) => true;

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