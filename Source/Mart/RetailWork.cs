using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static Revital.User;

namespace Revital.Mart
{
    public class MySellWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MySellVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            var diets = ObtainMap<short, Org>();
            var orgs = ObtainMap<short, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Retail.Empty).T(" FROM orders WHERE uid = @1 AND status >=  ORDER BY id DESC LIMIT 5 OFFSET 5 * @2");
            var arr = await dc.QueryAsync<Retail>(p => p.Set(prin.id).Set(page));
            Map<int, Retail> map = null;
            var ids = arr?.Exract(x => x.id);
            if (ids != null)
            {
                dc.Sql("SELECT ").collst(Retail.Empty).T(" FROM orderlgs WHERE orderid")._IN_(ids).T(" ORDER BY dt");
                map = dc.Query<int, Retail>(p => p.SetForIn(ids));
            }
        }
    }

    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("售货")]
    public class BizlySellWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlySellVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            int orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Retail.Empty).T(" FROM ros WHERE bizid = @1 AND status > 0 AND status < ORDER BY id DESC");
            var arr = await dc.QueryAsync<Retail>(p => p.Set(orgid));
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
            dc.Sql("SELECT ").collst(Retail.Empty).T(" FROM orders WHERE orgid = @1 AND status >= ORDER BY id DESC");
            var arr = await dc.QueryAsync<Retail>(p => p.Set(orgid));
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