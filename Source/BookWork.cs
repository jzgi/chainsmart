using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainMart
{
    public abstract class BookWork<V> : WebWork where V : BookVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    [UserAuthorize(Org.TYP_SHP, 1)]
    [Ui("供应链采购", "商户")]
    public class ShplyBookWork : BookWork<ShplyBookVarWork>
    {
        [Ui("供应链采购", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE shpid = @1 AND state = 0 ORDER BY id");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无订货");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
                    h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();
                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name);
                    h.P(o.tip);
                    h._DIV();
                    h._A();
                });
            });
        }

        [Ui(icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE shpid = @1 AND state >= 1 ORDER BY id");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
                    h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();
                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name);
                    h.P(o.tip);
                    h._DIV();
                    h._A();
                });
            });
        }

        [UserAuthorize(Org.TYP_SHP, User.ORGLY_OPN)]
        [Ui("新建", "选择产品批次", "plus", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int typ)
        {
            var mrt = wc[-1].As<Org>();
            int ctrid = mrt.ctrid;
            var topOrgs = Grab<int, Org>();
            var ctr = topOrgs[ctrid];
            var cats = Grab<short, Cat>();

            if (typ == 0)
            {
                wc.Subscript = typ = cats.KeyAt(0);
            }

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE ctrid = @1 AND status > 0 AND typ = @2");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(ctrid).Set(typ));

            wc.GivePage(200, h =>
            {
                h.TOPBAR_().NAVBAR(nameof(@new), typ, cats)._TOPBAR();

                if (arr == null)
                {
                    h.ALERT("没有批次");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_SHOW, false, tip: o.name, css: "uk-card-body uk-flex");
                    h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.id).T("/icon")._PIC();
                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name);
                    h.P(o.tip);
                    h._DIV();
                    h._A();
                });
            }, title: ctr.tip);
        }
    }

    [UserAuthorize(Org.TYP_SRC, User.ORGLY_LOG)]
    [Ui("销售订货", "产源")]
    public class SrclyBookWork : BookWork<SrclyBookVarWork>
    {
        [Ui("当前订货"), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();
            var topOrgs = Grab<int, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT itemid, last(name), ctrid, sum(qty - cut) FROM books WHERE srcid = @1 AND status = ").T(Book.STA_PAID).T(" GROUP BY itemid, ctrid");
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

        [Ui(tip: "以往订货", icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE srcid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE_();
                h._TABLE();
            });
        }
    }


    [UserAuthorize(Org.TYP_MKT, 1)]
#if ZHNT
    [Ui("供应链采购收货", "市场")]
#else
    [Ui("供应链采购收货", "驿站")]
#endif
    public class MktlyBookWork : BookWork<MktlyBookVarWork>
    {
        [Ui("按产品", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var mkt = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT lotid, first(name), count(qty), first(unit) FROM books WHERE mktid = @1 AND state > 0 GROUP BY lotid LIMIT 30 OFFSET 30 * @2");
            await dc.QueryAsync(p => p.Set(mkt.id).Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.TABLE_();
                int n = 0;
                while (dc.Next())
                {
                    dc.Let(out int lotid);
                    dc.Let(out string name);
                    dc.Let(out decimal qty);
                    dc.Let(out string unit);
                    h.TR_();
                    h.TD(name);
                    h.TD_("uk-visible@l").T(qty).SP().T(unit)._TD();
                    h._TR();
                    n++;
                }
                h._TABLE();

                h.PAGINATION(n == 30);
            });
        }

        [Ui(tip: "历史", icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT shpid, first(shpname) AS shpname, count(id) AS count FROM books WHERE mktid = @1 AND state > 0 GROUP BY shpid");
            var arr = await dc.QueryAsync<BookAgg>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.TABLE(arr, o =>
                {
                    h.TD_();
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");
                    h._A();
                    h._TD();
                });
            });
        }

        [Ui("按商户", group: 4), Tool(Anchor)]
        public async Task byshp(WebContext wc)
        {
            var mkt = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT shpid, first(shpname), count(qty) AS qty FROM books WHERE mktid = @1 AND state > 0 GROUP BY shpid, lotid");
            var arr = await dc.QueryAsync<BookAgg>(p => p.Set(mkt.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.TABLE_();
                int n = 0;
                while (dc.Next())
                {
                    dc.Let(out int lotid);
                    dc.Let(out string name);
                    dc.Let(out decimal qty);
                    dc.Let(out string unit);
                    h.TR_();
                    h.TD(name);
                    h.TD_("uk-visible@l").T(qty).SP().T(unit)._TD();
                    h._TR();
                    n++;
                }
                h._TABLE();

                h.PAGINATION(n == 30);
            });
        }
    }

    [UserAuthorize(Org.TYP_DST, User.ORGLY_)]
    [Ui("订货分运", "控运", icon: "sign-out")]
    public class CtrlyBookWork : BookWork<CtrlyBookVarWork>
    {
        [Ui("按批次", group: 2), Tool(Anchor)]
        public async Task @default(WebContext wc)
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

        [Ui("⌹", "按批次历史", group: 4), Tool(Anchor)]
        public async Task bylotpast(WebContext wc)
        {
        }

        [Ui("按市场", group: 8), Tool(Anchor)]
        public async Task bymrt(WebContext wc)
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

        [Ui("以往按市场", group: 16), Tool(Anchor)]
        public async Task bymrtpast(WebContext wc)
        {
        }

        [Ui("发出", group: 255), Tool(ButtonOpen)]
        public async Task rev(WebContext wc)
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

        [Ui("取消发出", group: 2), Tool(ButtonOpen)]
        public async Task unrcv(WebContext wc)
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