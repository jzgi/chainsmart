using ChainFx;

namespace ChainMart
{
    public class User : Entity, IKeyable<int>
    {
        public static readonly User Empty = new User();

        // pro types
        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {0, "普通"},
            {1, "市场运营师"},
            {2, "健康管理师"},
        };

        public const short
            ROLE_ = 0b000001, // common
            ROLE_OPN = 0b000011, // operation
            ROLE_LOG = 0b000101, // logistic
            ROLE_FIN = 0b001001, // finance
            ROLE_MGT = 0b011111, // management
            ROLE_RVW = 0b100001, // review
            ROLE_DEL = ROLE_MGT | ROLE_RVW; // delegate

        public static readonly Map<short, string> Admly = new Map<short, string>
        {
            {ROLE_OPN, "业务"},
            {ROLE_FIN, "财务"},
            {ROLE_MGT, "管理"},
            {ROLE_RVW, "审核"},
        };

        public static readonly Map<short, string> Orgly = new Map<short, string>
        {
            {0, null},
            {ROLE_OPN, "业务"},
            {ROLE_LOG, "物流"},
            {ROLE_MGT, "管理"},
            {ROLE_RVW, "审核"},
            {ROLE_DEL, "代办"},
        };

        internal int id;
        internal string tel;
        internal string addr;
        internal string im;

        // later
        internal string credential;
        internal short admly;
        internal int orgid;
        internal short orgly;
        internal int vip;
        internal bool icon;

        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(tel), ref tel);
                s.Get(nameof(im), ref im);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(addr), ref addr);
                s.Get(nameof(credential), ref credential);
                s.Get(nameof(admly), ref admly);
                s.Get(nameof(orgid), ref orgid);
                s.Get(nameof(orgly), ref orgly);
                s.Get(nameof(vip), ref vip);
                s.Get(nameof(icon), ref icon);
            }
        }

        public override void Write(ISink s, short msk = 0xff)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(tel), tel);
            s.Put(nameof(im), im);
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(addr), addr);
                s.Put(nameof(credential), credential);
                s.Put(nameof(admly), admly);
                s.Put(nameof(orgid), orgid);
                s.Put(nameof(orgly), orgly);
                s.Put(nameof(vip), vip);
                s.Put(nameof(icon), icon);
            }
        }

        public int Key => id;

        public bool CanDelegate(Org targ) =>
            (targ.prtid == 0 && (admly & ROLE_MGT) == ROLE_MGT) ||
            (targ.prtid == orgid && (orgly & ROLE_MGT) == ROLE_MGT);

        public bool IsProfessional => typ >= 1;

        public bool IsAdmly => admly > 0;

        public bool IsOrgly => orgly > 0 && orgid > 0;

        public override string ToString() => name;
    }
}