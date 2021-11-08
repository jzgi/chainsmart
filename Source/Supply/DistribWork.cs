using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital.Supply
{
    [UserAuthorize(Org.TYP_BIZ, 1)]
    [Ui("商户进货", "cart")]
    public class BizlyDistribWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyDistribVarWork>();
        }

        [Ui("购物车", kind: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Distrib.Empty).T(" FROM ups WHERE partyid = @1 AND status = 0 ORDER BY id");
            var arr = await dc.QueryAsync<Distrib>(p => p.Set(orgid));

            var items = ObtainMap<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD(items[o.itemid].name);
                    h.TD(o.qty);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("当前", kind: 2), Tool(Anchor)]
        public async Task buys(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Distrib.Empty).T(" FROM downs WHERE partyid = @1 AND status >= ").T(Distrib.STATUS_SUBMITTED).T(" ORDER BY id");
            var arr = await dc.QueryAsync<Distrib>(p => p.Set(orgid));

            var items = ObtainMap<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD(items[o.itemid].name);
                    h.TD(o.qty);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("历史", kind: 4), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Distrib.Empty).T(" FROM downs WHERE partyid = @1 AND status >= ").T(Distrib.STATUS_SUBMITTED).T(" ORDER BY id");
            var arr = await dc.QueryAsync<Distrib>(p => p.Set(orgid));

            var items = ObtainMap<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD(items[o.itemid].name);
                    h.TD(o.qty);
                    h.TDFORM(() => { });
                });
            });
        }


        [Ui("✚", "添加", kind: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    short typ = wc.Query[nameof(typ)];

                    h.FORM_().FIELDSUL_();
                    h.LI_().SELECT(null, nameof(typ), typ, Item.Typs, refresh: true)._LI();

                    if (typ > 0)
                    {
                        var prods = ObtainMap<short, Supply>();
                        for (int i = 0; i < prods?.Count; i++)
                        {
                            var o = prods.ValueAt(i);
                            if (o.typ != typ)
                            {
                                continue;
                            }

                            h.LI_().T(o.name)._LI();
                        }
                    }

                    h._FIELDSUL();
                    h.BOTTOMBAR_();

                    h._BOTTOMBAR();
                    h._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Distrib>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO buys ").colset(Distrib.Empty, 0)._VALUES_(Distrib.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }
}