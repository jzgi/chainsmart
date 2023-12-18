using System;
using ChainFX.Web;

namespace ChainSmart;

/// <summary>
/// To implement principal authorization of access to the target resources.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class MyAuthorizeAttribute : AuthorizeAttribute
{
    // user type 
    readonly short typ;


    public MyAuthorizeAttribute(short typ = 0) : base(0)
    {
        this.typ = typ;
    }

    public override bool DoCheck(WebContext wc, out bool super)
    {
        var prin = (User)wc.Principal;

        super = false;

        // if meet typ
        if (typ == 0 || (prin.typ & typ) == typ)
        {
            return true;
        }

        return false;
    }
}