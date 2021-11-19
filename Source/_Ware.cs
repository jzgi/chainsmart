using SkyChain;

namespace Revital
{
    /// <summary>
    /// A data model for mercandise item that is for sale.
    /// </summary>
    public abstract class _Ware : _Article
    {
        // public const short
        //     STA_DISABLED = 0,
        //     STA_SHOWABLE = 1,
        //     STA_WORKABLE = 2,
        //     STA_PREFERABLE = 3;
        //
        // public static readonly Map<short, string> Statuses = new Map<short, string>
        // {
        //     {STA_DISABLED, "禁用"},
        //     {STA_SHOWABLE, "可展示"},
        //     {STA_WORKABLE, "可使用"},
        //     {STA_PREFERABLE, "可优先"},
        // };
        //
        // public const byte ID = 1, LATER = 2, PRIVACY = 4;

        // the specialized extensible discriminator
        internal int orgid;
        internal short itemid;
        internal short cat;
        internal string ext;
        internal string unit;
        internal short unitx;
        internal short min;
        internal short max;
        internal short step;
        internal decimal price;
        internal decimal off;

        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);

            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(cat), ref cat);
            s.Get(nameof(ext), ref ext);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitx), ref unitx);
            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
            s.Get(nameof(step), ref step);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            base.Write(s, proj);

            s.Put(nameof(orgid), orgid);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(cat), cat);
            s.Put(nameof(ext), ext);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(step), step);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
        }
    }
}