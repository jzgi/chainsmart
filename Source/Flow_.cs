using System;
using SkyChain;

namespace Supply
{
    /// <summary>
    /// A business document used in workflow process.
    /// </summary>
    public abstract class Flow_ : IData
    {
        public const short
            STATUS_CREATED = 0,
            STATUS_SUBMITTED = 1, // before processing
            STATUS_ABORTED = 2,
            STATUS_CONFIRMED = 3, // ready for distr center op 
            STATUS_SHIPPED = 4, //  
            STATUS_CLOSED = 5; // after clearing

        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STATUS_CREATED, "草稿中"},
            {STATUS_SUBMITTED, "已提交"},
            {STATUS_ABORTED, "已撤销"},
            {STATUS_CONFIRMED, "已确认"},
            {STATUS_SHIPPED, "已发货"},
            {STATUS_CLOSED, "已关闭"},
        };


        // extensible discriminator
        internal short typ;

        // current workflow status
        internal short status;

        // cooperative party
        internal short partyid;

        // bound distribution center
        internal short ctrid;

        internal DateTime created;

        internal string creator;

        internal DateTime traded;

        internal string trader;

        internal DateTime settled;

        internal string settler;

        public virtual void Read(ISource s, byte proj = 0x0f)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(partyid), ref partyid);
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
            s.Put(nameof(created), created);
            s.Put(nameof(creator), creator);
        }
    }
}