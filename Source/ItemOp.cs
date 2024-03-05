using System;
using ChainFX;

namespace ChainSmart;

public struct ItemOp : IData, IKeyable<DateTime>
{
    public static readonly Map<short, string> Typs = new()
    {
        { 1, "进仓 ＋" },
        { 2, "出仓 －" },
        { 3, "盘盈 ＋" },
        { 4, "盘亏 －" },
        { 5, "增益 ＋" },
        { 6, "损耗 －" },
        { 7, "冲加 ＋" },
        { 8, "冲减 －" },
    };


    public DateTime dt;

    public int qty; // can be negative

    public int stock;

    public short typ;

    public string by;

    public void Read(ISource s, short msk = 0xff)
    {
        s.Get(nameof(dt), ref dt);
        s.Get(nameof(qty), ref qty);
        s.Get(nameof(stock), ref stock);
        s.Get(nameof(typ), ref typ);
        s.Get(nameof(by), ref by);
    }

    public void Write(ISink s, short msk = 0xff)
    {
        s.Put(nameof(dt), dt);
        s.Put(nameof(qty), qty);
        s.Put(nameof(stock), stock);
        s.Put(nameof(typ), typ);
        s.Put(nameof(by), by);
    }

    public DateTime Key => dt;

    public bool IsAdd() => typ % 2 != 0;

    public static bool IsAddOp(short optyp) => optyp % 2 != 0;
}