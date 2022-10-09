using System.Threading.Tasks;
using ChainFx.Web;
using static ChainMart.User;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class BuyWork : WebWork
    {
    }

    [Ui("我的消费订单", "账号功能")]
    public class MyBuyWork : BuyWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<MyBuyVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE uid = @1 AND status > 0  ORDER BY id DESC LIMIT 10 OFFSET 10 * @2");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(prin.id).Set(page));

            wc.GivePage(200, h =>
            {
                if (arr == null)
                {
                    h.ALERT("尚无消费订单");
                    return;
                }

                h.GRID(arr, o => { h.T(o.name); });

                h.PAGINATION(arr?.Length > 10);
            });
        }
    }


    [UserAuthorize(orgly: ORGLY_OPN)]
    [Ui("线上销售", "商户")]
    public class ShplyBuyWork : BuyWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<ShplyBuyVarWork>();
        }

        [Ui("当前外卖", group: 1), Tool(Modal.Anchor)]
        public async Task @default(WebContext wc)
        {
            var shp = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE shpid = @1 AND status > 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(shp.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    // h.TD(o.mrtname, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }

        [Ui("⌹", "历史外卖", group: 2), Tool(Modal.Anchor)]
        public async Task closed(WebContext wc)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE toid = @1 AND status > 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(orgid));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    // h.TD(o.mrtname, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }
    }

    [UserAuthorize(Org.TYP_MRT, 1)]
    [Ui("线上销售送货", "市场")]
    public class MktlyBuyWork : BuyWork
    {
        [Ui("当前", @group: 1), Tool(Modal.Anchor)]
        public async Task @default(WebContext wc)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }
}