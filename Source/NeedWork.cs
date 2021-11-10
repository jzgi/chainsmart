using System.Threading.Tasks;
using SkyChain.Web;
using static Revital.User;

namespace Revital
{
    public abstract class NeedWork : WebWork
    {
    }

    public class MyNeedWork : NeedWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MyNeedVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Need.Empty).T(" FROM needs WHERE uid = @1 AND status > 0  ORDER BY id DESC LIMIT 5 OFFSET 5 * @2");
            var arr = await dc.QueryAsync<Need>(p => p.Set(prin.id).Set(page));
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

    [UserAuthorize(orgly: ORGLY_OP)]
    public abstract class BizlyNeedWork : NeedWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyNeedVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Need.Empty).T(" FROM needs WHERE bizid = @1 AND status > 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Need>(p => p.Set(org.id));
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
            dc.Sql("SELECT ").collst(Need.Empty).T(" FROM needs WHERE orgid = @1 AND status >= ORDER BY id DESC");
            var arr = await dc.QueryAsync<Need>(p => p.Set(orgid));
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
    public class AgriBizlyNeedWork : BizlyNeedWork
    {
    }

    [Ui("客户预订管理", forkie: Org.FRK_DIETARY)]
    public class DietaryBizlyNeedWork : BizlyNeedWork
    {
    }
}