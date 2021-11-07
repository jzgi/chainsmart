using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static Revital.User;

namespace Revital.Shop
{
    public class MyBidWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MyBidVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            var diets = ObtainMap<short, Org>();
            var orgs = ObtainMap<short, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bid.Empty).T(" FROM orders WHERE uid = @1 AND status >=  ORDER BY id DESC LIMIT 5 OFFSET 5 * @2");
            var arr = await dc.QueryAsync<Bid>(p => p.Set(prin.id).Set(page));
            Map<int, Bid> map = null;
            var ids = arr?.Exract(x => x.id);
            if (ids != null)
            {
                dc.Sql("SELECT ").collst(Bid.Empty).T(" FROM orderlgs WHERE orderid")._IN_(ids).T(" ORDER BY dt");
                map = dc.Query<int, Bid>(p => p.SetForIn(ids));
            }
        }
    }

    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("订单或参与")]
    public class BizlyBidWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyBidVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            int orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bid.Empty).T(" FROM ros WHERE bizid = @1 AND status > 0 AND status < ORDER BY id DESC");
            var arr = await dc.QueryAsync<Bid>(p => p.Set(orgid));
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
            dc.Sql("SELECT ").collst(Bid.Empty).T(" FROM orders WHERE orgid = @1 AND status >= ORDER BY id DESC");
            var arr = await dc.QueryAsync<Bid>(p => p.Set(orgid));
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