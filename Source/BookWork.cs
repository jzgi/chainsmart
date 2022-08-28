using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public class BookWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_MRT, 1)]
    [Ui("市场进货动态")]
    public class MrtlyBookWork : BookWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [UserAuthorize(Org.TYP_SHP, 1)]
#if ZHNT
    [Ui("商户进货业务", icon: "pull")]
#else
    [Ui("驿站进货业务", icon: "pull")]
#endif
    public class ShplyBookWork : BookWork
    {
        [Ui("当前进货", group: 1), Tool(Anchor)]
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

        [Ui("⌹", "历史进货", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM purchs WHERE bizid = @1 AND status >= 1 ORDER BY id");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            var items = Grab<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: "历史订货");
                h.TABLE(arr, o =>
                {
                    // h.TD(items[o.itemid].name);
                    h.TD(o.ctrid);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("✛", "新增进货", group: 1), Tool(ButtonOpen)]
        public void @new(WebContext wc)
        {
            var mrt = wc[-1].As<Org>();
            wc.GiveRedirect("/" + mrt.ctrid + "/");
        }
    }

    [UserAuthorize(Org.TYP_SRC, User.ORGLY_SAL)]
    [Ui("产源销货业务", icon: "sign-in")]
    public class SrclyBookWork : BookWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyBookVarWork>();
        }

        [Ui("当前销货"), Tool(Anchor)]
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

        [Ui("⌹", "历史销货"), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE srcid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: "历史销售订货");
                h.TABLE_();
                h._TABLE();
            });
        }
    }


    [UserAuthorize(Org.TYP_SRC, User.ORGLY_SAL)]
    [Ui("产源业务报表")]
    public class SrclyRptWork : BookWork
    {
        public async Task @default(WebContext wc, int page)
        {
        }
    }

    [UserAuthorize(Org.TYP_DST, User.ORGLY_)]
    [Ui("中控分发管理", icon: "sign-out")]
    public class CtrlyBookWork : BookWork
    {
        [Ui("概况", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }

        [Ui("按批次", group: 2), Tool(Anchor)]
        public async Task bylot(WebContext wc, int page)
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

        [Ui("", "按批次", group: 4), Tool(Anchor)]
        public async Task bylotpast(WebContext wc, int page)
        {
        }

        [Ui("按市场", group: 8), Tool(Anchor)]
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

        [Ui("▷", "发出", group: 1), Tool(ButtonShow)]
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

        [Ui("◁", "取消发出", group: 2), Tool(ButtonShow)]
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
}