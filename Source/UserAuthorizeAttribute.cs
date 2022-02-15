using System;
using SkyChain.Web;

namespace Revital
{
    /// <summary>
    /// To implement principal authorization of access to the target resources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class UserAuthorizeAttribute : AuthorizeAttribute
    {
        // org typ requirement (bitwise) 
        readonly short orgtyp;

        // org role requirement (bitwise)
        readonly short orgly;

        // platform admin role requirement (bitwise)
        readonly short admly;


        public UserAuthorizeAttribute(short orgtyp = 0, short orgly = 0, short admly = 0)
        {
            this.orgtyp = orgtyp;
            this.orgly = orgly;
            this.admly = admly;
        }

        public override bool Do(WebContext wc, bool mock)
        {
            var prin = (User) wc.Principal;

            if (prin == null) // auth required
            {
                return false;
            }

            if (admly > 0)
            {
                return (prin.admly & admly) == admly;
            }

            // require sign-in only
            if (orgtyp == 0 || orgly == 0) return true;

            // check access to org
            var org = wc[typeof(OrglyVarWork)].As<Org>();

            // is the manager of the org
            if (org.mgrid == prin.id)
            {
                if (!mock)
                {
                    wc.Party = org.name;
                    wc.Role = "管理员";
                }
                return true;
            }

            // is of any role for the org
            if (org.id == prin.orgid && (org.typ & orgtyp) == orgtyp && (prin.orgly & orgly) == orgly)
            {
                if (!mock)
                {
                    wc.Party = org.name;
                    wc.Role = User.Orgly[prin.orgly];
                }
                return true;
            }

            // is trusted for the org
            if (org.trust && org.sprid == prin.orgid)
            {
                if (!mock)
                {
                    wc.Party = org.name;
                    wc.Role = "代办";
                }
                return true;
            }

            return false;
        }
    }
}