using System.Threading.Tasks;
using Chainly.Web;
using static Revital.User;
using static Chainly.Nodal.Store;

namespace Revital
{
    public abstract class OrderWork : WebWork
    {
    }


    public class MyOrderWork : OrderWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<MyOrderVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Order.Empty).T(" FROM buys WHERE uid = @1 AND status > 0  ORDER BY id DESC LIMIT 5 OFFSET 5 * @2");
            var arr = await dc.QueryAsync<Order>(p => p.Set(prin.id).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    h.TD(o.mrtname, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }
    }

    [Ui("平台零售电商报告", "table")]
    public class AdmlyOrderWork : OrderWork
    {
        public async Task @default(WebContext wc)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [UserAuthorize(Org.TYP_MRT, 1)]
    [Ui("市场零售电商发货", "sign-out")]
    public class MrtlyOrderWork : OrderWork
    {
        [Ui("当前", group: 1), Tool(Modal.Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }


    [UserAuthorize(orgly: ORGLY_OPN)]
    [Ui("商户线上零售", "cloud-upload")]
    public class BizlyOrderWork : OrderWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<BizlyOrderVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Order.Empty).T(" FROM buys WHERE toid = @1 AND status > 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Order>(p => p.Set(org.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    h.TD(o.mrtname, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }

        public async Task closed(WebContext wc)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Order.Empty).T(" FROM buys WHERE toid = @1 AND status > 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Order>(p => p.Set(orgid));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    h.TD(o.mrtname, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }
    }
}