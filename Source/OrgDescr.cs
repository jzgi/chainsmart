using SkyChain;
using static Revital.Org;

namespace Revital
{
    public abstract class OrgDescr : IKeyable<short>
    {
        public static readonly Map<short, OrgDescr> All = new Map<short, OrgDescr>()
        {
            new AgriOrgDescr
            {
                typ = FRK_AGRI,
                name = "生态农产"
            },
            new DietaryOrgDescr
            {
                typ = FRK_DIETARY,
                name = "调养膳食"
            },
            new FactoryOrgDescr
            {
                typ = FRK_HOME,
                name = "工业产品"
            },
            new CareOrgDescr
            {
                typ = FRK_CARE,
                name = "家政陪护"
            },
            new AdOrgDescr
            {
                typ = FRK_AD,
                name = "广告宣传"
            },
            new CharityOrgDescr
            {
                typ = FRK_CHARITY,
                name = "志愿公益"
            }
        };

        short typ;

        string name;

        public short Key => typ;

        public override string ToString() => name;
    }

    // agricultural
    public class AgriOrgDescr : OrgDescr
    {
    }

    // dietary recipe
    public class DietaryOrgDescr : OrgDescr
    {
    }

    // home utilities
    public class HomeOrgDescr : OrgDescr
    {
    }

    // gardening
    public class FactoryOrgDescr : OrgDescr
    {
    }

    // care service
    public class CareOrgDescr : OrgDescr
    {
    }

    // parcel post
    public class PostOrgDescr : OrgDescr
    {
    }

    // advertising
    public class AdOrgDescr : OrgDescr
    {
    }

    // advertising
    public class CharityOrgDescr : OrgDescr
    {
    }
}