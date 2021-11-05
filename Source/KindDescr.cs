using SkyChain;
using static Revital.Org;

namespace Revital
{
    public abstract class KindDescr : IKeyable<short>
    {
        public static readonly Map<short, KindDescr> All = new Map<short, KindDescr>()
        {
            new AgriKindDescr
            {
                kind = KIND_AGRICTR,
                name = "生态农产"
            },
            new DietaryKindDescr
            {
                kind = KIND_DIETARYCTR,
                name = "调养膳食"
            },
            new HomeKindDescr
            {
                kind = KIND_HOMECTR,
                name = "家政陪护"
            },
            new PostKindDescr
            {
                kind = KIND_POSTCTR,
                name = "包裹快递"
            },
            new AdKindDescr
            {
                kind = KIND_ADCTR,
                name = "广告宣传"
            },
            new CharityKindDescr
            {
                kind = KIND_CHARITYCTR,
                name = "志愿公益"
            }
        };

        short kind;

        string name;


        public short Key => kind;

        public override string ToString() => name;
    }

    // agricultural
    public class AgriKindDescr : KindDescr
    {
    }

    // dietary recipe
    public class DietaryKindDescr : KindDescr
    {
    }

    // home service
    public class HomeKindDescr : KindDescr
    {
    }

    // parcel post
    public class PostKindDescr : KindDescr
    {
    }

    // advertising
    public class AdKindDescr : KindDescr
    {
    }

    // advertising
    public class CharityKindDescr : KindDescr
    {
    }
}