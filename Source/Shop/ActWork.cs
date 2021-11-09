using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static Revital.User;

namespace Revital.Shop
{
    public abstract class ActWork : WebWork
    {
    }

    public class MyActWork : ActWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MyActVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            var diets = ObtainMap<short, Org>();
            var orgs = ObtainMap<short, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Act.Empty).T(" FROM acts WHERE uid = @1 AND status >=  ORDER BY id DESC LIMIT 5 OFFSET 5 * @2");
            var arr = await dc.QueryAsync<Act>(p => p.Set(prin.id).Set(page));
            Map<int, Act> map = null;
        }
    }

    [UserAuthorize(orgly: ORGLY_OP)]
    public abstract class BizlyActWork : ActWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyActVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Act.Empty).T(" FROM bids WHERE bizid = @1 AND status > 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Act>(p => p.Set(org.id));
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
            dc.Sql("SELECT ").collst(Act.Empty).T(" FROM orders WHERE orgid = @1 AND status >= ORDER BY id DESC");
            var arr = await dc.QueryAsync<Act>(p => p.Set(orgid));
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

    [Ui("客户订单管理", forkie: Org.FRK_AGRI)]
    public class AgriBizlyActWork : BizlyActWork
    {
    }

    [Ui("客户预订管理", forkie: Org.FRK_DIETARY)]
    public class DietaryBizlyActWork : BizlyActWork
    {
    }
}