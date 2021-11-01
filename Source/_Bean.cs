using System;
using SkyChain;

namespace Revital
{
    /// <summary>
    /// A publicly used model object.
    /// </summary>
    public abstract class _Bean : IData
    {
        public const short
            STA_DISABLED = 0,
            STA_SHOWABLE = 1,
            STA_WORKABLE = 2,
            STA_PREFERABLE = 3;

        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STA_DISABLED, "禁用"},
            {STA_SHOWABLE, "可展示"},
            {STA_WORKABLE, "可使用"},
            {STA_PREFERABLE, "可优先"},
        };

        public const byte ID = 1, LATER = 2, PRIVACY = 4;

        // the specialized extensible discriminator
        internal short typ;
        internal short status;
        internal string name;
        internal string tip;
        internal DateTime created;
        internal string creator;
        internal DateTime modified;
        internal string modifier;

        public virtual void Read(ISource s, byte proj = 0x0f)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(name), ref name);
            s.Get(nameof(tip), ref tip);
            s.Get(nameof(created), ref created);
            s.Get(nameof(creator), ref creator);
            s.Get(nameof(modified), ref modified);
            s.Get(nameof(modifier), ref modifier);
        }

        public virtual void Write(ISink s, byte proj = 0x0f)
        {
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);
            s.Put(nameof(name), name);
            s.Put(nameof(tip), tip);
            s.Put(nameof(created), created);
            s.Put(nameof(creator), creator);
            s.Put(nameof(modified), modified);
            s.Put(nameof(modifier), modifier);
        }

        public virtual bool IsDisabled => status <= STA_DISABLED;

        public virtual bool IsShowable => status == STA_SHOWABLE;

        public virtual bool CanShow => status >= STA_SHOWABLE;

        public virtual bool IsWorkable => status == STA_WORKABLE;

        public virtual bool CanWork => status >= STA_WORKABLE;

        public virtual bool IsPreferable => status == STA_PREFERABLE;
    }
}