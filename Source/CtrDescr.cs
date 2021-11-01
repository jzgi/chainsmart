using SkyChain;
using static Revital.Org;

namespace Revital
{
    public class CtrDescr : IKeyable<short>
    {
        public static readonly Map<short, CtrDescr> All = new Map<short, CtrDescr>()
        {
            new AgriCtrDescr
            {
                kind = KIND_AGRICTR,
                name = "农副供应中心"
            },
            new DietaryCtrDescr
            {
                kind = KIND_DIETARYCTR,
                name = "膳食烹制中心"
            },
            new PostCtrDescr
            {
                kind = KIND_POSTCTR,
                name = "包裹分拣中心"
            },
            new HomeCtrDescr
            {
                kind = KIND_HOMECTR,
                name = "家政服务中心"
            }
        };

        short kind;

        string name;


        public short Key => kind;

        public override string ToString() => name;
    }

    public class AgriCtrDescr : CtrDescr
    {
    }

    public class DietaryCtrDescr : CtrDescr
    {
    }

    public class PostCtrDescr : CtrDescr
    {
    }

    public class HomeCtrDescr : CtrDescr
    {
    }
}