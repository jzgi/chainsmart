using System;
using ChainFX.Web;

namespace ChainSmart;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class MgtAuthorizeAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Used for the management service. 
    /// </summary>
    /// <param name="accessTyp">a bitwise composition, 0 = adm; 1 and above represents an org type</param>
    /// <param name="role">user role</param>
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
        var atyp = AccessTyp;


        if (atyp == 0) // only admin 
        {
            return prin.admly > 0 && (prin.admly & role) == role;
        }

        var seg = wc[typeof(MgtVarWork)];
        var org = seg.As<Org>();
        
        if ((org.typ & atyp) != atyp)
        {
            return false;
        }

        if ((atyp & Org.TYP_RTL_) == Org.TYP_RTL_) // check retail
        {
            if ((prin.mktly & role) == role)
            {
                if (prin.mktid == org.id)
                {
                    return true;
                }
                if (prin.mktid == org.parentid && org.trust) // parent org
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
        else if ((atyp & Org.TYP_WHL_) == Org.TYP_WHL_) // supply
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