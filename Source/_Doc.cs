using System;
using SkyChain;

namespace Zhnt
{
    /// <summary>
    /// A business document with workflow process capability.
    /// </summary>
    public abstract class _Doc : IData
    {
        public const short
            STATUS_DRAFT = 0,
            STATUS_ISSUED = 1,
            STATUS_ARGUED = 2, // user interected
            STATUS_CLOSED = 3, // after clearing
            STATUS_ABORTED = 4;

        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STATUS_DRAFT, "草稿"},
            {STATUS_ISSUED, "启动"},
            {STATUS_ARGUED, "质疑"},
            {STATUS_CLOSED, "关闭"},
            {STATUS_ABORTED, "撤销"},
        };


        internal short typ;

        internal short status;

        internal short orgid;

        internal DateTime issued;

        internal DateTime ended;

        internal short span;

        internal string name;

        internal string tag;

        public virtual void Read(ISource s, byte proj = 0x0f)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(ended), ref ended);
            s.Get(nameof(issued), ref issued);
            s.Get(nameof(span), ref span);
            s.Get(nameof(name), ref name);
            s.Get(nameof(tag), ref tag);
        }

        public virtual void Write(ISink s, byte proj = 0x0f)
        {
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);
            s.Put(nameof(orgid), orgid);
            s.Put(nameof(ended), ended);
            s.Put(nameof(issued), issued);
            s.Put(nameof(span), span);
            s.Put(nameof(name), name);
            s.Put(nameof(tag), tag);
        }


        public abstract bool IsShowable { get; }

        public abstract bool IsWorkable { get; }
    }
}