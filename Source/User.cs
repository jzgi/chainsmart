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
            ROL_ = 0b000001, // common
            ROL_OPN = 0b0000011, // operation
            ROL_LOG = 0b0000101, // logistic
            ROL_FIN = 0b0001001, // finance
            ROL_MGT = 0b0011111, // management
            // suppliement
            ROL_RVW = 0b0100001; // review

        public static readonly Map<short, string> Admly = new Map<short, string>
        {
            {ROL_OPN, "业务"},
            {ROL_LOG, "物流"},
            {ROL_FIN, "财务"},
            {ROL_MGT, "管理"},
            // suppliement
            {ROL_RVW, "审核"},
        };

        public static readonly Map<short, string> Orgly = new Map<short, string>
        {
            {ROL_OPN, "业务"},
            {ROL_LOG, "物流"},
            {ROL_FIN, "财务"},
            {ROL_MGT, "管理"},
            {ROL_RVW, "审核"},

            {ROL_MGT | ROL_RVW, "管理审核"},
        };

        internal int id;
        internal string tel;
        internal string addr;
        internal string im;

        // later
        internal string credential;
        internal short admly;
        internal int srcid;
        internal short zonly;
        internal int shpid;
        internal short mktly;
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
                s.Get(nameof(srcid), ref srcid);
                s.Get(nameof(zonly), ref zonly);
                s.Get(nameof(shpid), ref shpid);
                s.Get(nameof(mktly), ref mktly);
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
                s.Put(nameof(srcid), srcid);
                s.Put(nameof(zonly), zonly);
                s.Put(nameof(shpid), shpid);
                s.Put(nameof(mktly), mktly);
                s.Put(nameof(vip), vip);
                s.Put(nameof(icon), icon);
            }
        }

        public int Key => id;

        public bool IsProfessional => typ >= 1;

        public bool HasAdmly => admly > 0;

        public bool HasAdmlyMgt => (admly & ROL_MGT) == ROL_MGT;


        /// <summary>
        /// admly, srcid + zonly, shpid + mktly
        /// </summary>
        public (bool dive, short role ) GetRoleForOrg(Org org, short orgtyp = 0)
        {
            bool dive;
            short role = 0;

            var orgid_ = org.IsSource ? srcid : shpid;
            var orgly_ = org.IsSource ? zonly : mktly;

            // is of any role for the org
            if (org.id == orgid_)
            {
                role = orgly_;
                dive = false;
            }
            else //  diving role
            {
                if (org.IsTopOrg && admly > 0)
                {
                    if (org.trust)
                    {
                        role = admly;
                    }
                    if ((admly & ROL_MGT) == ROL_MGT)
                    {
                        role |= ROL_RVW;
                    }
                }
                else if (!org.IsTopOrg && orgid_ == org.prtid && (orgly_ > 0 && orgid_ > 0))
                {
                    if (org.trust)
                    {
                        role = orgly_;
                    }
                    if ((orgly_ & ROL_MGT) == ROL_MGT)
                    {
                        role |= ROL_RVW;
                    }
                }
                dive = true;
            }

            return (dive, role);
        }

        public bool CanDive(Org org)
        {
            var (div, role) = GetRoleForOrg(org);
            return div && role > 0;
        }

        public override string ToString() => name;
    }
}