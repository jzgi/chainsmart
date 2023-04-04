using System;
using ChainFx;

namespace ChainSmart
{
    public class Clear : IData, IKeyable<int>
    {
        public static readonly Clear Empty = new();

        public const short
            TYP_PLAT = 1,
            TYP_SHP = 3,
            TYP_MKT = 4,
            TYP_SRC = 5,
            TYP_ZON = 6,
            TYP_CTR = 7;

        public static readonly Map<short, string> Typs = new()
        {
            { TYP_PLAT, "平台服务" },
            { TYP_SHP, "线上销售" },
            { TYP_MKT, "市场服务" },
            { TYP_SRC, "线上销售" },
            { TYP_CTR, "中库服务" },
        };

        internal int id;
        internal int orgid;
        internal DateTime dt;
        internal short typ;
        internal string name;
        internal short trans;
        internal decimal amt;
        internal short rate;
        internal decimal topay;
        internal decimal pay;

        public void Read(ISource s, short msk = 0xff)
        {
            s.Get(nameof(id), ref id);
            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(dt), ref dt);
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(name), ref name);
            s.Get(nameof(trans), ref trans);
            s.Get(nameof(amt), ref amt);
            s.Get(nameof(rate), ref rate);
            s.Get(nameof(topay), ref topay);
            s.Get(nameof(pay), ref pay);
        }

        public void Write(ISink s, short msk = 0xff)
        {
        }

        public int Key => id;
    }
}