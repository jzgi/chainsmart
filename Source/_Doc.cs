using System;
using SkyChain;

namespace Zhnt.Supply
{
    /// <summary>
    /// A business document used in workflow process.
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


        // extensible discriminator
        internal short typ;

        // current workflow status
        internal short status;

        // cooperative party
        internal short partyid;

        // doc number
        internal int no;

        // bound distribution center
        internal short ctrid;

        internal DateTime created;

        internal string creator;

        public virtual void Read(ISource s, byte proj = 0x0f)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(partyid), ref partyid);
            s.Get(nameof(no), ref no);
            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(created), ref created);
            s.Get(nameof(creator), ref creator);
        }

        public virtual void Write(ISink s, byte proj = 0x0f)
        {
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);
            s.Put(nameof(partyid), partyid);
            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(no), no);
            s.Put(nameof(created), created);
            s.Put(nameof(creator), creator);
        }
    }
}