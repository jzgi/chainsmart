using System;
using ChainFx;

namespace ChainSmart
{
    public struct StockOp : IData, IKeyable<DateTime>
    {
        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            { 1, "到货＋" },
            { 2, "返库＋" },
            { 3, "调拨＋" },
            { 4, "盘盈＋" },
            { 5, "提库－" },
            { 6, "损耗－" },
            { 7, "调拨－" },
            { 8, "盘亏－" },
        };


        public DateTime dt;

        public short typ;

        public string tip;

        public int qty;

        public int avail;

        public string by;

        public void Read(ISource s, short msk = 0xff)
        {
            s.Get(nameof(dt), ref dt);
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(tip), ref tip);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(avail), ref avail);
            s.Get(nameof(by), ref by);
        }

        public void Write(ISink s, short msk = 0xff)
        {
            s.Put(nameof(dt), dt);
            s.Put(nameof(typ), typ);
            s.Put(nameof(tip), tip);
            s.Put(nameof(qty), qty);
            s.Put(nameof(avail), avail);
            s.Put(nameof(by), by);
        }

        public DateTime Key => dt;
    }
}