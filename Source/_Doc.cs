using System;
using SkyChain;

namespace Revital
{
    /// <summary>
    /// A data model for workflow document.
    /// </summary>
    public abstract class _Doc : _Art
    {
        public const short
            STA_CREATED = 1,
            STA_ABORTED = 2,
            STA_ADAPTED = 3,
            STA_HANDLED = 4;

        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {0, null},
            {STA_CREATED, "已提交"},
            {STA_ABORTED, "已撤销"},
            {STA_ADAPTED, "已接受"},
            {STA_HANDLED, "已处理"},
        };

        internal int sprid;
        internal int fromid;
        internal int toid;
        internal int ccid;
        internal DateTime handled;
        internal string handler;


        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);

            s.Get(nameof(sprid), ref sprid);
            s.Get(nameof(fromid), ref fromid);
            s.Get(nameof(toid), ref toid);
            s.Get(nameof(ccid), ref ccid);
            s.Get(nameof(handled), ref handled);
            s.Get(nameof(handler), ref handler);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            base.Write(s, proj);

            s.Put(nameof(sprid), sprid);
            s.Put(nameof(fromid), fromid);
            s.Put(nameof(toid), toid);
            s.Put(nameof(ccid), ccid);
            s.Put(nameof(handled), handled);
            s.Put(nameof(handler), handler);
        }
    }
}