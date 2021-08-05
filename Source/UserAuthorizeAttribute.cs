using System;
using SkyChain;
using SkyChain.Web;
using Zhnt.Mart;
using Zhnt.Supply;
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

        readonly short bizly;

        readonly short orgly;

        readonly short typ;

        public UserAuthorizeAttribute(short admly = 0, short bizly = 0, short orgly = 0, short typ = 0)
        {
            this.admly = admly;
            this.bizly = bizly;
            this.orgly = orgly;
            this.typ = typ;
        }

        public override bool Do(WebContext wc)
        {
            var prin = (User) wc.Principal;

            if (prin == null) return false;

            if (admly > 0)
            {
                return (prin.admly & admly) == admly;
            }

            if (bizly > 0)
            {
                if ((prin.bizly & bizly) != bizly) // inclusive check
                {
                    return false;
                }
                short at = wc[typeof(BizlyVarWork)];
                if (at != 0 && prin.bizid == at)
                {
                    if (typ > 0)
                    {
                        var map = Fetch<Map<short, Biz>>();
                        var biz = map[prin.bizid];
                        if (biz != null)
                        {
                            return (biz.typ & typ) > 0; // inclusive
                        }
                    }
                    else // orgtyp not specified
                    {
                        return true;
                    }
                }
                return false;
            }

            if (orgly > 0)
            {
                if ((prin.orgly & orgly) != orgly) return false; // inclusive check
                short at = wc[typeof(OrglyVarWork)];
                if (at != 0 && prin.orgid == at)
                {
                    if (typ > 0)
                    {
                        var orgs = Fetch<Map<short, Org>>();
                        var org = orgs[prin.orgid];
                        if (org != null)
                        {
                            return (org.typ & typ) > 0; // inclusive
                        }
                    }
                    else // orgtyp not specified
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }
    }
}