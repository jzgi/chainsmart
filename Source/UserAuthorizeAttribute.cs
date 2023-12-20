using System;
using ChainFX.Web;

namespace ChainSmart;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class UserAuthorizeAttribute : AuthorizeAttribute
{
    // org typ requirement (bitwise) 
    readonly short orgtyp;


    /// <summary>
    /// Used for the management service. 
    /// </summary>
    /// <param name="orgtyp">0 = adm, 1 and above represents org</param>
    /// <param name="role"></param>
    public UserAuthorizeAttribute(short orgtyp, short role = 1) : base(role)
    {
        this.orgtyp = orgtyp;
    }

    public override bool DoCheck(WebContext wc, out bool super)
    {
        var prin = (User)wc.Principal;
        var role = Role;
        super = false;

        if (orgtyp == 0) // admly required
        {
            if (role > 0)
            {
                return (prin.admly & role) == role;
            }
            return false;
        }

        var seg = wc[typeof(ZonlyVarWork)];
        var org = seg.As<Org>();

        if ((orgtyp & Org.TYP_RTL) == Org.TYP_RTL)
        {
            if ((prin.rtlly & role) == role)
            {
                if (prin.rtlid == org.id)
                {
                    return true;
                }
                if (prin.rtlid == org.upperid && org.trust) // upper org
                {
                    super = true;
                    return true;
                }
                if ((prin.admly & role) == role) // admin
                {
                    super = true;
                    return true;
                }
            }
        }
        else if ((orgtyp & Org.TYP_SUP) == Org.TYP_SUP)
        {
            if ((prin.suply & role) == role)
            {
                if (prin.supid == org.id)
                {
                    return true;
                }
                if (prin.supid == org.upperid && org.trust)
                {
                    super = true;
                    return true;
                }
                if ((prin.admly & role) == role) // admin
                {
                    super = true;
                    return true;
                }
            }
        }

        return false;
    }
}