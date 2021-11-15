using System;
using SkyChain;

namespace Revital
{
    /// <summary>
    /// An abstract workflow document data object.
    /// </summary>
    public abstract class _DocX : _Doc
    {
        // public const short
        //     STA_DISABLED = 0,
        //     STA_SHOWABLE = 1,
        //     STA_WORKABLE = 2,
        //     STA_PREFERABLE = 3;

        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STA_DISABLED, "禁用"},
            {STA_SHOWABLE, "可展示"},
            {STA_WORKABLE, "可使用"},
            {STA_PREFERABLE, "可优先"},
        };

        internal int sprid;
        internal int fromid;
        internal int toid;
        internal int ccid;
        internal DateTime closed;
        internal string closer;


        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);
            s.Get(nameof(sprid), ref sprid);
            s.Get(nameof(fromid), ref fromid);
            s.Get(nameof(toid), ref toid);
            s.Get(nameof(ccid), ref ccid);
            s.Get(nameof(closed), ref closed);
            s.Get(nameof(closer), ref closer);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            base.Write(s, proj);
            s.Put(nameof(sprid), sprid);
            s.Put(nameof(fromid), fromid);
            s.Put(nameof(toid), toid);
            s.Put(nameof(ccid), ccid);
            s.Put(nameof(closed), closed);
            s.Put(nameof(closer), closer);
        }
    }
}