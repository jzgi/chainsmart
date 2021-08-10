using System;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Db.DbEnviron;

namespace Zhnt
{
    /// <summary>
    /// To implement principal authorization of access to the target resources.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class UserAuthorizeAttribute : AuthorizeAttribute
    {
        readonly short admly;

        readonly short orgly;

        readonly short typ;

        public UserAuthorizeAttribute(short admly = 0, short orgly = 0, short typ = 0)
        {
            this.admly = admly;
            this.orgly = orgly;
            this.typ = typ;
        }

        public override bool Do(WebContext wc)
        {
            var prin = (User) wc.Principal;

            if (prin == null)
            {
                return false;
            }

            if (admly > 0)
            {
                return (prin.admly & admly) == admly;
            }

            if (typ > 0 && orgly > 0)
            {
                if ((prin.orgly & orgly) != orgly) return false; // inclusive check
                int orgid = wc[typeof(IOrglyVar)];
                if (orgid != 0 && prin.orgid == orgid)
                {
                    var orgs = Fetch<Map<int, Org>>();
                    var org = orgs[prin.orgid];
                    if (org != null)
                    {
                        return (org.typ & typ) > 0; // inclusive
                    }
                }
                return false;
            }

            return true;
        }
    }
}