using System;
using ChainFx;

namespace ChainMart
{
    public struct StockOp : IData, IKeyable<DateTime>
    {
        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {1, "到货＋"},
            {2, "调拨＋"},
            {3, "盘盈＋"},
            {4, "其它＋"},
            {5, "损耗－"},
            {6, "调拨－"},
            {7, "盘亏－"},
            {8, "其它－"},
        };


        public DateTime dt;

        public short typ;

        public decimal qty;

        public decimal avail;

        public string by;


        public void Read(ISource s, short msk = 0xff)
        {
            s.Get(nameof(dt), ref dt);
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(avail), ref avail);
            s.Get(nameof(by), ref by);
        }

        public void Write(ISink s, short msk = 0xff)
        {
            s.Put(nameof(dt), dt);
            s.Put(nameof(typ), typ);
            s.Put(nameof(qty), qty);
            s.Put(nameof(avail), avail);
            s.Put(nameof(by), by);
        }

        public DateTime Key => dt;
    }
}