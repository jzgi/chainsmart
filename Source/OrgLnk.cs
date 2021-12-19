using SkyChain;

namespace Revital
{
    public class OrgLnk : _Art
    {
        public static readonly OrgLnk Empty = new OrgLnk();

        public const short
            TYP_DISTRIB = 1,
            TYP_VOID = 2;


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_DISTRIB, "供应关系"},
            {TYP_VOID, "未知"},
        };

        internal int ctrid;
        internal int mrtid;

        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);

            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(mrtid), ref mrtid);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            base.Write(s, proj);

            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(mrtid), mrtid);
        }
    }
}