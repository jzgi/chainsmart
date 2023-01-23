using System;
using ChainFx;

namespace ChainMart
{
    public struct WareOp : IData, IKeyable<DateTime>
    {
        public const short
            TYP_ENTRY = 1,
            TYP_SURPASS = 2,
            TYP_ELSEADD = 3,
            TYP_WASTE = 5,
            TYP_LOSS = 6,
            TYP_ELSERDC = 7;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_ENTRY, "＋（入库）"},
            {TYP_SURPASS, "＋（盘盈）"},
            {TYP_ELSEADD, "＋（其它）"},
            {TYP_WASTE, "－（损耗）"},
            {TYP_LOSS, "－（盘亏）"},
            {TYP_ELSERDC, "－（其它）"},
        };


        public DateTime dt;

        public short typ;

        public decimal remain;

        public decimal qty;

        public string by;


        public void Read(ISource s, short msk = 0xff)
        {
            s.Get(nameof(dt), ref dt);
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(remain), ref remain);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(by), ref by);
        }

        public void Write(ISink s, short msk = 0xff)
        {
            s.Put(nameof(dt), dt);
            s.Put(nameof(typ), typ);
            s.Put(nameof(remain), remain);
            s.Put(nameof(qty), qty);
            s.Put(nameof(by), by);
        }

        public DateTime Key => dt;
    }
}