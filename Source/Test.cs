using ChainFX;

namespace ChainSMart
{
    /// <summary>
    /// An event logged targeted to certain org
    /// </summary>
    public class Test : Entity
    {
        public static readonly Test Empty = new Test();

        public const short
            TYP_MRT = 1,
            TYP_PRV = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_MRT, "市场"},
            {TYP_PRV, "供给"},
        };

        internal int id;
        internal int orgid;
        internal int credit;

        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(orgid), ref orgid);
                s.Get(nameof(credit), ref credit);
            }
        }

        public override void Write(ISink s, short msk = 0xff)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Put(nameof(orgid), orgid);
                s.Put(nameof(credit), credit);
            }
        }
    }
}