using System;
using System.Data;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Web.Modal;
using static ChainSmart.CloudUtility;
using static ChainFX.Application;
using static ChainFX.Nodal.Storage;

namespace ChainSmart;

public abstract class BuyVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE id = @1");
        var o = await dc.QueryTopAsync<Buy>(p => p.Set(id));

        wc.GivePane(200, h =>
        {
            h.H4("基本", css: "uk-padding");
            h.UL_("uk-list uk-list-divider");
            h.LI_().LABEL("单号").SPAN_("uk-static").T(o.id, digits: 8).T('（').T(o.created, time: 2).T('）')._SPAN()._LI();
            if (o.IsOnline)
            {
                h.LI_().LABEL("买家").SPAN_("uk-static").T(o.uname).SP().ATEL(o.utel, o.utel).AICON($"javascript: navigator.clipboard.writeText('{o.uname} {o.utel} {o.uaddr}');", "copy",css:"uk-margin-auto-left")._SPAN()._LI();
                h.LI_().LABEL(string.Empty).SPAN_("uk-static").T(o.uarea).T('-').T(o.uaddr).SP()._SPAN()._LI();
            }
            h.LI_().LABEL("明细");
            h.TABLE(o.lns, d =>
            {
                h.TD_().T(d.name);
                if (d.unitip != null)
                {
                    h.SP().SMALL_().T(d.unitip)._SMALL();
                }
                h._TD();
                h.TD_(css: "uk-text-right").CNY(d.RealPrice)._TD();
                h.TD2(d.qty, d.unit, css: "uk-text-right");
            });
            h._LI();
            h.LI_().FIELD("运费", o.fee, true).FIELD("总额", o.topay, true)._LI();
            h.LI_().FIELD("模式", o.mode, Org.Modes)._LI();
            h._UL();

            h.H4("状态", css: "uk-padding");
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("状态", o.status, Buy.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("支付", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "撤销" : "派发", o.oker, o.oked, sep: "<br>")._LI();
            h._UL();


            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        });
    }
}

public class MyBuyVarWork : BuyVarWork
{
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE buys SET oked = @1, oker = @2, status = 4 WHERE id = @3 AND uid = @4 AND status = 2 RETURNING orgid, pay");
        if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(prin.id)))
        {
            dc.Let(out int orgid);
            dc.Let(out decimal pay);
        }

        wc.Give(200);
    }
}

[Ui("订单操作")]
public class ShplyBuyVarWork : BuyVarWork
{
    [MgtAuthorize(Org.TYP_RTL_, User.ROL_DLV)]
    [Ui("派发", "给买家派发商品", icon: "forward", status: 2), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE buys SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4 AND status = 2 RETURNING uim, pay");
        if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)))
        {
            dc.Let(out string uim);
            dc.Let(out decimal pay);

            await PostSendAsync(uim, $"商品开始派发，请留意收货（{org.name}，单号{id:D8}，￥{pay}）");
        }

        wc.Give(200);
    }

    [Ui("回退", tip: "回退到收单状态", icon: "reply", status: 4), Tool(ButtonConfirm, state: Buy.STA_REVERSABLE)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE buys SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2 AND status = 4");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.Give(200);
    }

    [MgtAuthorize(Org.TYP_RTL_, User.ROL_OPN)]
    [Ui("打印", "打印小票", icon: "print", status: 2 | 4), Tool(ButtonScript)]
    public async Task @out(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE id = @1");
        var o = await dc.QueryTopAsync<Buy>(p => p.Set(id));

        var cnt = new JsonBuilder(true, 1024 * 4) { };
        cnt.Put(null, o);

        wc.Give(200, cnt, shared: false, 12);
    }

    [MgtAuthorize(Org.TYP_RTL_, User.ROL_OPN)]
    [Ui("撤销", "撤销该单并全款退回消费者", icon: "trash", status: 2), Tool(ButtonConfirm)]
    public async Task @void(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext(IsolationLevel.ReadCommitted);
        try
        {
            dc.Sql("UPDATE buys SET refund = pay, status = 0, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4 AND status = 2 RETURNING uim, topay, refund");
            if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)))
            {
                dc.Let(out string uim);
                dc.Let(out decimal topay);
                dc.Let(out decimal refund);

                // remote call
                var trade_no = Buy.GetOutTradeNo(id, topay);
                string err = await PostRefundAsync(sup: false, trade_no, refund, refund, trade_no);
                if (err != null) // not success
                {
                    dc.Rollback();
                    Err(err);
                }
                else
                {
                    // notify user
                    await PostSendAsync(uim, "您的订单已经撤销，请查收退款（" + org.name + "　#" + trade_no + "　￥" + refund + "）");
                }
            }
        }
        catch (Exception)
        {
            dc.Rollback();
            Err("退款失败，订单号：" + id);
            return;
        }

        wc.Give(200);
    }
}

public class MktlyBuyVarWork : BuyVarWork
{
    public async Task com(WebContext wc)
    {
        string com = wc[0];
        var mkt = wc[-2].As<Org>();
        bool noncom = com == "_";

        using var dc = NewDbContext();
        dc.Sql("SELECT localtimestamp(0); SELECT utel, first(uaddr), count(CASE WHEN status = 1 THEN 1 END), count(CASE WHEN status = 2 THEN 2 END) FROM buys WHERE mktid = @1 AND (status = 1 OR status = 2) AND typ = 1 AND ucom ").T(noncom ? "IS NULL" : "= @2").T(" GROUP BY ucom, utel;");
        await dc.QueryTopAsync(p => p.Set(mkt.id).Set(com));

        // the time stamp to fence the range to update
        dc.Let(out DateTime stamp);
        var intstamp = MainUtility.ToInt2020(stamp);

        wc.GivePane(200, h =>
        {
            h.TABLE_();
            h.THEAD_().TH("地址").TH("收单", css: "uk-width-tiny").TH("合单", css: "uk-width-tiny")._THEAD();

            dc.NextResult();
            while (dc.Next())
            {
                dc.Let(out string utel);
                dc.Let(out string uaddr);
                dc.Let(out int created);
                dc.Let(out int adapted);

                h.TR_();
                h.TD_().PICK(utel).SP().SPAN_().ATEL(utel, utel).SP().T(uaddr)._SPAN()._TD();
                h.TD_();
                if (created > 0)
                {
                    h.ADIALOG_(nameof(created), "?utel=", utel, mode: ToolAttribute.MOD_SHOW, false, css: "uk-link uk-button-link uk-flex-center").T(created)._A();
                }
                h._TD();
                h.TD_();
                if (adapted > 0)
                {
                    h.ADIALOG_(nameof(adapted), "?utel=", utel, mode: ToolAttribute.MOD_SHOW, false, css: "uk-link uk-button-link uk-flex-center").T(adapted)._A();
                }
                h._TD();
                h._TR();
            }

            h._TABLE();

            if (!noncom)
            {
                h.TOOLBAR(subscript: intstamp, toggle: true, bottom: true);
            }
        });
    }

    public async Task created(WebContext wc)
    {
        string com = wc[0];
        var mkt = wc[-2].As<Org>();
        string utel = wc.Query[nameof(utel)];

        bool non = com == "_";

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE mktid = @1 AND status = 1 AND typ = 1 AND ucom ").T(non ? "IS NULL" : "= @2").T(" AND utel = @3");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(mkt.id).Set(com).Set(utel));

        wc.GivePane(200, h =>
        {
            h.MAINGRID(arr, o =>
            {
                h.UL_("uk-card-body uk-list uk-list-divider");
                h.LI_().H4_().T(o.name);
                if (o.tip != null)
                {
                    h.T('（').T(o.tip).T('）');
                }
                h._H4();
                h.SPAN_("uk-badge").T(o.created, time: 0).SP().T(Buy.Statuses[o.status])._SPAN()._LI();

                foreach (var it in o.lns)
                {
                    h.LI_();

                    h.SPAN_("uk-width-expand").T(it.name)._SPAN();
                    h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.RealPrice)._SPAN();
                    h.SPAN_("uk-width-tiny uk-flex-right").T(it.qty).SP().T(it.unit)._SPAN();
                    h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.SubTotal)._SPAN();
                    h._LI();
                }
                h._LI();

                h._UL();
            });
        }, false, 6);
    }

    public async Task adapted(WebContext wc)
    {
        string com = wc[0];
        var mkt = wc[-2].As<Org>();
        string utel = wc.Query[nameof(utel)];

        bool non = com == "_";

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE mktid = @1 AND status = 2 AND typ = 1 AND ucom ").T(non ? "IS NULL" : "= @2").T(" AND utel = @3");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(mkt.id).Set(com).Set(utel));

        wc.GivePane(200, h =>
        {
            h.MAINGRID(arr, o =>
            {
                h.UL_("uk-card-body uk-list uk-list-divider");
                h.LI_().H4_().T(o.name);
                if (o.tip != null)
                {
                    h.T('（').T(o.tip).T('）');
                }
                h._H4();
                h.SPAN_("uk-badge").T(o.created, time: 0).SP().T(Buy.Statuses[o.status])._SPAN()._LI();

                foreach (var it in o.lns)
                {
                    h.LI_();

                    h.SPAN_("uk-width-expand").T(it.name);
                    if (it.unitip != null)
                    {
                        h.SP().SMALL_().T(it.unitip).T(it.unit)._SMALL();
                    }
                    h._SPAN();
                    h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.RealPrice)._SPAN();
                    h.SPAN_("uk-width-tiny uk-flex-right").T(it.qty).SP().T(it.unit)._SPAN();
                    h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.SubTotal)._SPAN();
                    h._LI();
                }
                h._LI();

                h._UL();
            });
        }, false, 6);
    }

    [MgtAuthorize(Org.TYP_MKV, User.ROL_DLV)]
    [Ui("派发", "统一派发？"), Tool(ButtonPickConfirm)]
    public async Task ok(WebContext wc, int v2020)
    {
        string com = wc[0];
        var prin = (User)wc.Principal;
        var mkt = wc[-2].As<Org>();
        var stamp = MainUtility.ToDateTime(v2020);

        var f = await wc.ReadAsync<Form>();
        string[] key = f[nameof(key)];

        // Note: update only status = 2
        using var dc = NewDbContext();
        dc.Sql("UPDATE buys SET status = 4, oked = @1, oker = @2 WHERE mktid = @3 AND status = 2 AND typ = 1 AND ucom = @4 AND utel")._IN_(key).T(" AND adapted <= @5");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(mkt.id).Set(com).SetForIn(key).Set(stamp));

        wc.GivePane(200);
    }
}