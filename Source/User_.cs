using System;
using SkyChain;

namespace Revital.Supply
{
    public class User_ : IData, IKeyable<int>
    {
        public static readonly User_ Empty = new User_();

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
            ADMLY_MART_OP = 1,
            ADMLY_MART_MGT = 3,
            ADMLY_SUPLLY_OP = 4,
            ADMLY_SUPLLY_MGT = 12;

        public static readonly Map<short, string> Admly = new Map<short, string>
        {
            {0, null},
            {ADMLY_MART_OP, "市场业务管理"},
            {ADMLY_MART_MGT, "市场服务器管理"},
            {ADMLY_SUPLLY_OP, "供应业务管理"},
            {ADMLY_SUPLLY_MGT, "供应服务器管理"},
        };

        public const short
            ORGLY_GUEST = 1,
            ORGLY_OP = 3,
            ORGLY_MGR = 7;

        public static readonly Map<short, string> Orgly = new Map<short, string>
        {
            {0, null},
            {ORGLY_GUEST, "访客"},
            {ORGLY_OP, "操作员"},
            {ORGLY_MGR, "管理员"},
        };

        public const short
            STA_DISABLED = 0,
            STA_NORMAL = 1;


        internal short typ;
        internal short status;
        internal string name;
        internal string tip;
        internal DateTime created;
        internal string creator;

        internal int id;
        internal string tel;
        internal string im;
        internal string credential;
        internal short admly;
        internal int orgid;
        internal short orgly;

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
            s.Get(nameof(tel), ref tel);
            s.Get(nameof(im), ref im);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(credential), ref credential);
                s.Get(nameof(admly), ref admly);
                s.Get(nameof(orgid), ref orgid);
                s.Get(nameof(orgly), ref orgly);
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
            s.Put(nameof(tel), tel);
            s.Put(nameof(im), im);
            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(credential), credential);
                s.Put(nameof(admly), admly);
                s.Put(nameof(orgid), orgid);
                s.Put(nameof(orgly), orgly);
            }
        }

        public int Key => id;

        public bool IsPro => typ >= 1;

        public bool IsCertified => false;

        public bool IsDisabled => status <= STA_DISABLED;

        public override string ToString() => name;
    }
}