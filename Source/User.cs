using System;
using ChainFx;

namespace ChainMart
{
    public class User : Entity, IKeyable<int>
    {
        public static readonly User Empty = new User();

        // pro types
        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {0, null},
            {1, "健康市场运营师"},
            {2, "健康管理师"},
        };

        public const short
            ADMLY_ = 0b000001, // common
            ADMLY_OPN = 0b000011,
            ADMLY_LOG = 0b000101,
            ADMLY_MON = 0b001001,
            ADMLY_FIN = 0b010001,
            ADMLY_MGT = 255;

        public static readonly Map<short, string> Admly = new Map<short, string>
        {
            {ADMLY_OPN, "作业"},
            {ADMLY_LOG, "物流"},
            {ADMLY_MON, "质检"},
            {ADMLY_FIN, "财务"},
            {ADMLY_MGT, "管理"},
        };

        public const short
            ORGLY_ = 0b000001, // common
            ORGLY_OPN = 0b000011,
            ORGLY_LOG = 0b000101,
            ORGLY_MON = 0b001001,
            ORGLY_FIN = 0b010001,
            ORGLY_MGT = 255;

        public static readonly Map<short, string> Orgly = new Map<short, string>
        {
            {0, null},
            {ORGLY_OPN, "作业"},
            {ORGLY_LOG, "物流"},
            {ORGLY_MON, "质检"},
            {ORGLY_FIN, "财务"},
            {ORGLY_MGT, "管理"},
        };

        internal int id;
        internal string tel;
        internal string im;

        // later
        internal string credential;
        internal short admly;
        internal int orgid;
        internal short orgly;
        internal string idcard;
        internal bool icon;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((proj & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(tel), ref tel);
                s.Get(nameof(im), ref im);
            }
            if ((proj & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(credential), ref credential);
                s.Get(nameof(admly), ref admly);
                s.Get(nameof(orgid), ref orgid);
                s.Get(nameof(orgly), ref orgly);
                s.Get(nameof(idcard), ref idcard);
                s.Get(nameof(icon), ref icon);
            }
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(tel), tel);
            s.Put(nameof(im), im);
            if ((proj & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(credential), credential);
                s.Put(nameof(admly), admly);
                s.Put(nameof(orgid), orgid);
                s.Put(nameof(orgly), orgly);
                s.Put(nameof(idcard), idcard);
                s.Put(nameof(icon), icon);
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