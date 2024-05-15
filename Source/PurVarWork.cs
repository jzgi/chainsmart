using System;
using System.Data;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Application;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

namespace ChainSmart;

public abstract class PurVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Pur.Empty).T(" FROM purs WHERE id = @1");
        var o = await dc.QueryTopAsync<Pur>(p => p.Set(id));

        var org = GrabTwin<int, Org>(o.orgid);

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");
            h.LI_().LABEL("单号").SPAN_("uk-static").T(o.id, digits: 10).T('（').T(o.created, time: 2).T('）')._SPAN()._LI();
            h.LI_().LABEL("买方").SPAN_("uk-static").T(org.name).SP().ATEL(org.tel)._SPAN()._LI();
            h.LI_().FIELD("产品名", o.name)._LI();
            h.LI_().FIELD("基本单位", o.unit).FIELD("附注", o.unitip)._LI();
            h.LI_().FIELD2("整件", o.unitx, o.unit).FIELD("运费", o.fee, money: true)._LI();
            h.LI_().FIELD("单价", o.price, money: true).FIELD("优惠立减", o.off)._LI();
            h.LI_().FIELD("件数", o.QtyX).FIELD("支付金额", o.pay, money: true)._LI();

            h.LI_().FIELD("状态", o.status, Pur.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2(o.IsVoid ? "撤销" : "发货", o.adapter, o.adapted, sep: "<br>").FIELD2("收货", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        });
    }
}

public class MchlyPurVarWork : PurVarWork
{
}

public class SuplyPurVarWork : PurVarWork
{
    [MgtAuthorize(Org.TYP_SUP_, User.ROL_DLV)]
    [Ui("发货", "确认开始发货", icon: "arrow-right", status: 2), Tool(ButtonConfirm)]
    public async Task adapt(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext(IsolationLevel.ReadUncommitted);
        try
        {
            // set status and decrease the stock
            dc.Sql("UPDATE purs SET status = 2, adapted = @1, adapter = @2 WHERE id = @3 AND supid = @4 AND status = 1 RETURNING hubid, lotid, qty, orgid, topay");
            if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)))
            {
                dc.Let(out int hubid);
                dc.Let(out int lotid);
                dc.Let(out int qty);
                dc.Let(out int orgid);
                dc.Let(out decimal topay);

                // adjust stock
                dc.Sql("UPDATE lotinvs SET stock = stock - @1 WHERE lotid = @2 AND hubid = @3");
                await dc.ExecuteAsync(p => p.Set(qty).Set(lotid).Set(hubid));

                // put a notice to the shop
                var sta = GrabTwin<int, Org>(orgid);
                sta.NoticePack.Put(OrgNoticePack.PUR_OKED, 1, topay);
            }
        }
        catch (Exception)
        {
            dc.Rollback();
            Err("发货操作失败, purid " + id);
            return;
        }

        wc.Give(204);
    }

    [Ui("返现", tip: "返回一定数量的支付款", status: 1 | 2 | 4), Tool(ButtonShow, state: Buy.STA_REVERSABLE)]
    public async Task refund(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        decimal refund = 0;
        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT pay FROM purs WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));
            dc.Let(out decimal pay);

            wc.GivePane(200, h =>
            {
                //
                h.FORM_().FIELDSUL_("返现");
                h.LI_().NUMBER("金额", nameof(refund), refund, min: 0.10M, max: pay)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(refund))._FORM();
            });
        }
        else // POST
        {
            refund = (await wc.ReadAsync<Form>())[nameof(refund)];

            using var dc = NewDbContext(IsolationLevel.ReadCommitted);
            try
            {
                dc.Sql("UPDATE purs SET refund = @1, refunder = @2 WHERE id = @3 AND supid = @4 AND status BETWEEN 1 AND 4 RETURNING orgid, pay");
                if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)))
                {
                    dc.Let(out int orgid);
                    dc.Let(out decimal pay);

                    // remote call to refund
                    var trade_no = Buy.GetOutTradeNo(id, pay);
                    string err = await WeChatUtility.PostRefundAsync(sup: true, trade_no, pay, refund, trade_no, "返现");
                    if (err != null) // not success
                    {
                        dc.Rollback();
                        Err(err);
                    }

                    // put a notice to the shop
                    var sta = GrabTwin<int, Org>(orgid);
                    sta.NoticePack.Put(OrgNoticePack.PUR_REFUND, 1, refund);
                }
            }
            catch (Exception)
            {
                dc.Rollback();
                Err("返现失败, purid " + id);
                return;
            }
        }

        wc.Give(200);
    }

    [MgtAuthorize(Org.TYP_SUP_, User.ROL_OPN)]
    [Ui("撤销", "确认撤销订单并退款？", icon: "trash", status: 7), Tool(ButtonConfirm, state: Pur.STA_CANCELL)]
    public async Task @void(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext(IsolationLevel.ReadCommitted);
        try
        {
            dc.Sql("UPDATE purs SET status = 0, ret = qty, refund = pay, adapted = @1, adapter = @2 WHERE id = @3 AND supid = @4 AND status BETWEEN 1 AND 2 RETURNING lotid, hubid, qty, orgid, topay, refund");
            if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)))
            {
                dc.Let(out int lotid);
                dc.Let(out int hubid);
                dc.Let(out int qty);
                dc.Let(out int orgid);
                dc.Let(out decimal topay);
                dc.Let(out decimal refund);

                // adjust the stock
                dc.Sql("UPDATE lotinvs SET stock = stock + @1 WHERE lotid = @2 AND hubid = @3");
                await dc.ExecuteAsync(p => p.Set(qty).Set(lotid).Set(hubid));

                // remote call to refund
                var trade_no = Buy.GetOutTradeNo(id, topay);
                string err = await WeChatUtility.PostRefundAsync(sup: true, trade_no, refund, refund, trade_no, "撤单");
                if (err != null) // not success
                {
                    dc.Rollback();
                    Err(err);
                }

                // put a notice to the shop
                var sta = GrabTwin<int, Org>(orgid);
                sta.NoticePack.Put(OrgNoticePack.PUR_VOID, 1, refund);
            }
        }
        catch (Exception)
        {
            dc.Rollback();
            Err("撤单失败, purid " + id);
            return;
        }

        wc.Give(204);
    }
}

public class CtrlyPurVarWork : PurVarWork
{
    public async Task mkt(WebContext wc)
    {
        int mktid = wc[0];
        var hub = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT localtimestamp(0); SELECT lotid, first(name), sum(CASE WHEN status = 1 THEN (qty / unitx) END), sum(CASE WHEN status = 2 THEN (qty / unitx) END) FROM purs WHERE hubid = @1 AND (status = 1 OR status = 2) AND mktid = @2 GROUP BY mktid, lotid");
        await dc.QueryTopAsync(p => p.Set(hub.id).Set(mktid));

        // the time stamp to fence the range to update
        dc.Let(out DateTime stamp);
        var intstamp = MainUtility.ToInt2020(stamp);

        wc.GivePage(200, h =>
        {
            h.TABLE_();
            h.THEAD_().TH("产品").TH("收单", css: "uk-width-tiny").TH("发货", css: "uk-width-tiny")._THEAD();

            dc.NextResult();
            while (dc.Next())
            {
                dc.Let(out int lotid);
                dc.Let(out string name);
                dc.Let(out int created);
                dc.Let(out int adapted);

                h.TR_();
                h.TD_().PICK(lotid).SP().T(name)._TD();
                h.TD_();
                if (created > 0)
                {
                    h.ADIALOG_(nameof(created), "?lotid=", lotid, mode: ToolAttribute.MOD_SHOW, false, css: "uk-link uk-button-link uk-flex-center").T(created)._A();
                }
                h._TD();
                h.TD_();
                if (adapted > 0)
                {
                    h.ADIALOG_(nameof(adapted), "?lotid=", lotid, mode: ToolAttribute.MOD_SHOW, false, css: "uk-link uk-button-link uk-flex-center").T(adapted)._A();
                }
                h._TD();
                h._TR();
            }

            h._TABLE();

            h.TOOLBAR(subscript: intstamp, bottom: true);
        }, false, 6);
    }

    public async Task created(WebContext wc)
    {
        int mktid = wc[0];
        int lotid = wc.Query[nameof(lotid)];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Pur.Empty).T(" FROM purs WHERE mktid = @1 AND status = 1 AND lotid = @2");
        var arr = await dc.QueryAsync<Pur>(p => p.Set(mktid).Set(lotid));

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
                h._LI();

                h._UL();
            });
        }, false, 6);
    }

    public async Task adapted(WebContext wc)
    {
        int mktid = wc[0];
        int lotid = wc.Query[nameof(lotid)];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Pur.Empty).T(" FROM purs WHERE mktid = @1 AND status = 2 AND lotid = @2");
        var arr = await dc.QueryAsync<Pur>(p => p.Set(mktid).Set(lotid));

        wc.GivePane(200, h =>
        {
            h.TABLE_();
            foreach (var o in arr)
            {
                var org = GrabTwin<int, Org>(o.orgid);

                h.TR_();
                h.TD(org.name);
                h.TD2(o.QtyX, "件", css: "uk-text-right");
                // h.TD_().NUMBER(null, nameof(o.ret), o.ret)._TD();

                h._TR();
            }
            h._TABLE();
        }, false, 6);
    }


    [Ui("发货", icon: "arrow-right", status: 255), Tool(ButtonOpen)]
    public async Task ok(WebContext wc)
    {
        var prin = (User)wc.Principal;
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

public class MktlyPurVarWork : PurVarWork
{
    [Ui("代收货", icon: "download"), Tool(ButtonPickConfirm)]
    public async Task sta(WebContext wc)
    {
        int orgid = wc[0];
        var prin = (User)wc.Principal;
        var org = wc[-2].As<Org>();

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT id, name, unitx, unit, (qty / unitx) FROM purs WHERE mktid = @1 AND status = 4 AND orgid = @2");
            await dc.QueryAsync(p => p.Set(org.id).Set(orgid));

            wc.GivePane(200, h =>
            {
                h.FORM_();
                h.TABLE_();
                while (dc.Next())
                {
                    dc.Let(out int id);
                    dc.Let(out string name);
                    dc.Let(out short unitx);
                    dc.Let(out string unit);
                    dc.Let(out int qtyx);

                    h.TR_();
                    h.TD_().PICK(id).SP().T(name).SMALL_().T('（').T(unitx).T(unit).T('）')._SMALL()._TD();
                    h.TD(qtyx);
                    // h.TD_().NUMBER(null, nameof(o.ret), o.ret)._TD();
                    h._TR();
                }
                h._TABLE();
                h._FORM();

                h.TOOLBAR(toggle: true, bottom: true);
            }, false, 6);
        }
        else // POST
        {
            int[] key = (await wc.ReadAsync<Form>())[nameof(key)];

            using var dc = NewDbContext();
            dc.Sql("UPDATE purs SET status = 8, ended = @1, ender = @2 WHERE mktid = @3 AND status = 4 AND orgid = @4 AND id")._IN_(key);
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(org.id).Set(orgid).SetForIn(key));

            wc.Give(204);
        }
    }
}