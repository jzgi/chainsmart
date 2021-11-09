using System;
using System.Threading.Tasks;
using SkyChain;
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

        [Ui("购物车", group: 1), Tool(Anchor)]
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

        [Ui("当前", group: 2), Tool(Anchor)]
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

        [Ui("历史", group: 4), Tool(Anchor)]
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


        [Ui("✚", "添加", group: 1), Tool(ButtonOpen)]
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

    [UserAuthorize(orgly: User.ORGLY_OP)]
    public class CtrlyDistribWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyDistribVarWork>();
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

    [Ui("销售分拣管理", "sign-out", fork: Org.FRK_AGRI)]
    public class CtrlyAgriDistribWork : CtrlyDistribWork
    {
    }

    [Ui("销售分拣管理", "sign-out")]
    public class CtrlyDietaryDistribWork : CtrlyDistribWork
    {
    }

    public class CtrlyHomeDistribWork : CtrlyDistribWork
    {
    }

    public class CtrlyCareDistribWork : CtrlyDistribWork
    {
    }

    public class CtrlyAdDistribWork : CtrlyDistribWork
    {
    }

    public class CtrlyCharityDistribWork : CtrlyDistribWork
    {
    }
}