using System;
using ChainFX.Web;

namespace ChainSmart;

/// <summary>
/// To implement principal authorization of access to the target resources.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class MyAuthorizeAttribute : AuthorizeAttribute
{
    public MyAuthorizeAttribute(short typ = 0)
    {
        AccessTyp = typ;
    }

    public override bool DoCheck(WebContext wc, out bool super)
    {
        var prin = (User)wc.Principal;

        super = false;

        var typ = AccessTyp;
        if (typ == 0 || (prin.typ & typ) == typ)
        {
            return true;
        }

        return false;
    }
}