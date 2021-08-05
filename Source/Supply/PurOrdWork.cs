using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Db;
using SkyChain.Web;
using static SkyChain.Web.Modal;
using static Zhnt._Doc;
using static Zhnt.User;

namespace Zhnt.Supply
{
    public class PubLotWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PubLotVarWork>();
        }

        [Ui, Tool(Anchor)]
        public virtual async Task @default(WebContext wc, int sub)
        {
            var orgs = Fetch<Map<short, Org>>();
            var regs = Fetch<Map<short, Reg>>();
            short regid = wc.Query[nameof(regid)];
            if (regid == 0)
            {
                regid = regs.KeyAt(0);
            }
            var reg = regs[regid];
            var orgida = orgs.GetRelatedOrgs(reg);
            var typ = (short) (sub + 1);
            // retrieve lots and lines
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purchase.Empty).T(" FROM lots_vw WHERE orgid = ANY(@1::smallint[]) AND typ = @2 AND status = ").T(STATUS_DRAFT).T(" ORDER BY id DESC");
            var arr = await dc.QueryAsync<Purchase>(p => p.Set(orgida).Set(typ));

            wc.GivePage(200, h =>
                {
                    h.TOPBAR_LOT(reg, sub, regs);
                    if (arr != null)
                    {
                        h.ViewLotList(arr, orgs, DateTime.Today);
                    }
                    else
                    {
                        h.DIV_("uk-section uk-padding").T("今天没有可参加的" + Purchase.Typs[typ]);
                    }
                },
                true, 12
            );
        }

        public void result(WebContext wc, int err)
        {
            wc.GivePane(200, h =>
            {
                h.DIV_("uk-width-1-1 uk-col uk-flex-middle uk-padding-large");
                if (err > 0)
                {
                    h.PIC("/no.png", css: "uk-width-medium");
                    h.SPAN_("uk-padding").T("操作失败，请稍后重试")._SPAN();
                }
                else
                {
                    h.PIC("/yes.png", css: "uk-width-medium");
                    h.SPAN_("uk-padding").T("操作成功，结果请参看「").A_GOTO("我的拼团", href: "/my//lot/").T("」")._SPAN();
                }
                h._DIV();
            }, true, 3600);
        }
    }

    public class MyLotWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MyLotVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            var orgs = Fetch<Map<short, Org>>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Purchase.Empty, alias: "m").T(" FROM lots_vw m, lotjns d WHERE m.id = d.lotid AND d.uid = @1 AND d.status > 0 ORDER BY d.lotid DESC LIMIT 10 OFFSET 10 * @2");
            var arr = await dc.QueryAsync<Purchase>(p => p.Set(prin.id).Set(page));

            wc.GivePage(200, h =>
            {
                if (arr == null)
                {
                    h.ALERT_().SPAN_().T("暂无拼团记录；请参看「").A_GOTO("健康拼团", href: "/lot/").T("」")._SPAN()._ALERT();
                    return;
                }
                h.ViewLotList(arr, orgs, DateTime.Today);
                h.PAGINATION(arr.Length > 10);
            }, false, 12, "我的拼团");
        }
    }


    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("拼团")]
    public class OrglyLotWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<OrglyLotVarWork>();
        }

        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            var orgs = Fetch<Map<short, Org>>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purchase.Empty).T(" FROM lots_vw WHERE orgid = @1 AND status < ").T(STATUS_ISSUED).T(" ORDER BY id DESC LIMIT 10 OFFSET @2 * 10");
            var arr = await dc.QueryAsync<Purchase>(p => p.Set(orgid).Set(page), 0xff);

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: "当前活跃拼团");
                if (arr != null)
                {
                    h.ViewLotList(arr, orgs, DateTime.Today);
                }
                h.PAGINATION(arr?.Length == 10);
            }, false, 3);
        }

        [Ui("停止", group: 2), Tool(Anchor)]
        public async Task closed(WebContext wc, int page)
        {
            short orgid = wc[-1];
            var orgs = Fetch<Map<short, Org>>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Purchase.Empty).T(" FROM lots_vw WHERE orgid = @1 AND status >= ").T(STATUS_ISSUED).T(" ORDER BY status, id DESC LIMIT 10 OFFSET @2 * 10");
            var arr = await dc.QueryAsync<Purchase>(p => p.Set(orgid).Set(page), 0xff);

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: "已成和已结拼团");
                if (arr != null)
                {
                    h.ViewLotList(arr, orgs, DateTime.Today);
                }
                h.PAGINATION(arr?.Length == 10);
            }, false, 3);
        }

        [Ui("发布", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int typ)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-1];
            var orgs = Fetch<Map<short, Org>>();
            var org = orgs[orgid];
            if (wc.IsGet)
            {
                if (typ == 0) // display type selection
                {
                    wc.GivePane(200, h =>
                    {
                        h.FORM_().FIELDSUL_("请选择推广类型");
                        for (int i = 0; i < LotDescr.All.Count; i++)
                        {
                            var descr = LotDescr.All.ValueAt(i);
                            if (!descr.CanDo(org)) continue;
                            h.LI_("uk-flex");
                            h.A_HREF_(nameof(@new) + "-" + descr.Key, end: true, css: "uk-button uk-button-secondary uk-width-1-4").T(descr.name)._A();
                            h.P(descr.tip, "uk-padding uk-width-expand");
                            h._LI();
                        }
                        h._FIELDSUL();
                        h._FORM();
                    });
                }
                else // typ specified
                {
                    var descr = LotDescr.All[(short) typ];
                    var o = new Purchase
                    {
                        typ = (short) typ,
                        least = 1,
                        span = 2,
                        start = DateTime.Now
                    };
                    wc.GivePane(200, h =>
                    {
                        h.FORM_();
                        h.FIELDSUL_(descr.name);
                        h.HIDDEN(nameof(o.typ), o.typ);
                        descr.FormView(h, descr, o, org);
                        h.BOTTOMBAR_().BUTTON("发布", nameof(@new))._BOTTOMBAR();
                        h._FORM();
                    });
                }
            }
            else // POST
            {
                var today = DateTime.Today;
                var o = await wc.ReadObjectAsync(inst: new Purchase
                {
                    status = STATUS_DRAFT,
                    orgid = orgid,
                    issued = today,
                    author = prin.name,
                    @extern = org.@extern,
                });
                // database op
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO lots ").colset(o, 0)._VALUES_(o, 0);
                await dc.QueryTopAsync(p => o.Write(p, 0));

                wc.GivePane(201);
            }
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

        public async Task tbank(WebContext wc, int page)
        {
            short orgid = wc[-1];
            var orgs = Fetch<Map<short, Org>>();
            var org = orgs[orgid];
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            var todo = await dc.SeekQueueAsync(org.Acct);
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: "核实公益价值（区块链）");

                h.LI_().LABEL("代办事项")._LI();
                foreach (var o in todo)
                {
                    h.LI_("uk-flex");
                    h.SPAN_("uk-width-1-2").T(o.Name).T(" ➜")._SPAN();
                    h.SPAN(o.Remark, "uk-width-expand");
                    // h.SPAN_("uk-width-micro uk-text-right").VARTOOLS(o.Job)._SPAN();
                    h._LI();
                }
            });
        }
    }


    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("运送")]
    public class OrglyDistWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<OrglyLotDistVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT m.id, m.name, m.price, m.unit, d.uid, d.uname, d.utel, d.uim, d.qty, d.pay FROM lots m, lotjns d WHERE m.id = d.lotid AND m.status = ").T(STATUS_ISSUED).T(" AND d.ptid = @1 ORDER BY m.id");
            await dc.QueryAsync(p => p.Set(orgid));

            var orgs = Fetch<Map<short, Org>>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: "拼团递货管理");

                int last = 0;
                while (dc.Next())
                {
                    dc.Let(out int id);
                    dc.Let(out string name);
                    dc.Let(out decimal price);
                    dc.Let(out string unit);
                    dc.Let(out int uid);
                    dc.Let(out string uname);
                    dc.Let(out string utel);
                    dc.Let(out string uim);
                    dc.Let(out short qty);
                    dc.Let(out decimal pay);
                    if (id != last)
                    {
                        h._LI();
                        if (last != 0)
                        {
                            h._UL();
                            h._ARTICLE();
                        }
                        h.ARTICLE_("uk-card uk-card-default");
                        h.HEADER_("uk-card-header").T(name).SPAN_("uk-badge").CNY(price).T('／').T(unit)._SPAN()._HEADER();
                        h.UL_("uk-card-body");
                    }
                    h.LI_("uk-flex uk-width-1-1");
                    h.P(uname, "uk-width-1-3");
                    h.P(qty, "uk-text-right uk-width-1-6");
                    h.P(pay, "uk-text-right uk-width-1-6");
                    h._LI();

                    last = id;
                }
                h._UL();
                h._ARTICLE();
            });
        }
    }
}