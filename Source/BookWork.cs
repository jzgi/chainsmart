using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public class BookWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_BIZ, 1)]
    public abstract class BizlyBookWork : BookWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE fromid = @1 AND status = 0 ORDER BY id");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

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

        [Ui("过往", group: 2), Tool(Anchor)]
        public async Task pre(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE fromid = @1 AND status >= 1 ORDER BY id");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

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


        [Ui("✚", "进货", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Plan.Empty).T(" FROM plans WHERE orgid = @1 AND status > 0 ORDER BY typ, ");
                var arr = await dc.QueryAsync<Plan>(p => p.Set(org.ctrid));
                wc.GivePane(200, h =>
                {
                    short typ = wc.Query[nameof(typ)];

                    h.FORM_().FIELDSUL_();
                    if (typ > 0)
                    {
                        var prods = ObtainMap<short, Plan>();
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
                var o = await wc.ReadObjectAsync<Book>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO buys ").colset(Book.Empty, 0)._VALUES_(Book.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }

    [Ui("网上进货管理", "cart", forkie: Item.TYP_AGRI)]
    public class AgriBizlyBookWork : BizlyBookWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AgriBizlyBookVarWork>();
        }
    }

    [Ui("网上进货管理", forkie: Item.TYP_DIET)]
    public class DietBizlyBookWork : BizlyBookWork
    {
        protected override void OnMake()
        {
            MakeVarWork<DietBizlyBookVarWork>();
        }
    }

    [UserAuthorize(Org.TYP_CTR, User.ORGLY_)]
    public abstract class CtrlyBookWork : BookWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyBookVarWork>();
        }

        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT sprid, fromid, sum(pay) FROM books WHERE toid = @1 AND status = 1 GROUP BY sprid, fromid ORDER BY sprid, fromid");
            await dc.QueryAsync(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAIN_();
                int last = 0;
                while (dc.Next())
                {
                    dc.Let(out int sprid);
                    dc.Let(out int fromid);
                    dc.Let(out decimal sum);

                    if (sprid != last)
                    {
                        var spr = Obtain<int, Org>(sprid);
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    var from = Obtain<int, Org>(fromid);
                    h.TD(from.name);
                    h.TD_("uk-visible@l").T(sum)._TD();
                    h._TR();

                    last = sprid;
                }
                h._MAIN();
            });
        }

        [Ui("以往", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            short orgid = wc[-1];
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [Ui("供应分拣管理", "sign-out", forkie: Item.TYP_AGRI)]
    public class AgriCtrlyBookWork : CtrlyBookWork
    {
    }

    [Ui("供应分拣管理", "sign-out", forkie: Item.TYP_DIET)]
    public class DietCtrlyBookWork : CtrlyBookWork
    {
    }

    [Ui("供应分拣管理", "sign-out", forkie: Item.TYP_FACT)]
    public class FactCtrlyBookWork : CtrlyBookWork
    {
    }

    [Ui("供应分派管理", "sign-out", forkie: Item.TYP_CARE)]
    public class CareCtrlyBookWork : CtrlyBookWork
    {
    }

    [Ui("公益分派管理", "sign-out", forkie: Item.TYP_CHAR)]
    public class CharCtrlyBookWork : CtrlyBookWork
    {
    }

    [Ui("传媒派发管理", "sign-out", forkie: Item.TYP_ADVT)]
    public class AdvtCtrlyBookWork : CtrlyBookWork
    {
    }
}