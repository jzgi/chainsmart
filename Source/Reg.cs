using System;
using SkyChain;

namespace Revital.Supply
{
    public class Reg : IData, IKeyable<short>
    {
        public static readonly Reg Empty = new Reg();

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

        public const short
            TYP_METROPOLIS = 1,
            TYP_CITY = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_METROPOLIS, "省会"},
            {TYP_CITY, "地市"},
        };

        internal short typ;
        internal short status;
        internal string name;
        internal string tip;
        internal DateTime created;
        internal string creator;
        internal short id;
        internal short idx;

        public void Read(ISource s, byte proj = 0x0f)
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
            s.Get(nameof(idx), ref idx);
        }

        public void Write(ISink s, byte proj = 0x0f)
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
            s.Put(nameof(idx), idx);
        }

        public short Key => id;

        public bool IsMetropolis => typ == 1;

        public bool IsCity => typ == 2;

        public override string ToString() => name;
    }
}