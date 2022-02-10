using System;
using SkyChain;

namespace Revital
{
    public class User : _Info, IKeyable<int>
    {
        public static readonly User Empty = new User();

        // pro types
        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {0, null},
            {1, "初级护理员"},
            {2, "中级护理员"},
            {3, "高级护理员"},
            {7, "三级健康管理师"},
            {8, "二级健康管理师"},
            {9, "一级健康管理师"},
        };

        public const short
            ADMLY_ = 0b0001, // common
            ADMLY_OPN = 0b0011,
            ADMLY_SAL = 0b0101,
            ADMLY_FIN = 0b1001,
            ADMLY_MGT = 0b1111;

        public static readonly Map<short, string> Admly = new Map<short, string>
        {
            {0, null},
            {ADMLY_SAL, "销售"},
            {ADMLY_OPN, "运营"},
            {ADMLY_FIN, "财务"},
            {ADMLY_MGT, "统管"},
        };

        public const short
            ORGLY_ = 0b0001, // common
            ORGLY_OPN = 0b0011,
            ORGLY_SAL = 0b0101,
            ORGLY_FIN = 0b1001,
            ORGLY_MGT = 0b1111;

        public static readonly Map<short, string> Orgly = new Map<short, string>
        {
            {0, null},
            {ORGLY_SAL, "销售"},
            {ORGLY_OPN, "运营"},
            {ORGLY_FIN, "财务"},
            {ORGLY_MGT, "统管"},
        };

        public const short
            INSERT = TYP | STATUS | LABEL | CREATE | BASIC,
            UPDATE = STATUS | LABEL | ADAPT | BASIC;


        internal int id;
        internal string tel;
        internal string im;

        // later
        internal string credential;
        internal short admly;
        internal int orgid;
        internal short orgly;
        internal string idcard;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((proj & BASIC) == BASIC)
            {
                s.Get(nameof(tel), ref tel);
                s.Get(nameof(im), ref im);
            }
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(credential), ref credential);
                s.Get(nameof(admly), ref admly);
                s.Get(nameof(orgid), ref orgid);
                s.Get(nameof(orgly), ref orgly);
                s.Get(nameof(idcard), ref idcard);
            }
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            if ((proj & BASIC) == BASIC)
            {
                s.Put(nameof(tel), tel);
                s.Put(nameof(im), im);
            }
            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(credential), credential);
                s.Put(nameof(admly), admly);
                s.Put(nameof(orgid), orgid);
                s.Put(nameof(orgly), orgly);
                s.Put(nameof(idcard), idcard);
            }
        }

        public int Key => id;

        public bool IsProfessional => typ >= 1;

        public short Sex
        {
            get
            {
                if (idcard == null)
                {
                    return 0;
                }
                var num = idcard[16] - '0';
                return (short) (num % 2 == 1 ? 1 : 2);
            }
        }

        public DateTime Birth
        {
            get
            {
                if (idcard == null)
                {
                    return default;
                }
                var n = idcard;
                int year = (n[6] - '0') * 1000 + (n[7] - '0') * 100 + (n[8] - '0') * 10 + (n[9] - '0') * 1;
                int month = (n[10] - '0') * 10 + (n[11] - '0') * 1;
                int day = (n[12] - '0') * 10 + (n[13] - '0') * 1;
                return new DateTime(year, month, day);
            }
        }

        public bool IsCertified => !string.IsNullOrEmpty(idcard);

        public override string ToString() => name;
    }
}