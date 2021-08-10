using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;
using static Zhnt._Doc;
using static Zhnt.User;

namespace Zhnt.Market
{
    public class MyRoWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MyRoVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            var diets = Fetch<Map<short, Org>>();
            var orgs = Fetch<Map<short, Org>>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ro.Empty).T(" FROM orders WHERE uid = @1 AND status >= ").T(STATUS_ISSUED).T(" ORDER BY id DESC LIMIT 5 OFFSET 5 * @2");
            var arr = await dc.QueryAsync<Ro>(p => p.Set(prin.id).Set(page));
            Map<int, Ro> map = null;
            var ids = arr?.Exract(x => x.id);
            if (ids != null)
            {
                dc.Sql("SELECT ").collst(Ro.Empty).T(" FROM orderlgs WHERE orderid")._IN_(ids).T(" ORDER BY dt");
                map = dc.Query<int, Ro>(p => p.SetForIn(ids));
            }
        }
    }

    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("售货")]
    public class BizlyRoWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyRoVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            int orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ro.Empty).T(" FROM ros WHERE bizid = @1 AND status > 0 AND status < ").T(STATUS_CLOSED).T(" ORDER BY id DESC");
            var arr = await dc.QueryAsync<Ro>(p => p.Set(orgid));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    h.TD(o.pay, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }

        public async Task closed(WebContext wc)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ro.Empty).T(" FROM orders WHERE orgid = @1 AND status >= ").T(STATUS_CLOSED).T(" ORDER BY id DESC");
            var arr = await dc.QueryAsync<Ro>(p => p.Set(orgid));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    h.TD(o.pay, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }
    }
}