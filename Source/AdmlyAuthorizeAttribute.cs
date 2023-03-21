using System;
using ChainFx.Web;

namespace ChainSmart
{
    /// <summary>
    /// To implement principal authorization of access to the target resources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class AdmlyAuthorizeAttribute : AuthorizeAttribute
    {
        // platform admin role requirement (bitwise)
        readonly short role;


        public AdmlyAuthorizeAttribute(short role = 1)
        {
            this.role = role;
        }

        public override bool Do(WebContext wc, bool mock)
        {
            var prin = (User)wc.Principal;

            // admly required
            if (role > 0)
            {
                if (!mock)
                {
                    wc.Role = prin.admly;
                }

                return (prin.admly & role) == role;
            }

            return false;
        }
    }
}