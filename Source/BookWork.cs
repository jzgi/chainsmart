using System;
using System.Threading.Tasks;
using SkyChain;
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
    }

    [Ui("进货管理", "cart", forkie: Item.TYP_AGRI)]
    public class AgriBizlyBookWork : BizlyBookWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AgriBizlyBookVarWork>();
        }

        [Ui("常规", group: 1), Tool(Anchor)]
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

        [Ui("远期", group: 2), Tool(Anchor)]
        public async Task pre(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE fromid = @1 AND status >= ").T(Book.STA_SUBMITTED).T(" ORDER BY id");
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


        [Ui("✚", "常规进货", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Book.Empty).T(" FROM plans WHERE orgid = @1 AND status > = ").T(Book.STA_SUBMITTED).T(" ORDER BY id");
                var arr = await dc.QueryAsync<Book>(p => p.Set(org.ctrid));
                wc.GivePane(200, h =>
                {
                    short typ = wc.Query[nameof(typ)];

                    h.FORM_().FIELDSUL_();
                    h.LI_().SELECT(null, nameof(typ), typ, Item.Cats, refresh: true)._LI();

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

        [Ui("✚", "远期进货", group: 2), Tool(ButtonOpen)]
        public async Task newpre(WebContext wc)
        {
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    short typ = wc.Query[nameof(typ)];

                    h.FORM_().FIELDSUL_();
                    h.LI_().SELECT(null, nameof(typ), typ, Item.Cats, refresh: true)._LI();

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

    [Ui("进货管理", forkie: Item.TYP_DIETARY)]
    public class DietaryBizlyBookWork : BizlyBookWork
    {
        protected override void OnMake()
        {
            MakeVarWork<DietaryBizlyBookVarWork>();
        }

        [Ui("购物车", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE bizid = @1 AND status = 0 ORDER BY id");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            var items = ObtainMap<int, Item>();
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
    }

    [UserAuthorize(Org.TYP_CTR, User.ORGLY_)]
    public abstract class CtrlyBookWork : BookWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyBookVarWork>();
        }

        [Ui("常规", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }

        [Ui("以往", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            short orgid = wc[-1];
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }

        [Ui("远期", group: 4), Tool(Anchor)]
        public async Task pre(WebContext wc, int page)
        {
            short orgid = wc[-1];
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }

        [Ui("以往", group: 8), Tool(Anchor)]
        public async Task prepast(WebContext wc, int typ)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-1];
        }

        [Ui("新建常规", group: 1), Tool(ButtonOpen)]
        public async Task copy(WebContext wc)
        {
            short orgid = wc[-1];
            var prin = (User) wc.Principal;
            var ended = DateTime.Today.AddDays(3);
            int[] key;
            if (wc.IsGet)
            {
                key = wc.Query[nameof(key)];
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("目标截止日期");
                    h.LI_().DATE("截止", nameof(ended), ended)._LI();
                    h._FIELDSUL();
                    h.HIDDENS(nameof(key), key);
                    h.BOTTOM_BUTTON("确认", nameof(copy));
                    h._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ended = f[nameof(ended)];
                key = f[nameof(key)];
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO lots (typ, status, orgid, issued, ended, span, name, tag, tip, unit, unitip, price, min, max, least, step, extern, addr, start, author, icon, img) SELECT typ, 0, orgid, issued, @1, span, name, tag, tip, unit, unitip, price, min, max, least, step, extern, addr, start, @2, icon, img FROM lots WHERE orgid = @3 AND id")._IN_(key);
                await dc.ExecuteAsync(p => p.Set(ended).Set(prin.name).Set(orgid).SetForIn(key));

                wc.GivePane(201);
            }
        }
    }

    [Ui("供应分拣管理", "sign-out", forkie: Item.TYP_AGRI)]
    public class AgriCtrlyBookWork : CtrlyBookWork
    {
    }

    [Ui("供应分拣管理", "sign-out", forkie: Item.TYP_DIETARY)]
    public class DietaryCtrlyBookWork : CtrlyBookWork
    {
    }

    [Ui("供应分拣管理", "sign-out", forkie: Item.TYP_FACTORY)]
    public class FactoryCtrlyBookWork : CtrlyBookWork
    {
    }

    [Ui("供应分派管理", "sign-out", forkie: Item.TYP_CARE)]
    public class CareCtrlyBookWork : CtrlyBookWork
    {
    }

    [Ui("公益分派管理", "sign-out", forkie: Item.TYP_CHARITY)]
    public class CharityCtrlyBookWork : CtrlyBookWork
    {
    }

    [Ui("传媒派发管理", "sign-out", forkie: Item.TYP_AD)]
    public class AdCtrlyBookWork : CtrlyBookWork
    {
    }
}