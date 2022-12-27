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

    [OrglyAuthorize(Org.TYP_SHP, 1)]
    [Ui("供应链采购", "商户")]
    public class ShplyBookWork : BookWork<ShplyBookVarWork>
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            // add sub work for creation of booking
            CreateWork<ShplyBookLotWork>("lot");
        }

        [Ui("供应链采购", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE shpid = @1 AND status BETWEEN 1 AND 2 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无采购");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN(Book.Statuses[o.status], "uk-badge")._HEADER();
                    h.P(o.tip, "uk-width-expand");
                    h.FOOTER_().T(o.qty).SP().T("件").SP().T(o.unitx * o.qty).SP().T(o.unit).SPAN_("uk-margin-auto-left").CNY(o.Total)._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            });
        }

        [Ui(tip: "历史记录", icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE shpid = @1 AND status = 4 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无采购");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN(Book.Statuses[o.status], "uk-badge")._HEADER();
                    h.P(o.tip, "uk-width-expand");
                    h.FOOTER_().T(o.qty).SP().T("件").SP().T(o.unitx * o.qty).SP().T(o.unit).SPAN_("uk-margin-auto-left").CNY(o.Total)._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            });
        }

        [OrglyAuthorize(Org.TYP_SHP, User.ROL_OPN)]
        [Ui("下单", "供应链采购下单", "plus", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int typ)
        {
            var org = wc[-1].As<Org>();
            int ctrid = org.ctrid;
            var topOrgs = Grab<int, Org>();
            var cats = Grab<short, Cat>();

            if (typ == 0)
            {
                wc.Subscript = typ = cats.KeyAt(0);
            }

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE status = 4 AND typ = @2 AND (targs IS NULL OR targs @> ARRAY[@3] OR targs @> ARRAY[@4])");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(ctrid).Set(typ).Set(org.ctrid).Set(org.MarketId));


            wc.GivePage(200, h =>
            {
                h.TOPBAR_().NAVBAR(nameof(@new), typ, cats)._TOPBAR();

                if (arr == null)
                {
                    h.ALERT("尚无产品");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    // anchor to the lot sub work
                    h.ADIALOG_("lot/", o.Key, "/", MOD_SHOW, false, tip: o.name, css: "uk-card-body uk-flex");

                    h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN("")._HEADER();
                    h.P(o.tip, "uk-width-expand");
                    h.FOOTER_().T("每件").SP().T(o.unitx).SP().T(o.unit).SPAN_("uk-margin-auto-left").CNY(o.price)._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            });
        }
    }

    [OrglyAuthorize(Org.TYP_SRC, 1)]
    [Ui("供应链销售", "产源")]
    public class SrclyBookWork : BookWork<SrclyBookVarWork>
    {
        [Ui("供应链销售"), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE srcid = @1 AND status BETWEEN 1 AND 2 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无销售");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN(Book.Statuses[o.status], "uk-badge")._HEADER();
                    h.P(o.tip, "uk-width-expand");
                    h.FOOTER_().T(o.qty).SP().T("件").SP().T(o.unitx * o.qty).SP().T(o.unit).SPAN_("uk-margin-auto-left").CNY(o.Total)._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            });
        }

        [Ui(tip: "以往订货", icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE srcid = @1 AND status = 4 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无销售");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN(Book.Statuses[o.status], "uk-badge")._HEADER();
                    h.P(o.tip, "uk-width-expand");
                    h.FOOTER_().T(o.qty).SP().T("件").SP().T(o.unitx * o.qty).SP().T(o.unit).SPAN_("uk-margin-auto-left").CNY(o.Total)._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            });
        }
    }


    [OrglyAuthorize(Org.TYP_MKT, 1)]
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

    [OrglyAuthorize(Org.TYP_CTR, 1)]
    [Ui("供应链销售统一发货", "中库")]
    public class CtrlyBookWork : BookWork<CtrlyBookVarWork>
    {
        [Ui("按批次", group: 2), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var ctr = wc[-1].As<Org>();
            var topOrgs = Grab<int, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT mktid, lotid, last(name), sum(qty) AS qty FROM books WHERE ctrid = @1 AND status = 2 GROUP BY mktid, lotid ORDER BY mktid");
            await dc.QueryAsync(p => p.Set(ctr.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.MAIN_();
                int last = 0;
                while (dc.Next())
                {
                    dc.Let(out int mktid);
                    dc.Let(out int wareid);
                    dc.Let(out string name);
                    dc.Let(out decimal qty
                    );
                    if (mktid != last)
                    {
                        var spr = topOrgs[mktid];
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    h.TD(name);
                    h.TD_("uk-visible@l").T(qty)._TD();
                    h._TR();

                    last = mktid;
                }
                h._MAIN();
            });
        }

        [Ui(icon: "history", group: 4), Tool(Anchor)]
        public async Task bylotpast(WebContext wc)
        {
        }

        [Ui("按市场", group: 8), Tool(Anchor)]
        public async Task bymrt(WebContext wc)
        {
            var ctr = wc[-1].As<Org>();
            var topOrgs = Grab<int, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT mktid, wareid, last(name), sum(qty - qtyre) AS qty FROM books WHERE ctrid = @1 AND status = 4 GROUP BY mktid, wareid ORDER BY mktid");
            await dc.QueryAsync(p => p.Set(ctr.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.MAIN_();
                int last = 0;
                while (dc.Next())
                {
                    dc.Let(out int mktid);
                    dc.Let(out int wareid);
                    dc.Let(out string name);
                    dc.Let(out decimal qty
                    );
                    if (mktid != last)
                    {
                        var spr = topOrgs[mktid];
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                    }
                    h.TR_();
                    h.TD(name);
                    h.TD_("uk-visible@l").T(qty)._TD();
                    h._TR();

                    last = mktid;
                }
                h._MAIN();
            });
        }

        [Ui(icon: "history", group: 16), Tool(Anchor)]
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