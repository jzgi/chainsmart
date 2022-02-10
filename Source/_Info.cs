using System;
using SkyChain;

namespace Revital
{
    /// <summary>
    /// A data model for object information.
    /// </summary>
    public abstract class _Info : IData
    {
        public const short
            STA_GONE = 0,
            STA_DISABLED = 1,
            STA_ENABLED = 2,
            STA_PREFERRED = 3;

        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STA_GONE, "注销"},
            {STA_DISABLED, "禁用"},
            {STA_ENABLED, "可用"},
            {STA_PREFERRED, "优先"},
        };

        public static readonly Map<short, string> Symbols = new Map<short, string>
        {
            {STA_GONE, "注销"},
            {STA_DISABLED, "禁用"},
            {STA_ENABLED, null},
            {STA_PREFERRED, null},
        };

        public const short
            ID = 0x0001,
            BASIC = 0x0002,
            LATER = 0x0004,
            TYP = 0x0008,
            STATUS = 0x0010,
            LABEL = 0x0020,
            CREATE = 0x0040,
            ADAPT = 0x0080;


        internal short typ;
        internal short status;
        internal string name;
        internal string tip;
        internal DateTime created;
        internal string creator;
        internal DateTime adapted;
        internal string adapter;

        public virtual void Read(ISource s, short proj = 0xff)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(name), ref name);
            s.Get(nameof(tip), ref tip);
            s.Get(nameof(created), ref created);
            s.Get(nameof(creator), ref creator);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(adapted), ref adapted);
                s.Get(nameof(adapter), ref adapter);
            }
        }

        public virtual void Write(ISink s, short proj = 0xff)
        {
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);
            s.Put(nameof(name), name);
            s.Put(nameof(tip), tip);
            s.Put(nameof(created), created);
            s.Put(nameof(creator), creator);
            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(adapted), adapted);
                s.Put(nameof(adapter), adapter);
            }
        }

        public virtual bool IsGone => status <= STA_GONE;

        public virtual bool IsDisabled => status == STA_DISABLED;

        public virtual bool CanShow => status >= STA_DISABLED;

        public virtual bool IsEnabled => status == STA_ENABLED;

        public virtual bool CanWork => status >= STA_ENABLED;

        public virtual bool IsPreferred => status == STA_PREFERRED;
    }
}