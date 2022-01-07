using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public class BookWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_BIZ, 1)]
    [Ui("［商户］平台订货")]
    public class BizlyBookWork : BookWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books_ WHERE fromid = @1 AND status = 0 ORDER BY id");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            var items = Grab<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD(items[o.itemid].name);
                    h.TD(o.ctrid);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("以往", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE fromid = @1 AND status >= 1 ORDER BY id");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            var items = Grab<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD(items[o.itemid].name);
                    h.TD(o.ctrid);
                    h.TDFORM(() => { });
                });
            });
        }


        [Ui("✚", "新增进货", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            var ctr = GrabObject<int, Org>(2);
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Product.Empty).T(" FROM plans WHERE orgid = @1 AND status > 0 ORDER BY cat, status DESC");
                var arr = await dc.QueryAsync<Product>(p => p.Set(2));
                var prods = Grab<int, Product>();
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_(ctr.name);
                    short last = 0;
                    foreach (var o in arr)
                    {
                        if (o.cat != last)
                        {
                            h.LI_().SPAN_("uk-label").T(Item.Cats[o.cat])._SPAN()._LI();
                        }
                        h.LI_("uk-flex");
                        h.SPAN_("uk-width-1-4").T(o.name)._SPAN();
                        h.SPAN_("uk-visible@l").T(o.tip)._SPAN();
                        h.SPAN_().CNY(o.price, true).T("／").T(o.unit)._SPAN();
                        h.SPAN(Product.Typs[o.postg]);
                        h.SPAN(_Info.Statuses[o.status]);
                        h.BUTTON("✕", "", 1, onclick: "this.form.targid.value = ", css: "uk-width-micro uk-button-secondary");
                        h._LI();

                        last = o.cat;
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

    [UserAuthorize(Org.TYP_CTR, User.ORGLY_)]
    [Ui("供应－销售及分拣", "sign-out")]
    public abstract class PrvlyBookWork : BookWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
        }
    }

    [UserAuthorize(Org.TYP_SRC, 1)]
    [Ui("产源－订货管理")]
    public class SrclyBookWork : BookWork
    {
        protected override void OnMake()
        {
            MakeVarWork<SrclyBookVarWork>();
        }

        [Ui("当前订货"), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books_ WHERE srcid = @1 AND status > 0 ORDER BY id");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.Key);
                    h.TD(o.bizname);
                    h.TD(o.qty);
                });
            });
        }

        [Ui("以往订货"), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books_ WHERE srcid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h => { h.TOOLBAR(caption: "来自平台的订单"); });
        }
    }

    [UserAuthorize(Org.TYP_CTR, User.ORGLY_)]
    [Ui("［中心］收货管理", "sign-in")]
    public class CtrlyReceiveWork : BookWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
        }
    }

    [UserAuthorize(Org.TYP_CTR, User.ORGLY_)]
    public abstract class CtrlyBookWork : BookWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT sprid, fromid, sum(pay) FROM books WHERE ctrid = @1 AND status = 1 GROUP BY sprid, fromid ORDER BY sprid, fromid");
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
                        var spr = GrabObject<int, Org>(sprid);
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    var from = GrabObject<int, Org>(fromid);
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
                        var spr = GrabObject<int, Org>(sprid);
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    var from = GrabObject<int, Org>(fromid);
                    h.TD(from.name);
                    h.TD_("uk-visible@l").T(sum)._TD();
                    h._TR();

                    last = sprid;
                }
                h._MAIN();
            });
        }
    }


    [Ui("［中心］分拣管理", "sign-out", fork: Item.TYP_AGRI)]
    public class CtrlyAgriBookWork : CtrlyBookWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyAgriBookVarWork>();
        }
    }

    [Ui("［中心］分拣管理", "sign-out", fork: Item.TYP_FACT)]
    public class CtrlyFactBookWork : CtrlyBookWork
    {
    }

    [Ui("［中心］分拣管理", "sign-out", fork: Item.TYP_SRVC)]
    public class CtrlySrvcBookWork : CtrlyBookWork
    {
    }
}