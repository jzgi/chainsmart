using System;
using ChainFx.Web;

namespace ChainSmart
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class OrglyAuthorizeAttribute : AuthorizeAttribute
    {
        // org role requirement (bitwise)
        readonly short role;

        // org typ requirement (bitwise) 
        readonly short typ;

        // user level
        readonly int ulevel;

        /// <summary>
        /// Used for the management service. 
        /// </summary>
        /// <param name="typ">0 = adm, 1 and above represents org</param>
        /// <param name="role"></param>
        /// <param name="ulevel">user level, 1 = adm, 2 = mid, 4 = biz</param>
        public OrglyAuthorizeAttribute(short typ, short role = 1, int ulevel = 0)
        {
            this.role = role;
            this.typ = typ;
            this.ulevel = ulevel;
        }

        public override bool Do(WebContext wc, bool mock)
        {
            var prin = (User)wc.Principal;

            var seg = wc[typeof(OrglyVarWork)];
            var org = seg.As<Org>();

            // var and task group check
            if ((org.typ & typ) != typ)
            {
                return false;
            }

            var rl = prin.GetRoleForOrg(org, out var super, out var ulvl);
            if ((role & rl) == role && (ulevel == 0 || ulevel == ulvl))
            {
                if (!mock)
                {
                    wc.Super = super;
                    wc.Role = role;
                }

                return true;
            }

            return false;
        }
    }
}