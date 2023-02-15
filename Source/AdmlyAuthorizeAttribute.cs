using System;
using ChainFx.Web;

namespace ChainSMart
{
    /// <summary>
    /// To implement principal authorization of access to the target resources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class AdmlyAuthorizeAttribute : AuthorizeAttribute
    {
        // platform admin role requirement (bitwise)
        readonly short admly;


        public AdmlyAuthorizeAttribute(short admly = 0)
        {
            this.admly = admly;
        }

        public override bool Do(WebContext wc, bool mock)
        {
            var prin = (User) wc.Principal;

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