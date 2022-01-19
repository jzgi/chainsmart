using SkyChain;

namespace Revital
{
    public class Ledger : _Info, IKeyable<long>
    {
        public static readonly Ledger Empty = new Ledger();

        public const short
            TYP_AGRI = 0b00000001, // agriculture
            TYP_FACT = 0b00000010, // factory
            TYP_SRVC = 0b00000100; // service


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_AGRI, "农副产"},
            {TYP_FACT, "制造品"},
            {TYP_SRVC, "泛服务"},
        };

        internal long id;

        internal int orgid;

        internal decimal arcv;

        internal decimal apay;

        public override void Read(ISource s, short proj = 0x0fff)
        {
            base.Read(s, proj);
        }

        public override void Write(ISink s, short proj = 0x0fff)
        {
            base.Write(s, proj);
        }

        public long Key => id;
    }
}