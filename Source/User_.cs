using SkyChain;

namespace Revital.Supply
{
    public class User_ : _Bean, IKeyable<int>
    {
        public static readonly User_ Empty = new User_();

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
            ADMLY_MART_ = 0x01,
            ADMLY_MART_OP = 0x03,
            ADMLY_MART_SPR = 0x05,
            ADMLY_MART_MGT = 0x0f,
            ADMLY_SUPLLY_ = 0x10,
            ADMLY_SUPLLY_OP = 0x30,
            ADMLY_SUPLLY_SPR = 0x50,
            ADMLY_SUPLLY_MGT = 0xf0;

        public static readonly Map<short, string> Admly = new Map<short, string>
        {
            {0, null},
            {ADMLY_MART_OP, "市场业务"},
            {ADMLY_MART_SPR, "市场监察"},
            {ADMLY_MART_MGT, "系统管理"},
            {ADMLY_SUPLLY_OP, "供应链业务"},
            {ADMLY_SUPLLY_SPR, "供应链监察"},
            {ADMLY_SUPLLY_MGT, "系统管理"},
        };

        public const short
            ORGLY_ = 0b0001,
            ORGLY_OP = 0b0011,
            ORGLY_SPR = 0b0101,
            ORGLY_MGR = 0b1111;

        public static readonly Map<short, string> Orgly = new Map<short, string>
        {
            {0, null},
            {ORGLY_, "来宾"},
            {ORGLY_OP, "业务"},
            {ORGLY_SPR, "监察"},
            {ORGLY_MGR, "管理"},
        };

        internal int id;
        internal string tel;
        internal string im;
        internal string credential;
        internal short admly;
        internal int orgid;
        internal short orgly;

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
            }
        }

        public int Key => id;

        public bool IsProfessional => typ >= 1;

        public bool IsDisabled => status <= STA_DISABLED;

        public override string ToString() => name;
    }
}