using System.Threading.Tasks;
using Chainly.Web;
using static Chainly.Nodal.Store;

namespace Revital
{
    public class StockWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_BIZ, 1)]
#if ZHNT
    [Ui("商户货架设置", icon: "album")]
#else
    [Ui("驿站货架设置", icon: "album")]
#endif
    public class BizlyStockWork : StockWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<BizlyStockVarWork>();
        }

        [Ui("在销售", group: 2), Tool(Modal.Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Stock.Empty).T(" FROM stocks WHERE bizid = @1 AND state  >= 1 ORDER BY id");
            var arr = await dc.QueryAsync<Stock>(p => p.Set(org.id));

            var items = Grab<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    // h.TD(items[o.itemid].name);
                    h.TD(o.id);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("已下架", group: 2), Tool(Modal.Anchor)]
        public async Task off(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Stock.Empty).T(" FROM stocks WHERE bizid = @1 AND state  >= 1 ORDER BY id");
            var arr = await dc.QueryAsync<Stock>(p => p.Set(org.id));

            var items = Grab<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    // h.TD(items[o.itemid].name);
                    h.TD(o.id);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("✚", "新增采购", group: 1), Tool(Modal.ButtonOpen)]
        public void @new(WebContext wc)
        {
            var mrt = wc[-1].As<Org>();
            wc.GiveRedirect("/" + mrt.ToCtrId + "/");
        }
    }
}