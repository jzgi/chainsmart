using SkyChain;

namespace Zhnt
{
    /// <summary>
    /// An article record that is publishable thus has related conditions.
    /// </summary>
    public abstract class _Art : IData
    {
        public const short
            STATUS_DISABLED = 1,
            STATUS_SHOWABLE = 2,
            STATUS_WORKABLE = 3;

        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STATUS_DISABLED, "禁用"},
            {STATUS_SHOWABLE, "展示"},
            {STATUS_WORKABLE, "启用"}
        };

        public const byte ID = 1, LATER = 2;


        internal short typ;

        internal short status;

        internal string name;

        internal string tip;

        public virtual void Read(ISource s, byte proj = 0x0f)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(name), ref name);
            s.Get(nameof(tip), ref tip);
        }

        public virtual void Write(ISink s, byte proj = 0x0f)
        {
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);
            s.Put(nameof(name), name);
            s.Put(nameof(tip), tip);
        }

        public virtual bool IsShowable => status >= STATUS_SHOWABLE;

        public virtual bool IsWorkable => status >= STATUS_WORKABLE;
    }
}