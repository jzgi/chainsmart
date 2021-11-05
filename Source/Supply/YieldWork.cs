using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;
using static Revital.User;

namespace Revital.Supply
{
    [UserAuthorize(admly: 1)]
    [Ui("产品管理")]
    public class AdmlyYieldWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyProdVarWork>();
        }

        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }

        [Ui("以往", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
        }
    }


    [UserAuthorize(Org.TYP_SRC, ORGLY_OP)]
    [Ui("产源产品")]
    public class SrclyYieldWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<SrclyProdVarWork>();
        }

        [Ui("当前"), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purchase.Empty).T(" FROM purchs WHERE partyid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Purchase>(p => p.Set(orgid));

            wc.GivePage(200, h => { h.TOOLBAR(caption: "来自平台的订单"); });
        }

        [Ui("历史"), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            int orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purchase.Empty).T(" FROM purchs WHERE partyid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Purchase>(p => p.Set(orgid));

            wc.GivePage(200, h => { h.TOOLBAR(caption: "来自平台的订单"); });
        }
    }

    [UserAuthorize(Org.TYP_FRM, ORGLY_OP)]
    [Ui("产源团产品动态")]
    public class SrcColyYieldWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<SrcColyProdVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Yield.Empty).T(" FROM prods WHERE srcid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Yield>(p => p.Set(orgid));

            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }
}