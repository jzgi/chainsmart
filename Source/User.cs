using System;
using SkyChain;

namespace Revital
{
    public class User : _Art, IKeyable<int>
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
            ADMLY_ = 0b0001,
            ADMLY_ADT = 0b0011,
            ADMLY_OP = 0b0101,
            ADMLY_MGT = 0b1111;

        public static readonly Map<short, string> Admly = new Map<short, string>
        {
            {0, null},
            {ADMLY_, "访客"},
            {ADMLY_ADT, "审查"},
            {ADMLY_OP, "操作"},
            {ADMLY_MGT, "管理"},
        };

        public const short
            ORGLY_ = 0b0001,
            ORGLY_SPR = 0b0011,
            ORGLY_OP = 0b0101,
            ORGLY_MGR = 0b1111;

        public static readonly Map<short, string> Orgly = new Map<short, string>
        {
            {0, null},
            {ORGLY_, "访客"},
            {ORGLY_SPR, "审查"},
            {ORGLY_OP, "操作"},
            {ORGLY_MGR, "管理"},
        };

        internal int id;
        internal string tel;
        internal string im;
        internal string credential;
        internal short admly;
        internal int orgid;
        internal short orgly;
        internal string license;
        internal string idcard;
        internal bool icon;

        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(tel), ref tel);
            s.Get(nameof(im), ref im);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(credential), ref credential);
                s.Get(nameof(admly), ref admly);
                s.Get(nameof(orgid), ref orgid);
                s.Get(nameof(orgly), ref orgly);
                s.Get(nameof(license), ref license);
                s.Get(nameof(idcard), ref idcard);
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
            s.Put(nameof(tel), tel);
            s.Put(nameof(im), im);
            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(credential), credential);
                s.Put(nameof(admly), admly);
                s.Put(nameof(orgid), orgid);
                s.Put(nameof(orgly), orgly);
                s.Put(nameof(license), license);
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