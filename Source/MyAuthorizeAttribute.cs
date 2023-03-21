using System;
using ChainFx.Web;

namespace ChainSmart
{
    /// <summary>
    /// To implement principal authorization of access to the target resources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class MyAuthorizeAttribute : AuthorizeAttribute
    {
        // user type 
        readonly short typ;


        public MyAuthorizeAttribute(short typ = 0)
        {
            this.typ = typ;
        }

        public override bool Do(WebContext wc, bool mock)
        {
            var prin = (User)wc.Principal;

            // if meet typ
            if (typ == 0 || (prin.typ & typ) == typ)
            {
                return true;
            }

            return false;
        }
    }
}