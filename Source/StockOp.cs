using System;
using ChainFx;

namespace ChainSmart
{
    public struct StockOp : IData, IKeyable<DateTime>
    {
        public static readonly Map<short, string> Typs = new()
        {
            { 1, "＋" },
            { 2, "－" },
        };

        public static readonly string[] Tips = { "到货", "返库", "调拨", "盘点", "损耗" };


        public DateTime dt;

        public string tip;

        public int qty; // can be negative

        public int avail;

        public string by;

        public void Read(ISource s, short msk = 0xff)
        {
            s.Get(nameof(dt), ref dt);
            s.Get(nameof(tip), ref tip);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(avail), ref avail);
            s.Get(nameof(by), ref by);
        }

        public void Write(ISink s, short msk = 0xff)
        {
            s.Put(nameof(dt), dt);
            s.Put(nameof(tip), tip);
            s.Put(nameof(qty), qty);
            s.Put(nameof(avail), avail);
            s.Put(nameof(by), by);
        }

        public DateTime Key => dt;
    }
}