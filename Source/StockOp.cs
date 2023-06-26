using System;
using ChainFx;

namespace ChainSmart;

public struct StockOp : IData, IKeyable<DateTime>
{
    public static readonly Map<short, string> Typs = new()
    {
        { 1, "进货 ＋" },
        { 2, "出货 －" },
        { 3, "调进 ＋" },
        { 4, "调出 －" },
        { 5, "虚增 ＋" },
        { 6, "虚减 －" },
        { 7, "补偿 ＋" },
        { 8, "损耗 －" },
        { 9, "盘盈 ＋" },
        { 10, "盘亏 －" },
        { 11, "冲回 ＋" },
        { 12, "冲去 －" },
    };


    public DateTime dt;

    public int qty; // can be negative

    public int stock;

    public short typ;

    public string by;

    public int hub; // the target hub, if applied

    public void Read(ISource s, short msk = 0xff)
    {
        s.Get(nameof(dt), ref dt);
        s.Get(nameof(qty), ref qty);
        s.Get(nameof(stock), ref stock);
        s.Get(nameof(typ), ref typ);
        s.Get(nameof(by), ref by);
        s.Get(nameof(hub), ref hub);
    }

    public void Write(ISink s, short msk = 0xff)
    {
        s.Put(nameof(dt), dt);
        s.Put(nameof(qty), qty);
        s.Put(nameof(stock), stock);
        s.Put(nameof(typ), typ);
        s.Put(nameof(by), by);
        s.Put(nameof(hub), hub);
    }

    public DateTime Key => dt;

    public bool IsAdd() => typ % 2 != 0;

    public static bool IsAddOp(short optyp) => optyp % 2 != 0;
}