using System;
using ChainFx.Web;

namespace ChainMart
{
    /// <summary>
    /// To implement principal authorization of access to the target resources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class AdmlyAuthorizeAttribute : AuthorizeAttribute
    {
        // org typ requirement (bitwise) 
        readonly short orgtyp;

        // org role requirement (bitwise)
        readonly short orgly;

        // platform admin role requirement (bitwise)
        readonly short admly;


        public AdmlyAuthorizeAttribute(short admly = 0)
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

            // admly required
            if (admly > 0)
            {
                if (!mock)
                {
                    wc.Role = prin.admly;
                }
                return (prin.admly & admly) == admly;
            }

            return false;
        }
    }
}