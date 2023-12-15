using System;
using ChainFX.Web;

namespace ChainSmart;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class UserAuthorizeAttribute : AuthorizeAttribute
{
    // org role requirement (bitwise)
    readonly short role;

    // org typ requirement (bitwise) 
    readonly short orgtyp;

    // user level
    readonly int ulevel;

    /// <summary>
    /// Used for the management service. 
    /// </summary>
    /// <param name="orgtyp">0 = adm, 1 and above represents org</param>
    /// <param name="role"></param>
    /// <param name="ulevel">user level, 1 = adm, 2 = mid, 4 = biz</param>
    public UserAuthorizeAttribute(short orgtyp, short role = 1, int ulevel = 0)
    {
        this.orgtyp = orgtyp;
        this.role = role;
        this.ulevel = ulevel;
    }

    public override bool Do(WebContext wc, bool mock)
    {
        var prin = (User)wc.Principal;

        if (orgtyp == 0)
        {
            // admly required
            if (role > 0)
            {
                if (!mock)
                {
                    wc.Role = prin.admly;
                }

                return (prin.admly & role) == role;
            }

        }

        var seg = wc[typeof(ZonlyVarWork)];
        var org = seg.As<Org>();

        // var and task group check
        if ((org.typ & orgtyp) != orgtyp)
        {
            return false;
        }

        var ret = prin.GetRoleForOrg(org, out var super, out var ulvl);
        if ((role & ret) == role && (ulevel == 0 || (ulevel | ulvl) == ulvl))
        {
            if (!mock)
            {
                wc.Super = super;
                wc.Role = ret;
            }

            return true;
        }

        return false;
    }
}