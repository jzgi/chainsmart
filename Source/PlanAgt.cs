using SkyChain;

namespace Revital
{
    public class PlanAgt : _Art
    {
        public static readonly PlanAgt Empty = new PlanAgt();

        public const short
            TYP_DISTRIB = 1,
            TYP_VOID = 2;


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_DISTRIB, "供应关系"},
            {TYP_VOID, "未知"},
        };

        internal int planid;
        internal int bizid;

        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);

            s.Get(nameof(planid), ref planid);
            s.Get(nameof(bizid), ref bizid);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            base.Write(s, proj);

            s.Put(nameof(planid), planid);
            s.Put(nameof(bizid), bizid);
        }
    }
}