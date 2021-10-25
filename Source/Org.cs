using System;
using System.Text;
using SkyChain;

namespace Rev.Supply
{
    /// <summary>
    /// The data model for an organizational unit.
    /// </summary>
    public class Org : IData, IKeyable<int>
    {
        public static readonly Org Empty = new Org();

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

        public const short
            STA_DISABLED = 0,
            STA_SHOWED = 1,
            STA_ENABLED = 2,
            STA_PREFERED = 3;

        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STA_DISABLED, "禁用"},
            {STA_SHOWED, "展示"},
            {STA_ENABLED, "可用"},
            {STA_PREFERED, "优先"},
        };

        public const byte ID = 1, LATER = 2, PRIVACY = 4;


        internal short typ;
        internal short status;
        internal string name;
        internal string tip;
        internal DateTime created;
        internal string creator;

        internal short id;

        // joined group if any
        internal short coid;

        // the associated distribution center, if any
        internal short ctrid;

        internal short regid;

        internal string addr;
        internal double x;
        internal double y;

        internal bool delegated;


        internal int mgrid;
        internal string mgrname;
        internal string mgrtel;
        internal string mgrim;
        internal int cttid;
        internal string cttname;
        internal string ctttel;
        internal string cttim;

        internal bool icon;
        internal bool license;
        internal bool perm;

        public void Read(ISource s, byte proj = 0x0f)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(name), ref name);
            s.Get(nameof(tip), ref tip);
            s.Get(nameof(created), ref created);
            s.Get(nameof(creator), ref creator);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(regid), ref regid);
            s.Get(nameof(addr), ref addr);
            s.Get(nameof(x), ref x);
            s.Get(nameof(y), ref y);
            s.Get(nameof(coid), ref coid);
            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(delegated), ref delegated);
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
                s.Get(nameof(license), ref license);
                s.Get(nameof(perm), ref perm);
            }
        }

        public void Write(ISink s, byte proj = 0x0f)
        {
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);
            s.Put(nameof(name), name);
            s.Put(nameof(tip), tip);
            s.Put(nameof(created), created);
            s.Put(nameof(creator), creator);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            if (regid > 0) s.Put(nameof(regid), regid); // conditional
            else s.PutNull(nameof(regid));

            s.Put(nameof(addr), addr);
            s.Put(nameof(x), x);
            s.Put(nameof(y), y);

            if (coid > 0) s.Put(nameof(coid), coid); // conditional
            else s.PutNull(nameof(coid));

            if (ctrid > 0) s.Put(nameof(ctrid), ctrid); // conditional
            else s.PutNull(nameof(ctrid));

            s.Put(nameof(delegated), delegated);

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
                s.Put(nameof(license), license);
                s.Put(nameof(perm), perm);
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

        public bool IsMerchantTo(Reg reg) => IsMerchant && (regid == reg.id);

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