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
                typ = KIND_AGRICTR,
                name = "生态农产"
            },
            new DietaryOrgDescr
            {
                typ = KIND_DIETARYCTR,
                name = "调养膳食"
            },
            new FactoryOrgDescr
            {
                typ = KIND_HOMECTR,
                name = "工业产品"
            },
            new CareOrgDescr
            {
                typ = KIND_CARECTR,
                name = "家政陪护"
            },
            new AdOrgDescr
            {
                typ = KIND_ADCTR,
                name = "广告宣传"
            },
            new CharityOrgDescr
            {
                typ = KIND_CHARITYCTR,
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