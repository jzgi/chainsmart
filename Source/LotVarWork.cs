using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Application;
using static ChainFx.Entity;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart;

public class LotVarWork : WebWork
{
    static readonly string
        FabUrl = MainApp.WwwUrl + "/fab/",
        LotUrl = MainApp.WwwUrl + "/lot/",
        OrgUrl = MainApp.WwwUrl + "/org/";

    internal static void LotShow(HtmlBuilder h, Lot o, Org org, Fab fab, bool pricing, int tracenum = 0)
    {
        h.ARTICLE_("uk-card uk-card-primary");
        h.H4("产品信息", "uk-card-header");
        h.SECTION_("uk-card-body");
        if (o.pic)
        {
            h.PIC(LotUrl, o.id, "/pic", css: "uk-width-1-1");
        }

        h.UL_("uk-list uk-list-divider");
        h.LI_().FIELD("产品名", o.name).FIELD("分类", o.cattyp)._LI();
        h.LI_().FIELD("简介", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
        h.LI_().FIELD("交易类型", Lot.Typs[o.typ]);
        if (o.IsAdvance)
        {
            h.FIELD("输运起始日", o.started);
        }
        h._LI();

        if (pricing)
        {
            h.LI_().FIELD("零计单位", o.unit).FIELD("单位含重", o.unitw, Unit.Metrics[o.unitw])._LI();
            h.LI_().FIELD2("每件含", o.unitx, o.unit)._LI();
            h.LI_().FIELD("单价", o.price, true).FIELD("秒杀直降", o.off, true)._LI();
            h.LI_().FIELD("起订件数", o.minx).FIELD("限订件数", o.maxx)._LI();
        }

        h._UL();
        h._SECTION();
        h._ARTICLE();

        h.ARTICLE_("uk-card uk-card-primary");
        h.H4("批次检验", "uk-card-header");
        h.SECTION_("uk-card-body");
        h.UL_("uk-list uk-list-divider");
        h.LI_().FIELD("批次编号", o.id, digits: 8)._LI();

        if (o.nstart > 0 && o.nend > 0)
        {
            h.LI_().FIELD2("溯源码范围", $"{o.nstart:0000 0000}", $"{o.nend:0000 0000}", "－")._LI();
        }

        if (tracenum > 0)
        {
            h.LI_("uk-background-secondary").FIELD("本溯源码", $"{tracenum:0000 0000}")._LI();
            if (o.TryGetInvOp(tracenum, out var v))
            {
                h.LI_("uk-background-secondary").FIELD("中转库操作", v.dt)._LI();
            }
        }

        h.LI_().FIELD2("登记", o.created, o.creator)._LI();
        if (o.adapter != null) h.LI_().FIELD2("制码", o.adapted, o.adapter)._LI();

        h._UL();

        if (o.m1)
        {
            h.PIC(LotUrl, o.id, "/m-1", css: "uk-width-1-1");
        }

        if (o.m2)
        {
            h.PIC(LotUrl, o.id, "/m-2", css: "uk-width-1-1");
        }

        if (o.m3)
        {
            h.PIC(LotUrl, o.id, "/m-3", css: "uk-width-1-1");
        }

        if (o.m4)
        {
            h.PIC(LotUrl, o.id, "/m-4", css: "uk-width-1-1");
        }

        h._SECTION();
        h._ARTICLE();

        // ASSEET

        if (fab != null)
        {
            h.ARTICLE_("uk-card uk-card-primary");
            h.H3("产品源", "uk-card-header");
            h.SECTION_("uk-card-body");
            if (fab.pic)
            {
                h.PIC(FabUrl, fab.id, "/pic", css: "uk-width-1-1");
            }

            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("产品源", fab.name)._LI();
            h.LI_().FIELD("简介", fab.tip)._LI();
            h.LI_().FIELD("等级", fab.rank, Fab.Ranks)._LI();
            h.LI_().FIELD("说明", fab.remark)._LI();
            h._UL();

            if (fab.m1)
            {
                h.PIC(FabUrl, fab.id, "/m-1", css: "uk-width-1-1");
            }

            if (fab.m2)
            {
                h.PIC(FabUrl, fab.id, "/m-2", css: "uk-width-1-1");
            }

            if (fab.m3)
            {
                h.PIC(FabUrl, fab.id, "/m-3", css: "uk-width-1-1");
            }

            if (fab.m4)
            {
                h.PIC(FabUrl, fab.id, "/m-4", css: "uk-width-1-1");
            }

            h._SECTION();
            h._ARTICLE();
        }
        //
        // SUP

        h.ARTICLE_("uk-card uk-card-primary");
        h.H4("供应信息", "uk-card-header");
        h.SECTION_("uk-card-body");
        if (org.pic)
        {
            h.PIC(OrgUrl, org.id, "/pic", css: "uk-width-1-1");
        }

        h.UL_("uk-list uk-list-divider");
        h.LI_().FIELD("商户名", org.name)._LI();
        h.LI_().FIELD("简介", org.tip)._LI();
        h.LI_().FIELD("工商登记名", org.legal)._LI();
        h.LI_().FIELD("联系电话", org.tel)._LI();
        h.LI_().FIELD("地址／场地", org.addr)._LI();
        h.LI_().FIELD("指标参数", org.specs)._LI();
        h.LI_().FIELD2("登记", org.created, org.creator)._LI();

        h._UL();

        if (org.m1)
        {
            h.PIC(OrgUrl, org.id, "/m-1", css: "uk-width-1-1");
        }

        if (org.m2)
        {
            h.PIC(OrgUrl, org.id, "/m-2", css: "uk-width-1-1");
        }

        if (org.m3)
        {
            h.PIC(OrgUrl, org.id, "/m-3", css: "uk-width-1-1");
        }

        if (org.scene)
        {
            h.PIC(OrgUrl, org.id, "/m-4", css: "uk-width-1-1");
        }

        h._SECTION();
        h._ARTICLE();
    }

    public virtual async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1");
        var o = await dc.QueryTopAsync<Lot>(p => p.Set(id));

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("产品名", o.name)._LI();
            h.LI_().FIELD("简介", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
            h.LI_().FIELD("交易类型", Lot.Typs[o.typ]);
            if (o.typ == 2) h.FIELD("输运起始日", o.started);
            h._LI();

            h.LI_().FIELD("零计单位", o.unit).FIELD2("每件含单位", o.unitx, o.unit)._LI();
            h.LI_().FIELD("批次件数", o.capx)._LI();
            h.LI_().FIELD("单价", o.price, true).FIELD("秒杀直降", o.off, true)._LI();
            h.LI_().FIELD("起订件数", o.minx).FIELD("限订件数", o.maxx)._LI();
            h.LI_().FIELD2("溯源编号", o.nstart, o.nend, "－")._LI();

            h._UL();

            h.TABLE(o.ops, o =>
                {
                    h.TD_().T(o.dt, time: 1)._TD();
                    h.TD(o.typ);
                    h.TD(o.qty, right: true);
                    h.TD(o.stock, right: true);
                    h.TD(o.by);
                },
                thead: () => h.TH("操作时间").TH("摘要").TH("数量").TH("余量").TH("操作"),
                reverse: true
            );

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
    public override async Task @default(WebContext wc)
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

        var org = GrabTwin<int, Org>(o.orgid);
        Fab fab = null;
        if (o.fabid > 0)
        {
            fab = GrabTwin<int, Fab>(o.fabid);
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

            LotShow(h, o, org, fab, false);

            h.FOOTER_("uk-col uk-flex-middle uk-margin-large-top uk-margin-bottom");
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
    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改产品批次", icon: "pencil", status: 3), Tool(ButtonShow)]
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

            var fabs = GrabTwinSet<int, Fab>(o.orgid);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(o.IsSpot ? "现货（货入品控库之后再销售）" : "助农（货入品控库之前先销售）");

                h.LI_().TEXT("产品名称", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().SELECT("分类", nameof(o.cattyp), o.cattyp, cats, required: true)._LI();
                h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("产品源", nameof(o.fabid), o.fabid, fabs)._LI();
                if (o.IsAdvance)
                {
                    h.LI_().DATE("交货起始日", nameof(o.started), o.started)._LI();
                }
                h.LI_().SELECT("零售单位", nameof(o.unit), o.unit, Unit.Typs, showkey: true).SELECT("单位含重", nameof(o.unitw), o.unitw, Unit.Metrics)._LI();
                h.LI_().NUMBER("整件含单位", nameof(o.unitx), o.unitx, min: 1, money: false).NUMBER("批次整件数", nameof(o.capx), o.capx)._LI();

                h._FIELDSUL().FIELDSUL_("销售及优惠");

                h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.01M, max: 99999.99M).NUMBER("秒杀直降", nameof(o.off), o.off, min: 0.00M, max: 999.99M)._LI();
                h.LI_().NUMBER("起订整件数", nameof(o.minx), o.minx, min: 1, max: o.StockX).NUMBER("限订整件数", nameof(o.maxx), o.maxx, min: 1, max: o.StockX)._LI();

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

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "图标", icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "资料", icon: "album", status: 3), Tool(ButtonCrop, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, nameof(m) + sub, false, 6);
    }

    [OrglyAuthorize(0, User.ROL_OPN, ulevel: 2)]
    [Ui("溯源", "溯源码绑定或印制", icon: "tag", status: 3), Tool(ButtonShow)]
    public async Task tag(WebContext wc, int cmd)
    {
        int lotid = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            if (cmd == 0)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("溯源标签方式");
                    h.LI_().AGOTO("Ａ）绑定预制标签", nameof(tag), 1)._LI();
                    h.LI_().AGOTO("Ｂ）现场印制专属贴标", nameof(tag), 2)._LI();
                    h._FIELDSUL()._FORM();
                });
                return;
            }

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1 AND orgid = @2");
            var o = dc.QueryTop<Lot>(p => p.Set(lotid).Set(org.id));

            var fab = o.fabid == 0 ? null : GrabTwin<int, Fab>(o.fabid);

            if (cmd == 1)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("绑定预制标签号码");

                    h.LI_().NUMBER("起始号码", nameof(o.nstart), o.nstart)._LI();
                    h.LI_().NUMBER("截至号码", nameof(o.nend), o.nend)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(tag), subscript: cmd)._FORM();
                });
            }
            else // cmd = (page - 1)
            {
                const short NUM = 90;

                wc.GivePane(200, h =>
                {
                    h.UL_(css: "uk-grid uk-child-width-1-6");

                    var today = DateTime.Today;
                    var idx = (cmd - 2) * NUM;
                    for (var i = 0; i < NUM; i++)
                    {
                        h.LI_("height-1-15");

                        h.HEADER_();
                        h.QRCODE(MainApp.WwwUrl + "/lot/" + o.id + "/", css: "uk-width-1-3");
                        h.ASIDE_().H6_().T(Application.Name)._H6().SMALL_().T(today, date: 3, time: 0)._SMALL()._ASIDE();
                        h._HEADER();

                        h.H6_("uk-flex").T(lotid, digits: 8).T('-').T(idx + 1).SPAN(Fab.Ranks[fab?.rank ?? 0], "uk-margin-auto-left")._H6();

                        h._LI();

                        if (++idx >= o.capx)
                        {
                            break;
                        }
                    }

                    h._UL();

                    h.PAGINATION(idx < o.capx, begin: 2, print: true);
                });
            }
        }
        else // POST
        {
            if (cmd == 1)
            {
                var f = await wc.ReadAsync<Form>();
                int nstart = f[nameof(nstart)];
                int nend = f[nameof(nend)];

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE lots SET nstart = @1, nend = @2, status = 2, adapted = @3, adapter = @4 WHERE id = @5");
                await dc.ExecuteAsync(p => p.Set(nstart).Set(nend).Set(DateTime.Now).Set(prin.name).Set(lotid));
            }
            else if (cmd == 2)
            {
            }

            wc.GivePane(200); // close
        }
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("上线", "上线投入使用", icon: "cloud-upload", status: 3), Tool(ButtonConfirm, state: Lot.STA_OKABLE)]
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

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("下线", "下线以便修改", icon: "cloud-download", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE lots SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2")._MEET_(wc);
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.Give(200);
    }

    [OrglyAuthorize(0, User.ROL_LOG, ulevel: 2)]
    [Ui("数量", icon: "database", status: 7), Tool(ButtonShow)]
    public async Task stock(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        short optyp = 0;
        int hub = 0;
        int qtyx = 1;

        if (wc.IsGet)
        {
            var arr = GrabTwinSet<int, Org>(0, filter: x => x.IsCenter, sorter: (x, y) => y.id - x.id);

            using var dc = NewDbContext();

            await dc.ExecuteAsync("SELECT hubid, stock::varchar FROM lotinvs WHERE lotid = @1");
            var map = dc.ToIntMap();

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("中转库");

                for (var i = 0; i < arr.Length; i++)
                {
                    var o = arr[i];
                    var stock = map?[o.id];
                    h.RADIO(nameof(hub), o.id, label: o.name, required: i == 0, tip: stock);
                }

                h._FIELDSUL().FIELDSUL_("数量操作");
                h.LI_().SELECT("操作", nameof(optyp), optyp, StockOp.Typs, required: true)._LI();
                h.LI_().NUMBER("件数", nameof(qtyx), qtyx, min: 1)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(stock))._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            optyp = f[nameof(optyp)];
            qtyx = f[nameof(qtyx)];
            hub = f[nameof(hub)];

            if (!StockOp.IsAddOp(optyp))
            {
                qtyx = -qtyx;
            }
            using var dc = NewDbContext(IsolationLevel.ReadUncommitted);

            await dc.QueryTopAsync("SELECT unitx FROM lots_vw WHERE id = @1", p => p.Set(id));
            dc.Let(out int unitx);
            int qty = qtyx * unitx;

            await dc.QueryTopAsync("INSERT INTO lotinvs VALUES (@1, @2, @3) ON CONFLICT (lotid, hubid) DO UPDATE SET stock = (lotinvs.stock + @3) RETURNING stock", p => p.Set(id).Set(hub).Set(qty));
            dc.Let(out int stock);

            dc.Sql("UPDATE lots SET ops = ops || ROW(@1, @2, @3, @4, @5, @6)::StockOp WHERE id = @7 AND orgid = @8");
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(qty).Set(stock).Set(optyp).Set(prin.name).Set(hub).Set(id).Set(org.id));

            wc.GivePane(200); // close dialog
        }
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui(tip: "删除或者作废该批次", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE lots SET status = 0 WHERE id = @1 AND orgid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.Give(204); // no content
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui(tip: "恢复", icon: "reply", status: 0), Tool(ButtonConfirm)]
    public async Task restore(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        try
        {
            dc.Sql("UPDATE lots SET status = CASE WHEN adapter IS NULL 2 ELSE 1 END WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));
        }
        catch (Exception)
        {
        }

        wc.Give(204); // no content
    }
}

public class CtrlyLotVarWork : LotVarWork
{
}

public class RtllyPurLotVarWork : LotVarWork
{
    //
    // NOTE: this page is made publicly cacheable, though under variable path
    //
    public new async Task @default(WebContext wc)
    {
        int lotid = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1");
        var o = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

        var org = GrabTwin<int, Org>(o.orgid);
        Fab fab = null;
        if (o.fabid > 0)
        {
            fab = GrabTwin<int, Fab>(o.fabid);
        }

        wc.GivePane(200, h =>
        {
            LotShow(h, o, org, fab, true);

            // bottom bar
            //
            decimal realprice = o.RealPrice;
            int qtyx = 1;
            short unitx = o.unitx;
            int qty = qtyx * unitx;
            decimal topay = qty * o.RealPrice;

            h.BOTTOMBAR_();
            h.FORM_("uk-flex uk-flex-middle uk-width-1-1 uk-height-1-1", oninput: $"qty.value = (qtyx.value * {unitx}).toFixed(); topay.value = ({realprice} * qty.value).toFixed(2);");

            h.HIDDEN(nameof(realprice), realprice);

            h.SELECT_(null, nameof(qtyx), css: "uk-width-small");
            for (int i = 1; i <= Math.Min(o.maxx, o.StockX); i += (i >= 120 ? 5 : i >= 60 ? 2 : 1))
            {
                h.OPTION_(i).T(i)._OPTION();
            }
            h._SELECT().SP();
            h.SPAN_("uk-width-expand").T(" 整件，共").SP().OUTPUT(nameof(qty), qty).SP().T(o.unit)._SPAN();

            // pay button
            h.BUTTON_(nameof(pur), onclick: "return $pur(this);", css: "uk-button-danger uk-width-medium uk-height-1-1").CNYOUTPUT(nameof(topay), topay)._BUTTON();

            h._FORM();
            h._BOTTOMBAR();
        }, true, 60); // NOTE publicly cacheable though within a private context
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

            var m = new Pur(lot, rtl, sup)
            {
                created = DateTime.Now,
                creator = prin.name,
                qty = qty,
                topay = lot.RealPrice * qty,
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
                await dc.QueryTopAsync(p => m.Write(p));
            }
            else
            {
                dc.Sql("UPDATE purs ")._SET_(Pur.Empty, msk).T(" WHERE id = @1 RETURNING id, topay");
                await dc.QueryTopAsync(p => m.Write(p));
            }
            dc.Let(out purid);
            dc.Let(out decimal topay);

            // call WeChatPay to prepare order there
            string trade_no = (purid + "-" + topay).Replace('.', '-');
            var (prepay_id, err_code) = await WeixinUtility.PostUnifiedOrderAsync(sup: true,
                trade_no,
                topay,
                prin.im, // the payer
                wc.RemoteIpAddress.ToString(),
                MainApp.MgtUrl + "/" + nameof(MgtService.onpay),
                m.ToString()
            );

            if (prepay_id != null)
            {
                wc.Give(200, WeixinUtility.BuildPrepayContent(prepay_id));
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
            Err(e.Message);
            wc.Give(500);
        }
    }
}