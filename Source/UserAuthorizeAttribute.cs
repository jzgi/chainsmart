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

        public override bool Do(WebContext wc)
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

            if (orgtyp == 0 || orgly == 0) // require signin only
            {
                return true;
            }

            // check access to org
            var org = wc[typeof(OrglyVarWork)].As<Org>();

            if ((org.typ & orgtyp) == orgtyp && (prin.orgly & orgly) == orgly) return true;

            return org.trust && org.sprid == prin.orgid;
        }
    }
}