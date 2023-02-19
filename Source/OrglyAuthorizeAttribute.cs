using System;
using ChainFX.Web;

namespace ChainSMart
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class OrglyAuthorizeAttribute : AuthorizeAttribute
    {
        // org typ requirement (bitwise) 
        readonly short orgtyp;

        // org role requirement (bitwise)
        readonly short orgly;


        public OrglyAuthorizeAttribute(short orgtyp = 0, short orgly = 1)
        {
            this.orgtyp = orgtyp;
            this.orgly = orgly;
        }

        public override bool Do(WebContext wc, bool mock)
        {
            var prin = (User) wc.Principal;

            var seg = wc[typeof(OrglyVarWork)];
            var org = seg.As<Org>();

            // var and task group check
            if ((org.typ & orgtyp) != orgtyp)
            {
                return false;
            }

            var (dive, role) = prin.GetRoleForOrg(org, orgtyp);
            if ((role & orgly) == orgly)
            {
                if (!mock)
                {
                    wc.Dive = dive;
                    wc.Role = role;
                }
                return true;
            }

            return false;
        }
    }
}