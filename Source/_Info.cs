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

        public const byte ID = 1, LATER = 2, PRIVACY = 4;

        internal short typ;
        internal short status;
        internal string name;
        internal string tip;
        internal DateTime created;
        internal string creator;
        internal DateTime adapted;
        internal string adapter;

        public virtual void Read(ISource s, byte proj = 0x0f)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(name), ref name);
            s.Get(nameof(tip), ref tip);
            s.Get(nameof(created), ref created);
            s.Get(nameof(creator), ref creator);
            s.Get(nameof(adapted), ref adapted);
            s.Get(nameof(adapter), ref adapter);
        }

        public virtual void Write(ISink s, byte proj = 0x0f)
        {
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);
            s.Put(nameof(name), name);
            s.Put(nameof(tip), tip);
            s.Put(nameof(created), created);
            s.Put(nameof(creator), creator);
            s.Put(nameof(adapted), adapted);
            s.Put(nameof(adapter), adapter);
        }

        public virtual bool IsDisabled => status <= STA_GONE;

        public virtual bool IsShowable => status == STA_DISABLED;

        public virtual bool CanShow => status >= STA_DISABLED;

        public virtual bool IsWorkable => status == STA_ENABLED;

        public virtual bool CanWork => status >= STA_ENABLED;

        public virtual bool IsPreferable => status == STA_PREFERRED;
    }
}