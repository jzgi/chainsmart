using SkyChain;

namespace Zhnt.Supply
{
    public class User : _Art, IKeyable<int>
    {
        public static readonly User Empty = new User();


        public const byte TYP_CONSULTANT = 1, TYP_COOK = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {0, null},
            {TYP_CONSULTANT, "调养顾问"},
            {TYP_COOK, "调养厨师"},
        };

        public const short ADMLY_SAL = 1, ADMLY_PUR = 2, ADMLY_FIN = 4, ADMLY_MGT = 15;

        public static readonly Map<short, string> Admly = new Map<short, string>
        {
            {ADMLY_SAL, "销售部"},
            {ADMLY_PUR, "采购部"},
            {ADMLY_FIN, "财务部"},
            {ADMLY_MGT, "管理部"},
        };

        public const short CTRLY_OP = 1, CTRLY_MGR = 15;

        public static readonly Map<short, string> Ctrly = new Map<short, string>
        {
            {0, null},
            {CTRLY_OP, "操作员"},
            {CTRLY_MGR, "管理员"},
        };

        public const short ORGLY_OP = 1, ORGLY_MGR = 15;

        public static readonly Map<short, string> Orgly = new Map<short, string>
        {
            {0, null},
            {ORGLY_OP, "操作员"},
            {ORGLY_MGR, "管理员"},
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
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            base.Read(s, proj);

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
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            base.Write(s, proj);

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

        public bool IsDisabled => status <= STATUS_DISABLED;

        public override string ToString() => name;
    }
}