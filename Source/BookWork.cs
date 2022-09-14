using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class BookWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_MRT, 1)]
    [Ui("市场进货动态", "市场")]
    public class MrtlyBookWork : BookWork
    {
        [Ui("当前", @group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [UserAuthorize(Org.TYP_SHP, 1)]
#if ZHNT
    [Ui("进货管理", "商户")]
#else
    [Ui("进货管理", "商户")]
#endif
    public class ShplyBookWork : BookWork
    {
        [Ui("当前", "当前进货", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE shpid = @1 AND status = 0 ORDER BY id");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.TABLE(arr, o =>
                {
                    // h.TD(items[o.itemid].name);
                    h.TD(o.ctrid);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("历史", "历史进货", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE shpid = @1 AND status >= 1 ORDER BY id");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    // h.TD(items[o.itemid].name);
                    h.TD(o.ctrid);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("新增", "新增进货", "plus", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var mrt = wc[-1].As<Org>();
            int ctrid = mrt.ctrid;
            var topOrgs = Grab<int, Org>();
            var ctr = topOrgs[ctrid];
            var cats = Grab<short, Cat>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE ctrid = @1 AND status > 0");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(ctrid));

            wc.GivePage(200, h =>
            {
                if (arr == null)
                {
                    h.ALERT("没有在批发的产品");
                    return;
                }

                h.SUBNAV(cats, "", 0);

                h.TABLE_();
                var last = 0;
                for (var i = 0; i < arr?.Length; i++)
                {
                    var o = arr[i];
                    // if (o.prvid != last)
                    // {
                    //     var spr = topOrgs[o.prvid];
                    //     h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
                    // }
                    h.TR_();
                    h.TD(o.name);
                    // h.TD(o.price, true);
                    h._TR();

                    // last = o.prvid;
                }
                h._TABLE();
            }, title: ctr.tip);
        }
    }

    [UserAuthorize(Org.TYP_SRC, User.ORGLY_LOG)]
    [Ui("客户订货管理", "产源", icon: "sign-in")]
    public class SrclyBookWork : BookWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyBookVarWork>();
        }

        [Ui("当前订货"), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var src = wc[-1].As<Org>();
            var topOrgs = Grab<int, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT productid, last(name), ctrid, sum(qty - cut) FROM books WHERE srcid = @1 AND status = ").T(Book.STA_PAID).T(" GROUP BY productid, ctrid");
            await dc.QueryAsync(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.MAIN_();
                int last = 0;
                while (dc.Next())
                {
                    dc.Let(out int wareid);
                    dc.Let(out string name);
                    dc.Let(out int ctrid);
                    dc.Let(out decimal qty);

                    if (ctrid != last)
                    {
                        var spr = topOrgs[ctrid];
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    h.TD(name);
                    h.TD_("uk-visible@l").T(qty)._TD();
                    h._TR();

                    last = ctrid;
                }
                h._MAIN();
            });
        }

        [Ui("⌹", "历史订货"), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE srcid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: Label);
                h.TABLE_();
                h._TABLE();
            });
        }
    }


    [UserAuthorize(Org.TYP_SRC, User.ORGLY_LOG)]
    [Ui("业务报表", "产源")]
    public class SrclyRptWork : BookWork
    {
        public async Task @default(WebContext wc, int page)
        {
        }
    }

    [UserAuthorize(Org.TYP_DST, User.ORGLY_)]
    [Ui("订货分拣管理", "中控", icon: "sign-out")]
    public class CtrlyBookWork : BookWork
    {
        [Ui("按批次", @group: 2), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var ctr = wc[-1].As<Org>();
            var topOrgs = Grab<int, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT mrtid, wareid, last(name), sum(qty - qtyre) AS qty FROM purchs WHERE ctrid = @1 AND status = ").T(Book.STA_DELIVERED).T(" GROUP BY mrtid, wareid ORDER BY mrtid");
            await dc.QueryAsync(p => p.Set(ctr.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.MAIN_();
                int last = 0;
                while (dc.Next())
                {
                    dc.Let(out int mrtid);
                    dc.Let(out int wareid);
                    dc.Let(out string name);
                    dc.Let(out decimal qty
                    );
                    if (mrtid != last)
                    {
                        var spr = topOrgs[mrtid];
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    h.TD(name);
                    h.TD_("uk-visible@l").T(qty)._TD();
                    h._TR();

                    last = mrtid;
                }
                h._MAIN();
            });
        }

        [Ui("⌹", "按批次历史", @group: 4), Tool(Anchor)]
        public async Task bylotpast(WebContext wc, int page)
        {
        }

        [Ui("按市场", @group: 8), Tool(Anchor)]
        public async Task bymrt(WebContext wc, int page)
        {
            var ctr = wc[-1].As<Org>();
            var topOrgs = Grab<int, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT mrtid, wareid, last(name), sum(qty - qtyre) AS qty FROM books WHERE ctrid = @1 AND status = ").T(Book.STA_RECEIVED).T(" GROUP BY mrtid, wareid ORDER BY mrtid");
            await dc.QueryAsync(p => p.Set(ctr.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.MAIN_();
                int last = 0;
                while (dc.Next())
                {
                    dc.Let(out int mrtid);
                    dc.Let(out int wareid);
                    dc.Let(out string name);
                    dc.Let(out decimal qty
                    );
                    if (mrtid != last)
                    {
                        var spr = topOrgs[mrtid];
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    h.TD(name);
                    h.TD_("uk-visible@l").T(qty)._TD();
                    h._TR();

                    last = mrtid;
                }
                h._MAIN();
            });
        }

        [Ui("⌹", "以往按市场", @group: 16), Tool(Anchor)]
        public async Task bymrtpast(WebContext wc, int page)
        {
        }

        [Ui("▷", "发出", @group: 255), Tool(ButtonShow)]
        public async Task rev(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-1];
            short typ = 0;
            decimal amt = 0;
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("指定统计区间");
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                wc.GivePane(200); // close dialog
            }
        }

        [Ui("◁", "取消发出", @group: 2), Tool(ButtonShow)]
        public async Task unrcv(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-1];
            short typ = 0;
            decimal amt = 0;
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("指定统计区间");
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                wc.GivePane(200); // close dialog
            }
        }
    }

    [UserAuthorize(Org.TYP_DST, User.ORGLY_)]
    [Ui("订货派运管理", "中控", icon: "sign-out")]
    public class CtrlyDistribWork : BookWork
    {
        [Ui("按批次", @group: 2), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }
}