using System;
using ChainFX.Web;

namespace ChainSmart;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class MgtAuthorizeAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Used for the management service. 
    /// </summary>
    /// <param name="accessTyp">-1 = adm; 0 = any; 1 and above represents org</param>
    /// <param name="role"></param>
    public MgtAuthorizeAttribute(short accessTyp, short role = 1)
    {
        AccessTyp = accessTyp;
        Role = role;
    }

    public override bool DoCheck(WebContext wc, out bool super)
    {
        var prin = (User)wc.Principal;
        var role = Role;
        super = false;

        if (prin.admly > 0) // admly has all
        {
            return (prin.admly & role) == role;
        }

        var seg = wc[typeof(MgtVarWork)];
        var org = seg.As<Org>();

        var atyp = AccessTyp;

        if ((atyp & Org.TYP_RTL_) == Org.TYP_RTL_)
        {
            if ((prin.rtlly & role) == role)
            {
                if (prin.rtlid == org.id)
                {
                    return true;
                }
                if (prin.rtlid == org.parentid && org.trust) // parent org
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
        else if ((atyp & Org.TYP_SUP_) == Org.TYP_SUP_)
        {
            if ((prin.suply & role) == role)
            {
                if (prin.supid == org.id)
                {
                    return true;
                }
                if (prin.supid == org.parentid && org.trust)
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