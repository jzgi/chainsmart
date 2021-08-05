using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;
using static Zhnt._Doc;
using static Zhnt.User;

namespace Zhnt.Mart
{
    public abstract class SoWork : WebWork
    {
    }

    public class MySoWork : SoWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MySoVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            var diets = Fetch<Map<short, Biz>>();
            var orgs = Fetch<Map<short, Biz>>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(So.Empty).T(" FROM orders WHERE uid = @1 AND status >= ").T(STATUS_ISSUED).T(" ORDER BY id DESC LIMIT 5 OFFSET 5 * @2");
            var arr = await dc.QueryAsync<So>(p => p.Set(prin.id).Set(page));
            Map<int, OrderLg> map = null;
            var ids = arr?.Exract(x => x.id);
            if (ids != null)
            {
                dc.Sql("SELECT ").collst(OrderLg.Empty).T(" FROM orderlgs WHERE orderid")._IN_(ids).T(" ORDER BY dt");
                map = dc.Query<int, OrderLg>(p => p.SetForIn(ids));
            }
        }
    }

    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("采购")]
    public class OrglyBuyWork : SoWork
    {
        protected override void OnMake()
        {
            MakeVarWork<OrglyBuyVarWork>();
        }

        [Ui("当前"), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(So.Empty).T(" FROM orders WHERE orgid = @1 AND status > 0 AND status < ").T(STATUS_CLOSED).T(" ORDER BY id DESC");
            var arr = await dc.QueryAsync<So>(p => p.Set(orgid));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    h.TD(o.name, "uk-text-small");
                    h.TD(o.pay, true);
                    h.TD_("uk-text-center").A_HREF_(o.id, "/dtl")._ONCLICK_("return dialog(this,8,false,1,'')").T(o.ended, 2, 0)._A()._TD();
                    // h.TD(Statuses[o.status]);
                });
            });
        }

        [Ui("过去"), Tool(Anchor)]
        public async Task closed(WebContext wc)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(So.Empty).T(" FROM orders WHERE orgid = @1 AND status >= ").T(STATUS_CLOSED).T(" ORDER BY id DESC");
            var arr = await dc.QueryAsync<So>(p => p.Set(orgid));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    h.TD(o.name, "uk-text-small");
                    h.TD(o.pay, true);
                    h.TD_("uk-text-center").A_HREF_(o.id, "/dtl")._ONCLICK_("return dialog(this,8,false,1,'')").T(o.ended, 2, 0)._A()._TD();
                    // h.TD(Statuses[o.status]);
                });
            });
        }
    }

    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("订单")]
    public class OrglyOrderWork : SoWork
    {
        const int TODAY_WEEK = 3;

        const int MAX_WEEK = 21;

        protected override void OnMake()
        {
            MakeVarWork<OrglyOrderVarWork>();
        }

        public async Task @default(WebContext wc, int day)
        {
            if (day <= 0)
            {
                wc.Subscript = day = TODAY_WEEK;
            }
            var map = Fetch<Map<short, Plan>>();
            short orgid = wc[-1];
            var today = DateTime.Today;
            var dt = today.AddDays(day - TODAY_WEEK);
            using var dc = NewDbContext();
        }
    }

    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("作业")]
    public class OrglyJobWork : SoWork
    {
        const int TODAY_WEEK = 3;

        const int MAX_WEEK = 14;

        protected override void OnMake()
        {
            MakeVarWork<OrglySoDistVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            if (page <= 0)
            {
                wc.Subscript = page = TODAY_WEEK;
            }
            var map = Fetch<Map<short, Plan>>();
            short ptid = wc[-1];
            var today = DateTime.Today;
            var dt = today.AddDays(page - TODAY_WEEK);

            using var dc = NewDbContext();
        }
    }
}