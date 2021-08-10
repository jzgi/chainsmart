using System;
using SkyChain;

namespace Zhnt
{
    /// <summary>
    /// A user & principal data object.
    /// </summary>
    public class User : IData, IKeyable<int>
    {
        public static readonly User Empty = new User();

        public const byte
            ID = 1,
            PRIVACY = 2,
            LATER = 4;

        public const byte
            TYP_CONSULTANT = 1,
            TYP_COOK = 2;

        // user types
        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {0, null},
            {TYP_CONSULTANT, "调养顾问"},
            {TYP_COOK, "调养厨师"},
        };

        public const short
            ADMLY = 1,
            ADMLY_OP = 3,
            ADMLY_PROD = 5,
            ADMLY_IT = 7;

        public static readonly Map<short, string> Admly = new Map<short, string>
        {
            {0, null},
            {ADMLY, "基本"},
            {ADMLY_OP, "运营部"},
            {ADMLY_PROD, "生产部"},
            {ADMLY_IT, "信息部"},
        };

        public const short
            ORGLY_OP = 1,
            ORGLY_BIZ = 3,
            ORGLY_MGR = 7;

        public static readonly Map<short, string> Mrtly = new Map<short, string>
        {
            {0, null},
            {ORGLY_OP, "操作员"}, // 001
            {ORGLY_BIZ, "业务员"}, // 011
            {ORGLY_MGR, "管理员"}, // 011
        };

        public static readonly Map<short, string> Bizly = new Map<short, string>
        {
            {0, null},
            {ORGLY_OP, "操作员"}, // 001
            {ORGLY_MGR, "管理员"}, // 011
        };

        public static readonly Map<short, string> Ctrly = new Map<short, string>
        {
            {0, null},
            {ORGLY_OP, "操作员"}, // 001
            {ORGLY_BIZ, "业务员"}, // 011
            {ORGLY_MGR, "管理员"}, // 011
        };

        public const short
            STATUS_DISABLED = 0,
            STATUS_NORMAL = 1;


        internal int id;
        internal short typ;
        internal short status;
        internal string name;
        internal string tel;
        internal string im;
        internal DateTime created;
        internal string credential;
        internal short admly;
        internal int orgid;
        internal short orgly;
        internal string acct; // identity number

        public void Read(ISource s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(name), ref name);
            s.Get(nameof(tel), ref tel);
            s.Get(nameof(im), ref im);
            s.Get(nameof(created), ref created);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(credential), ref credential);
                s.Get(nameof(admly), ref admly);
                s.Get(nameof(orgid), ref orgid);
                s.Get(nameof(orgly), ref orgly);
                s.Get(nameof(acct), ref acct);
            }
        }

        public void Write(ISink s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);
            s.Put(nameof(name), name);
            s.Put(nameof(tel), tel);
            s.Put(nameof(im), im);
            s.Put(nameof(created), created);
            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(credential), credential);
                s.Put(nameof(admly), admly);
                s.Put(nameof(orgid), orgid);
                s.Put(nameof(orgly), orgly);
                s.Put(nameof(acct), acct);
            }
        }

        public int Key => id;

        public short Sex
        {
            get
            {
                if (acct == null)
                {
                    return 0;
                }
                var num = acct[16] - '0';
                return (short) (num % 2 == 1 ? 1 : 2);
            }
        }

        public DateTime Birth
        {
            get
            {
                if (acct == null)
                {
                    return default;
                }
                var n = acct;
                int year = (n[6] - '0') * 1000 + (n[7] - '0') * 100 + (n[8] - '0') * 10 + (n[9] - '0') * 1;
                int month = (n[10] - '0') * 10 + (n[11] - '0') * 1;
                int day = (n[12] - '0') * 10 + (n[13] - '0') * 1;
                return new DateTime(year, month, day);
            }
        }

        public bool IsPro => typ >= 1;

        public bool IsCertified => !string.IsNullOrEmpty(acct);

        public bool IsDisabled => status <= STATUS_DISABLED;

        public override string ToString() => name;
    }
}