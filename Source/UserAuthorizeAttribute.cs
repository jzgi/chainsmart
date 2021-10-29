using System;
using SkyChain.Chain;
using SkyChain.Web;

namespace Revital.Supply
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
            var prin = (User_) wc.Principal;

            if (prin == null)
            {
                return false;
            }

            if (orgtyp > 0 && orgly > 0)
            {
                if ((prin.orgly & orgly) != orgly) return false; // inclusive check
                int orgid = wc[typeof(OrglyVarWork)];
                if (orgid != 0 && prin.orgid == orgid)
                {
                    var org = Chain.Obtain<int, Org_>(prin.orgid);
                    if (org != null)
                    {
                        return (org.typ & orgtyp) > 0; // inclusive
                    }
                }
                return false;
            }

            if (admly > 0)
            {
                return (prin.admly & admly) == admly;
            }

            return true;
        }
    }
}