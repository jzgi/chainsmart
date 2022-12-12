using System;
using ChainFx.Web;

namespace ChainMart
{
    /// <summary>
    /// To implement principal authorization of access to the target resources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class OrglyAuthorizeAttribute : AuthorizeAttribute
    {
        // org typ requirement (bitwise) 
        readonly short orgtyp;

        // org role requirement (bitwise)
        readonly short orgly;


        public OrglyAuthorizeAttribute(short orgtyp = 0, short orgly = 0)
        {
            this.orgtyp = orgtyp;
            this.orgly = orgly;
        }

        public override bool Do(WebContext wc, bool mock)
        {
            var prin = (User) wc.Principal;

            if (prin == null) // auth required
            {
                return false;
            }

            // sign-in required
            if (orgtyp == 0 || orgly == 0)
            {
                return true;
            }
            var seg = wc[typeof(OrglyVarWork)];
            var org = seg.As<Org>();
            // var and task group check
            if ((org.typ & orgtyp) != orgtyp)
            {
                return false;
            }

            var (down, role) = prin.GetRoleForOrg(org, orgtyp);
            if ((role & orgly) == orgly)
            {
                if (!mock)
                {
                    wc.Dive = down;
                    wc.Role = role;
                }
                return true;
            }

            return false;
        }
    }
}