using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class BookWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_SHP, 1)]
    [Ui("线上采购", "商户")]
    public class ShplyBookWork : BookWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<ShplyBookVarWork>();
        }

        [Ui("当前进货", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE shpid = @1 AND state = 0 ORDER BY id");
            var map = await dc.QueryAsync<int, Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (map == null) return;
                h.GRIDA(map, o =>
                {
                    h.PIC_().T(ChainMartApp.WwwUrl).T("/item/").T(o.id).T("/icon")._PIC();
                    h.SECTION_("uk-width-4-5");
                    h.T(o.name);
                    h._SECTION();
                });
            });
        }

        [Ui(icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE shpid = @1 AND state >= 1 ORDER BY id");
            var map = await dc.QueryAsync<int, Book>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (map == null) return;
                h.GRIDA(map, o =>
                {
                    h.PIC_().T(ChainMartApp.WwwUrl).T("/item/").T(o.id).T("/icon")._PIC();
                    h.SECTION_("uk-width-4-5");
                    h.T(o.name);
                    h._SECTION();
                });
            });
        }

        [Ui("新建", "选择批次下单进货", "plus", group: 1), Tool(ButtonOpen)]
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
            var map = await dc.QueryAsync<int, Lot>(p => p.Set(ctrid).Set(typ));

            wc.GivePage(200, h =>
            {
                h.TOPBAR_().NAVBAR(cats, nameof(@new), typ)._TOPBAR();

                if (map == null)
                {
                    h.ALERT("没有批次");
                    return;
                }

                h.GRIDA(map, o =>
                {
                    h.DIV_("uk-card-body");
                    h.PIC_().T(ChainMartApp.WwwUrl).T("/item/").T(o.id).T("/icon")._PIC();
                    h.T(o.name);
                    h._DIV();
                }, min: 2);
            }, title: ctr.tip);
        }
    }

    [UserAuthorize(Org.TYP_SRC, User.ORGLY_LOG)]
    [Ui("处理客户订货", "产源")]
    public class SrclyBookWork : BookWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<SrclyBookVarWork>();
        }

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

        [Ui("历史", icon: "history", group: 2), Tool(Anchor)]
        public async Task history(WebContext wc)
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


    [UserAuthorize(Org.TYP_MRT, 1)]
    [Ui("线上采购收货", "市场")]
    public class MktlyBookWork : BookWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [UserAuthorize(Org.TYP_DST, User.ORGLY_)]
    [Ui("订货分拣管理", "中控", icon: "sign-out")]
    public class CtrlyBookWork : BookWork
    {
        [Ui("按批次", @group: 2), Tool(Anchor)]
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

        [Ui("按市场", @group: 8), Tool(Anchor)]
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

        [Ui("发出", @group: 255), Tool(ButtonOpen)]
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

        [Ui("取消发出", @group: 2), Tool(ButtonOpen)]
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

    [UserAuthorize(Org.TYP_DST, User.ORGLY_)]
    [Ui("订货派运管理", "中控", icon: "sign-out")]
    public class CtrlyDistrWork : BookWork
    {
        [Ui("按批次", @group: 2), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }
}