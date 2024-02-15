using System;
using System.Data;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Nodality;

namespace ChainSmart;

public class LotVarWork : WebWork
{
    static readonly string
        SrcUrl = MainApp.WwwUrl + "/org/",
        LotUrl = MainApp.WwwUrl + "/lot/";

    internal static void ShowLot(HtmlBuilder h, Lot o, Org src, bool pricing, bool linking, int tracenum = 0)
    {
        h.ARTICLE_("uk-card uk-card-primary");
        h.H2("产品信息", "uk-card-header");
        h.SECTION_("uk-card-body");
        if (o.pic)
        {
            h.PIC(LotUrl, o.id, "/pic", css: "uk-width-1-1");
        }
        h.UL_("uk-list uk-list-divider");
        h.LI_().FIELD("产品名", o.name)._LI();

        if (pricing)
        {
            h.LI_().FIELD("零售单位", o.unit).FIELD("单位含重", o.unitw, Unit.Weights[o.unitw])._LI();
            h.LI_().FIELD2("整件", o.unitx, o.unit)._LI();
            h.LI_().FIELD("单价", o.price, true).FIELD("优惠立减", o.off, true)._LI();
            h.LI_().FIELD("起订件数", o.min).FIELD("限订件数", o.max)._LI();
        }

        h._UL();

        if (src != null)
        {
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("产源设施", src.name)._LI();
            h.LI_().FIELD(string.Empty, src.tip)._LI();
            // h.LI_().FIELD("等级", src.rank, Src.Ranks)._LI();
            h._UL();

            if (src.tip != null)
            {
                h.ALERT_().T(src.tip)._ALERT();
            }
            if (src.pic)
            {
                h.PIC(SrcUrl, src.id, "/pic", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (src.m1)
            {
                h.PIC(SrcUrl, src.id, "/m-1", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (src.m2)
            {
                h.PIC(SrcUrl, src.id, "/m-2", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (src.m3)
            {
                h.PIC(SrcUrl, src.id, "/m-3", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (src.scene)
            {
                h.PIC(SrcUrl, src.id, "/m-scene", css: "uk-width-1-1 uk-padding-bottom");
            }
        }
        h._SECTION();
        h._ARTICLE();

        h.ARTICLE_("uk-card uk-card-primary");
        h.H2("批次检验", "uk-card-header");
        h.SECTION_("uk-card-body");

        h.UL_("uk-list uk-list-divider");
        h.LI_().FIELD("批次编号", o.id, digits: 8)._LI();
        if (o.nstart > 0 && o.nend > 0)
        {
            h.LI_().FIELD2("批次溯源码", $"{o.nstart:0000 0000}", $"{o.nend:0000 0000}", "－")._LI();
        }

        var offset = tracenum - o.nstart;
        if (offset > 0)
        {
            h.LI_().LABEL("本溯源码").SPAN($"{tracenum:0000 0000}", css: "uk-static uk-text-danger")._LI();
            if (o.TryGetStockOp(offset, out var v))
            {
                h.LI_().LABEL("生效日期").SPAN(v.dt, css: "uk-static uk-text-danger")._LI();
            }
        }
        h._LI();
        h._UL();

        if (o.m1)
        {
            h.PIC(LotUrl, o.id, "/m-1", css: "uk-width-1-1 uk-padding-bottom");
        }
        if (o.m2)
        {
            h.PIC(LotUrl, o.id, "/m-2", css: "uk-width-1-1 uk-padding-bottom");
        }
        if (o.m3)
        {
            h.PIC(LotUrl, o.id, "/m-3", css: "uk-width-1-1 uk-padding-bottom");
        }
        if (o.m4)
        {
            h.PIC(LotUrl, o.id, "/m-4", css: "uk-width-1-1 uk-padding-bottom");
        }

        if (linking)
        {
            h.UL_("uk-list uk-list-divider");
            if (!string.IsNullOrEmpty(o.linka))
            {
                h.LI_("uk-flex-center").A_(o.linka, css: "uk-button uk-icon-link").ICON("file-text").SP().T("合格证")._A()._LI();
            }
            if (!string.IsNullOrEmpty(o.linkb))
            {
                h.LI_("uk-flex-center").A_(o.linkb, css: "uk-button uk-icon-link").ICON("file-pdf").SP().T("质检报告")._A()._LI();
            }
            h._UL();
        }

        h._SECTION();
        h._ARTICLE();
    }

    public virtual async Task @default(WebContext wc, int v)
    {
        int id = wc[0];

        const short Msk = 0xff | MSK_EXTRA;

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty, Msk).T(" FROM lots_vw WHERE id = @1");
        var o = await dc.QueryTopAsync<Lot>(p => p.Set(id), Msk);

        if (o.IsOnHub)
        {
            await dc.QueryAsync("SELECT hubid, num FROM lotinvs WHERE lotid = @1", p => p.Set(id));
        }

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("批次编号", o.id, digits: 8)._LI();
            h.LI_().FIELD("产品名", o.name).FIELD("供应点类型", Lot.Typs[o.typ])._LI();
            h.LI_().FIELD("简介语", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
            if (o.typ == 2) h.FIELD("交货起始日", o.shipon);
            h._LI();

            h.LI_().FIELD("零售单位", o.unit).FIELD("单位含重", Unit.Weights[o.unitw])._LI();
            h.LI_().FIELD("总件数", o.cap)._LI();
            h.LI_().FIELD("单价", o.price, true).FIELD("优惠立减", o.off, true)._LI();
            h.LI_().FIELD("起订件数", o.min).FIELD("限订件数", o.max)._LI();
            h.LI_().FIELD2("溯源编号", o.nstart, o.nend, "－")._LI();

            h.LI_().LABEL("货架");
            if (o.IsOnHub)
            {
                h.UL_("uk-static");
                while (dc.Next())
                {
                    dc.Let(out int hubid);
                    dc.Let(out int stock);

                    var hub = GrabTwin<int, Org>(hubid);

                    h.LI_().T(hub.name).T('（').T(stock / o.unitx).T('）')._LI();
                }
                h._UL();
            }
            else
            {
                h.SPAN(o.stock, css: "uk-static");
            }
            h._LI();

            h.LI_().FIELD("状态", o.status, Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "上线", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        });
    }

    protected async Task doimg(WebContext wc, string col, bool shared, int maxage)
    {
        int id = wc[0];
        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").T(col).T(" FROM lots WHERE id = @1");
            if (await dc.QueryTopAsync(p => p.Set(id)))
            {
                dc.Let(out byte[] bytes);
                if (bytes == null) wc.Give(204); // no content 
                else wc.Give(200, new WebStaticContent(bytes), shared, maxage);
            }
            else
            {
                wc.Give(404, null, shared, maxage); // not found
            }
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            ArraySegment<byte> img = f[nameof(img)];

            using var dc = NewDbContext();
            dc.Sql("UPDATE lots SET ").T(col).T(" = @1 WHERE id = @2");
            if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
            {
                wc.Give(200); // ok
            }
            else
                wc.Give(500); // internal server error
        }
    }
}

public class PublyLotVarWork : LotVarWork
{
    public override async Task @default(WebContext wc, int tracenum)
    {
        int id = wc[0];

        const short msk = 255 | MSK_AUX;
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty, msk).T(" FROM lots_vw WHERE id = @1");
        var o = await dc.QueryTopAsync<Lot>(p => p.Set(id), msk);

        if (o == null)
        {
            wc.GivePage(200, h => { h.ALERT("无效的溯源产品批次"); });
            return;
        }

        Org src = null;
        if (o.srcid > 0)
        {
            src = GrabTwin<int, Org>(o.srcid);
        }

        wc.GivePage(200, h =>
        {
            h.TOPBARXL_();
            h.HEADER_("uk-width-expand uk-col uk-padding-small-left").H2(o.name)._HEADER();
            if (o.icon)
            {
                h.PIC("/lot/", o.id, "/icon", circle: true, css: "uk-width-small");
            }
            else
                h.PIC("/void.webp", circle: true, css: "uk-width-small");

            h._TOPBARXL();

            ShowLot(h, o, src, false, true, tracenum);

            h.FOOTER_("uk-col uk-flex-middle uk-background-muted");
            h.SPAN("金中关（北京）信息技术研究院", css: "uk-padding-small");
            h.SPAN("江西同其成科技有限公司", css: "uk-padding-small");
            h._FOOTER();
        }, true, 3600, title: "中惠农通产品溯源信息");
    }

    const int MAXAGE = 3600 * 6;

    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), true, MAXAGE);
    }

    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), true, MAXAGE);
    }

    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, nameof(m) + sub, true, MAXAGE);
    }
}

public class SuplyLotVarWork : LotVarWork
{
    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui(tip: "调整产品批次", icon: "pencil", status: 3), Tool(Modal.ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int lotid = wc[0];
        var org = wc[-2].As<Org>();
        var cats = Grab<short, Cat>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1 AND orgid = @2");
            var o = await dc.QueryTopAsync<Lot>(p => p.Set(lotid).Set(org.id));

            var srcs = GrabTwinArray<int, Org>(o.orgid);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().TEXT("产品名", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().SELECT("分类", nameof(o.cattyp), o.cattyp, cats, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("产源设施", nameof(o.srcid), o.srcid, srcs, required: o.typ == Lot.TYP_SRC)._LI();
                h.LI_().SELECT("零售单位", nameof(o.unit), o.unit, Unit.Typs, showkey: true).SELECT("单位含重", nameof(o.unitw), o.unitw, Unit.Weights)._LI();
                h.LI_().NUMBER("整件", nameof(o.unitx), o.unitx, min: 1, money: false).NUMBER("批次件数", nameof(o.cap), o.cap)._LI();

                h._FIELDSUL().FIELDSUL_("销售参数");

                if (o.typ == Lot.TYP_SRC)
                {
                    h.LI_().DATE("发货约期", nameof(o.shipon), o.shipon, disabled: true)._LI();
                }
                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("优惠立减", nameof(o.off), o.off, min: 0.00M, max: 999.99M)._LI();
                h.LI_().NUMBER("起订件数", nameof(o.min), o.min, min: 0, max: o.stock).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: o.stock)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            // populate 
            var o = await wc.ReadObjectAsync(msk, instance: new Lot
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE lots ")._SET_(Lot.Empty, msk).T(" WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p =>
            {
                o.Write(p, msk);
                p.Set(lotid).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui(tip: "图标", icon: "github-alt", status: 3), Tool(Modal.ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image", status: 3), Tool(Modal.ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui(tip: "资料", icon: "album", status: 3), Tool(Modal.ButtonCrop, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, nameof(m) + sub, false, 6);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui("质控", "溯源码以及质检材料", status: 3), Tool(Modal.ButtonShow)]
    public async Task tag(WebContext wc, int cmd)
    {
        int lotid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1 AND orgid = @2");
            var o = dc.QueryTop<Lot>(p => p.Set(lotid).Set(org.id));

            if (cmd == 0)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("溯源编号绑定");

                    h.LI_().NUMBER("起始编号", nameof(o.nstart), o.nstart)._LI();
                    h.LI_().NUMBER("截止编号", nameof(o.nend), o.nend)._LI();

                    h._FIELDSUL().FIELDSUL_("质控材料链接");

                    h.LI_().URL("合格证", nameof(o.linka), o.linka)._LI();
                    h.LI_().URL("检测报告", nameof(o.linkb), o.linkb)._LI();

                    h._FIELDSUL().FIELDSUL_("打印溯源贴标");

                    h.LI_().LABEL(string.Empty).AGOTO_(nameof(tag), 1, parent: false, css: "uk-button uk-button-secondary").T("打印专属贴标")._A()._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(tag), subscript: cmd)._FORM();
                });
            }
            else // cmd = (page - 1)
            {
                const short NUM = 90;

                var src = o.srcid == 0 ? null : GrabTwin<int, Org>(o.srcid);

                wc.GivePane(200, h =>
                {
                    h.UL_(css: "uk-grid uk-child-width-1-6");

                    var today = DateTime.Today;
                    var idx = (cmd - 1) * NUM;
                    for (var i = 0; i < NUM; i++)
                    {
                        h.LI_("height-1-15");

                        h.HEADER_();
                        h.QRCODE(MainApp.WwwUrl + "/lot/" + o.id + "/", css: "uk-width-1-3");
                        h.ASIDE_().H6_().T(Application.Node.name)._H6().SMALL_().T(today, date: 3, time: 0)._SMALL()._ASIDE();
                        h._HEADER();

                        // h.H6_("uk-flex").T(lotid, digits: 8).T('-').T(idx + 1).SPAN(Src.Ranks[src?.rank ?? 0], "uk-margin-auto-left")._H6();

                        h._LI();

                        if (++idx >= o.cap)
                        {
                            break;
                        }
                    }
                    h._UL();

                    h.PAGINATION(idx < o.cap, begin: 2, print: true);
                });
            }
        }
        else // POST
        {
            if (cmd == 0)
            {
                var f = await wc.ReadAsync<Form>();
                int nstart = f[nameof(nstart)];
                int nend = f[nameof(nend)];
                string linka = f[nameof(linka)];
                string linkb = f[nameof(linkb)];

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE lots SET status = 2, adapted = @1, adapter = @2, nstart = @3, nend = @4, linka = @5, linkb = @6 WHERE id = @7");
                await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(nstart).Set(nend).Set(linka).Set(linkb).Set(lotid));
            }

            wc.GivePane(200); // close
        }
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_LOG)]
    [Ui("货架", "管理供应数量", status: 7), Tool(Modal.ButtonShow)]
    public async Task stock(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        short optyp = 0;
        int hubid = 0;
        int qtyx = 1;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            await dc.QueryTopAsync("SELECT ops FROM lots_vw WHERE id = @1", p => p.Set(id));
            dc.Let(out StockOp[] ops);

            var arr = GrabTwinArray<int, Org>(0, filter: x => x.IsHub, sorter: (x, y) => y.id - x.id);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("货架操作");
                h.LI_().SELECT("操作", nameof(optyp), optyp, StockOp.Typs, required: true)._LI();
                h.LI_().NUMBER("件数", nameof(qtyx), qtyx, min: 1)._LI();
                h.LI_().SELECT("云仓", nameof(hubid), hubid, arr)._LI();
                h._FIELDSUL();

                h.TABLE(ops, o =>
                    {
                        h.TD_().T(o.dt, date: 2, time: 1)._TD();
                        h.TD2(StockOp.Typs[o.typ], o.qty, css: "uk-text-right");
                        h.TD_();
                        if (hubid > 0)
                        {
                            h.T(GrabTwin<int, Org>(o.hub).name);
                        }
                        h._TD();
                        h.TD(o.stock, right: true);
                        h.TD(o.by);
                    },
                    thead: () => h.TH("时间").TH("摘要").TH("云仓").TH("余量").TH("操作"),
                    reverse: true
                );

                h.BOTTOM_BUTTON("确认", nameof(stock))._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            optyp = f[nameof(optyp)];
            qtyx = f[nameof(qtyx)];
            hubid = f[nameof(hubid)];

            if (!StockOp.IsAddOp(optyp))
            {
                qtyx = -qtyx;
            }
            using var dc = NewDbContext(IsolationLevel.ReadUncommitted);

            await dc.QueryTopAsync("SELECT unitx FROM lots_vw WHERE id = @1", p => p.Set(id));
            dc.Let(out int unitx);
            int qty = qtyx * unitx;

            if (hubid > 0)
            {
                await dc.QueryTopAsync("INSERT INTO lotinvs VALUES (@1, @2, @3) ON CONFLICT (lotid, hubid) DO UPDATE SET stock = (lotinvs.stock + @3) RETURNING stock", p => p.Set(id).Set(hubid).Set(qty));
            }
            else
            {
                await dc.QueryTopAsync("UPDATE lots SET stock = stock + @1 WHERE id = @2 RETURNING stock", p => p.Set(qty).Set(id));
            }
            dc.Let(out int stock);

            dc.Sql("UPDATE lots SET ops = ops || ROW(@1, @2, @3, @4, @5, @6)::StockOp WHERE id = @7 AND orgid = @8");
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(qty).Set(stock).Set(optyp).Set(prin.name).Set(hubid).Set(id).Set(org.id));

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_MGT)]
    [Ui("上线", "上线投入使用", status: 3), Tool(Modal.ButtonConfirm, state: Lot.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE lots SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.Give(200);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_MGT)]
    [Ui("下线", "下线停用或调整", status: 4), Tool(Modal.ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE lots SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2")._MEET_(wc);
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.Give(200);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_MGT)]
    [Ui(tip: "作废此产品批次", icon: "trash", status: 3), Tool(Modal.ButtonConfirm)]
    public async Task @void(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE lots SET status = 0, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4 AND status BETWEEN 1 AND 2");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.Give(200);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_MGT)]
    [Ui(tip: "恢复", icon: "reply", status: 0), Tool(Modal.ButtonConfirm)]
    public async Task unvoid(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE lots SET status = CASE WHEN adapter IS NULL THEN 1 ELSE 2 END, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2 AND status = 0");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.Give(204); // no content
    }
}

public class RtllyPurLotVarWork : LotVarWork
{
    //
    // NOTE: this page is made publicly cacheable, though under variable path
    //
    public override async Task @default(WebContext wc, int hubid)
    {
        int lotid = wc[0];


        const short Msk = 0xff | MSK_EXTRA;
        Lot o;
        if (hubid > 0)
        {
            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Lot.Empty, alias: "o").T(", d.stock FROM lots_vw o, lotinvs d WHERE o.id = d.lotid AND o.id = @1");
            o = await dc.QueryTopAsync<Lot>(p => p.Set(lotid), Msk);
        }
        else
        {
            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Lot.Empty, Msk).T(" FROM lots_vw WHERE id = @1");
            o = await dc.QueryTopAsync<Lot>(p => p.Set(lotid), Msk);
        }

        Org src = null;
        if (o.srcid > 0)
        {
            src = GrabTwin<int, Org>(o.srcid);
        }

        wc.GivePane(200, h =>
        {
            ShowLot(h, o, src, false, false);

            // bottom bar
            //
            var fee = BankUtility.supfee;

            var realprice = o.RealPrice;
            int qtyx = 1;
            short unitx = o.unitx;
            int qty = qtyx * unitx;
            decimal topay = qty * o.RealPrice + fee;

            h.BOTTOMBAR_();
            h.FORM_("uk-flex uk-flex-middle uk-width-1-1 uk-height-1-1", oninput: $"qty.value = (qtyx.value * {unitx}).toFixed(); topay.value = ({realprice} * qty.value + (fee ? parseFloat(fee.value) : 0) ).toFixed(2);");

            h.HIDDEN(nameof(realprice), realprice);

            h.SELECT_(null, nameof(qtyx), css: "uk-width-small");
            for (int i = 1; i <= Math.Min(o.max, o.StockX); i += (i >= 120 ? 5 : i >= 60 ? 2 : 1))
            {
                h.OPTION_(i).T(i).SP().T('件')._OPTION();
            }
            h._SELECT().SP();
            h.SPAN_("uk-width-expand uk-padding").T("共").SP().OUTPUT(nameof(qty), qty).SP().T(o.unit);
            if (o.IsOnHub)
            {
                h.H6_("uk-margin-auto-left").T("到市场运费 +").OUTPUT(nameof(fee), fee)._H6();
            }
            h._SPAN();

            // pay button
            h.BUTTON_(nameof(pur), onclick: "return $pur(this);", css: "uk-button-danger uk-width-medium uk-height-1-1").CNYOUTPUT(nameof(topay), topay)._BUTTON();

            h._FORM();
            h._BOTTOMBAR();
        }, true, 120); // NOTE publicly cacheable though within a private context
    }

    public async Task pur(WebContext wc, int cmd)
    {
        var rtl = wc[-3].As<Org>();
        int lotid = wc[0];

        var prin = (User)wc.Principal;

        // submitted values
        var f = await wc.ReadAsync<Form>();
        short qtyx = f[nameof(qtyx)];

        using var dc = NewDbContext(IsolationLevel.ReadCommitted);
        try
        {
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1");
            var lot = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

            var qty = qtyx * lot.unitx;
            var sup = GrabTwin<int, Org>(lot.orgid);
            var fee = BankUtility.supfee;

            var o = new Pur(lot, rtl, sup)
            {
                created = DateTime.Now,
                creator = prin.name,
                qty = qty,
                fee = fee,
                topay = lot.RealPrice * qty + fee,
                status = -1
            };

            // check and try to use an existing record
            int purid = 0;
            if (await dc.QueryTopAsync("SELECT id FROM purs WHERE rtlid = @1 AND status = -1 LIMIT 1", p => p.Set(rtl.id)))
            {
                dc.Let(out purid);
            }

            // make use of any existing abandoned record
            const short msk = MSK_BORN | MSK_EDIT | MSK_STATUS;
            if (purid == 0)
            {
                dc.Sql("INSERT INTO purs ").colset(Pur.Empty, msk)._VALUES_(Pur.Empty, msk).T(" RETURNING id, topay");
                await dc.QueryTopAsync(p => o.Write(p));
            }
            else
            {
                dc.Sql("UPDATE purs ")._SET_(Pur.Empty, msk).T(" WHERE id = @1 RETURNING id, topay");
                await dc.QueryTopAsync(p =>
                {
                    o.Write(p);
                    p.Set(purid);
                });
            }
            dc.Let(out purid);
            dc.Let(out decimal topay);

            // call WeChatPay to prepare order there
            string trade_no = (purid + "-" + topay).Replace('.', '-');
            var (prepay_id, err_code) = await WeChatUtility.PostUnifiedOrderAsync(sup: true,
                trade_no,
                topay,
                prin.im, // the payer
                wc.RemoteIpAddress.ToString(),
                MainApp.MgtUrl + "/" + nameof(MgtService.onpay),
                o.ToString()
            );

            if (prepay_id != null)
            {
                wc.Give(200, WeChatUtility.BuildPrepayContent(prepay_id));
            }
            else
            {
                dc.Rollback();
                wc.Give(500);
            }
        }
        catch (Exception e)
        {
            dc.Rollback();
            Application.Err(e.Message);
            wc.Give(500);
        }
    }
}