using System;
using SkyChain;

namespace Zhnt.Supply
{
    /// <summary>
    /// A publicly used article record that has lifetime statuses
    /// </summary>
    public abstract class _Art : IData
    {
        public const short
            STATUS_DISABLED = 0,
            STATUS_SHOWABLE = 1,
            STATUS_WORKABLE = 2;

        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STATUS_DISABLED, "禁用"},
            {STATUS_SHOWABLE, "展示"},
            {STATUS_WORKABLE, "启用"}
        };

        public const byte ID = 1, LATER = 2, PRIVACY = 4;

        // the specialized extensible discriminator
        internal short typ;

        // object status
        internal short status;

        // readable name
        internal string name;

        // desctiprive text
        internal string tip;

        internal DateTime created;

        // persona who created or lastly modified
        internal string creator;

        public virtual void Read(ISource s, byte proj = 0x0f)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(name), ref name);
            s.Get(nameof(tip), ref tip);
            s.Get(nameof(created), ref created);
            s.Get(nameof(creator), ref creator);
        }

        public virtual void Write(ISink s, byte proj = 0x0f)
        {
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);
            s.Put(nameof(name), name);
            s.Put(nameof(tip), tip);
            s.Put(nameof(created), created);
            s.Put(nameof(creator), creator);
        }

        public virtual bool IsShowable => status >= STATUS_SHOWABLE;

        public virtual bool IsWorkable => status >= STATUS_WORKABLE;
    }
}