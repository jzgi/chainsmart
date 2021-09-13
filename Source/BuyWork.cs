using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;
using static Zhnt.Supply.User;

namespace Zhnt.Supply
{
    [UserAuthorize(admly: 1)]
    [Ui("销售订单")]
    public class AdmlyBuyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyBuyVarWork>();
        }

        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [UserAuthorize(Org.TYP_BIZ, 1)]
    [Ui("商户订货", "cart")]
    public class BizlyBuyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyBuyVarWork>();
        }

        [Ui("购物车"), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE partyid = @1 AND status = 0 ORDER BY id");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(orgid));

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

        [Ui("订货单"), Tool(Anchor)]
        public async Task buys(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE partyid = @1 AND status >= ").T(_Doc.STATUS_SUBMITTED).T(" ORDER BY id");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(orgid));

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


        [Ui("✚", "添加"), Tool(ButtonOpen)]
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
                        var prods = ObtainMap<short, Prod>();
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
                var o = await wc.ReadObjectAsync<Buy>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO buys ").colset(Buy.Empty, 0)._VALUES_(Buy.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }

    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("销售分拣", "sign-out")]
    public class CtrlyBuyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyBuyVarWork>();
        }

        [Ui("已确认", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }

        [Ui("已发货", group: 2), Tool(Anchor)]
        public async Task shipped(WebContext wc, int page)
        {
            short orgid = wc[-1];
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }

        [Ui("发货", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int typ)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-1];
        }

        [Ui("复制", group: 2), Tool(ButtonPickOpen)]
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
}