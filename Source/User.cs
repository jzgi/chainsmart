using System;
using SkyChain;

namespace Zhnt
{
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

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {0, null},
            {TYP_CONSULTANT, "调养顾问"},
            {TYP_COOK, "调养厨师"},
        };

        public const short
            ADMLY = 1,
            ADMLY_SAL = 3,
            ADMLY_PUR = 5,
            ADMLY_MGT = 15;

        public static readonly Map<short, string> Admly = new Map<short, string>
        {
            {0, null},
            {ADMLY, "基本"},
            {ADMLY_SAL, "销售"},
            {ADMLY_PUR, "采购"},
            {ADMLY_MGT, "管理"},
        };

        public const short CTRLY_OP = 1, CTRLY_MGR = 7;

        public static readonly Map<short, string> Ctrly = new Map<short, string>
        {
            {0, null},
            {CTRLY_OP, "操作"},
            {CTRLY_MGR, "管理"},
        };

        public const short ORGLY_OP = 1, ORGLY_MGR = 7;

        public static readonly Map<short, string> Orgly = new Map<short, string>
        {
            {0, null},
            {ORGLY_OP, "操作"}, // 001
            {ORGLY_MGR, "管理"}, // 011
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
        internal int refid;

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
                s.Get(nameof(refid), ref refid);
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
                s.Put(nameof(refid), refid);
            }
        }

        public int Key => id;


        public bool IsPro => typ >= 1;

        public bool IsDisabled => status <= STATUS_DISABLED;

        public override string ToString() => name;
    }
}