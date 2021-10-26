using System;
using SkyChain;

namespace Revital.Supply
{
    /// <summary>
    /// The data modal for a yield of item.
    /// </summary>
    public class Yield : IData, IKeyable<int>
    {
        public static readonly Yield Empty = new Yield();

        public const short
            STA_DISABLED = 0,
            STA_SHOWED = 1,
            STA_ENABLED = 2,
            STA_PREFERED = 3;

        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STA_DISABLED, "禁用"},
            {STA_SHOWED, "展示"},
            {STA_ENABLED, "可用"},
            {STA_PREFERED, "优先"},
        };

        public const byte ID = 1, LATER = 2, PRIVACY = 4;

        internal short typ;
        internal short status;
        internal string name;
        internal string tip;
        internal DateTime created;
        internal string creator;

        internal short id;
        internal short itemid;


        public void Read(ISource s, byte proj = 15)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);
            s.Get(nameof(name), ref name);
            s.Get(nameof(tip), ref tip);
            s.Get(nameof(created), ref created);
            s.Get(nameof(creator), ref creator);
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            s.Get(nameof(itemid), ref itemid);
        }

        public void Write(ISink s, byte proj = 15)
        {
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);
            s.Put(nameof(name), name);
            s.Put(nameof(tip), tip);
            s.Put(nameof(created), created);
            s.Put(nameof(creator), creator);
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            s.Put(nameof(itemid), itemid);
        }

        public int Key => id;
    }
}