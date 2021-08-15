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
            TYP_CTR = 1,
            TYP_SRC = 2,
            TYP_SRCGRP = 4,
            TYP_BIZ = 8,
            TYP_BIZGRP = 16;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_CTR, "分拣中心"},
            {TYP_SRC, "产源"},
            {TYP_SRCGRP, "产源群主"},
            {TYP_SRC + TYP_SRCGRP, "产源＋群主"},
            {TYP_BIZ, "商户"},
            {TYP_BIZGRP, "商户群主"},
            {TYP_BIZ + TYP_BIZGRP, "商户＋群主"},
        };

        internal int id;

        // joined group if any
        internal int grpid;

        // the associated distribution center, if any
        internal int refid;

        internal short regid;

        internal string addr;
        internal double x;
        internal double y;

        internal int mgrid;
        internal string mgrname;
        internal string mgrtel;
        internal string mgrim;

        internal bool icon;
        internal bool license;
        internal bool perm;

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
            s.Get(nameof(grpid), ref grpid);
            s.Get(nameof(refid), ref refid);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(mgrid), ref mgrid);
                s.Get(nameof(mgrname), ref mgrname);
                s.Get(nameof(mgrtel), ref mgrtel);
                s.Get(nameof(mgrim), ref mgrim);
                s.Get(nameof(icon), ref icon);
                s.Get(nameof(license), ref license);
                s.Get(nameof(perm), ref perm);
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

            if (grpid > 0) s.Put(nameof(grpid), grpid); // conditional
            else s.PutNull(nameof(grpid));

            s.Put(nameof(refid), refid);

            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(mgrid), mgrid);
                s.Put(nameof(mgrname), mgrname);
                s.Put(nameof(mgrtel), mgrtel);
                s.Put(nameof(mgrim), mgrim);
                s.Put(nameof(icon), icon);
                s.Put(nameof(license), license);
                s.Put(nameof(perm), perm);
            }
        }

        public int Key => id;

        public string Tel => mgrtel;

        public string Im => mgrim;

        public bool IsShop => (typ & TYP_SRC) == TYP_SRC;

        public bool IsPt => (typ & TYP_BIZ) == TYP_BIZ;

        public bool IsInternal => false;

        public bool IsMerchant => (typ & TYP_CTR) == TYP_CTR;

        public bool IsSocial => (typ & TYP_BIZGRP) == TYP_BIZGRP;

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