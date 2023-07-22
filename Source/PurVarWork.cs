using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Application;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart;

public abstract class PurVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Pur.Empty).T(" FROM purs WHERE id = @1");
        var o = await dc.QueryTopAsync<Pur>(p => p.Set(id));

        var rtl = GrabTwin<int, Org>(o.rtlid);

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("单号", o.id, digits: 10)._LI();
            h.LI_().LABEL("买方").ADIALOG_(MainApp.WwwUrl + "/org/", o.rtlid, "/", ToolAttribute.MOD_SHOW, false).T(rtl.name)._A()._LI();
            h.LI_().LABEL("卖方").ADIALOG_(MainApp.WwwUrl + "/org/", o.supid, "/", ToolAttribute.MOD_SHOW, false).T(o.ctrid)._A()._LI();
            h.LI_().FIELD("产品名", o.name)._LI();
            h.LI_().FIELD("简介语", o.tip)._LI();
            h.LI_().FIELD("基准单位", o.unit).FIELD("每件含量", o.unitx)._LI();
            h.LI_().FIELD("基准单价", o.price, money: true).FIELD("件数", o.QtyX)._LI();
            h.LI_().FIELD("支付", o.pay, money: true).FIELD("状态", o.status, Pur.Statuses)._LI();

            if (o.creator != null) h.LI_().FIELD2("下单", o.created, o.creator)._LI();
            if (o.adapter != null) h.LI_().FIELD2(o.IsVoid ? "撤单" : "备发", o.adapted, o.adapter)._LI();
            if (o.oker != null) h.LI_().FIELD2("发货", o.oked, o.oker)._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        });
    }
}

public class RtllyPurVarWork : PurVarWork
{
}

public class SuplyPurVarWork : PurVarWork
{
    [OrglyAuthorize(0, User.ROL_LOG)]
    [Ui("备发", "授权品控库发货？", icon: "eye", status: 1), Tool(ButtonConfirm)]
    public async Task adapt(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE purs SET adapted = @1, adapter = @2, status = 2 WHERE id = @3 AND supid = @4 AND status = 1 RETURNING ctrid, topay");
        if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)))
        {
            dc.Let(out int ctrid);
            dc.Let(out decimal topay);
        }

        wc.Give(204);
    }

    [OrglyAuthorize(0, User.ROL_LOG)]
    [Ui("发货", "确认从品控库发货？", icon: "arrow-right", status: 2), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext(IsolationLevel.ReadUncommitted);
        try
        {
            // set status and decrease the stock
            dc.Sql("UPDATE purs SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND supid = @4 AND status = 2 RETURNING lotid, qty, rtlid, topay");
            if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)))
            {
                dc.Let(out int lotid);
                dc.Let(out int qty);
                dc.Let(out int rtlid);
                dc.Let(out decimal topay);

                // adjust the stock
                dc.Sql("UPDATE lots SET stock = stock - @1 WHERE id = @2");
                await dc.ExecuteAsync(p => p.Set(qty).Set(lotid));

                // put a notice to the shop
                var rtl = GrabTwin<int, Org>(rtlid);
                rtl.Notices.Put(OrgNoticePack.PUR_OKED, 1, topay);
            }
        }
        catch (Exception)
        {
            dc.Rollback();
            Err("退款失败, purid " + id);
            return;
        }

        wc.Give(204);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("撤单", "确认撤单并退款？", icon: "trash", status: 7), Tool(ButtonConfirm, state: Pur.STA_CANCELL)]
    public async Task @void(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext(IsolationLevel.ReadCommitted);
        try
        {
            dc.Sql("UPDATE purs SET status = 0, ret = qty, refund = pay, adapted = @1, adapter = @2 WHERE id = @3 AND supid = @4 AND status BETWEEN 1 AND 2 RETURNING lotid, hubid, qty, rtlid, topay, refund");
            if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)))
            {
                dc.Let(out int lotid);
                dc.Let(out int hubid);
                dc.Let(out int qty);
                dc.Let(out int rtlid);
                dc.Let(out decimal topay);
                dc.Let(out decimal refund);

                // adjust the stock
                dc.Sql("UPDATE lotinvs SET stock = stock + @1 WHERE lotid = @2 AND hubid = @3");
                await dc.ExecuteAsync(p => p.Set(qty).Set(lotid).Set(hubid));

                // remote call to refund
                var trade_no = Buy.GetOutTradeNo(id, topay);
                string err = await WeixinUtility.PostRefundAsync(sup: true, trade_no, refund, refund, trade_no);
                if (err != null) // not success
                {
                    dc.Rollback();
                    Err(err);
                }

                // put a notice to the shop
                var rtl = GrabTwin<int, Org>(rtlid);
                rtl.Notices.Put(OrgNoticePack.PUR_VOID, 1, refund);
            }
        }
        catch (Exception)
        {
            dc.Rollback();
            Err("退款失败, purid " + id);
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
        dc.Sql("SELECT localtimestamp(0); SELECT lotid, first(name), sum(CASE WHEN status = 2 THEN qty END), count(CASE WHEN status = 4 THEN qty END) FROM purs WHERE hubid = @1 AND (status = 2 OR status = 4) AND mktid = @2 GROUP BY mktid, lotid");
        await dc.QueryTopAsync(p => p.Set(hub.id).Set(mktid));

        // the time stamp to fence the range to update
        dc.Let(out DateTime stamp);
        var intstamp = MainUtility.ToInt2020(stamp);

        wc.GivePage(200, h =>
        {
            h.TABLE_();
            h.THEAD_().TH("产品").TH("备货", css: "uk-width-tiny").TH("发货", css: "uk-width-tiny")._THEAD();

            dc.NextResult();
            while (dc.Next())
            {
                dc.Let(out int lotid);
                dc.Let(out string name);
                dc.Let(out int adapted);
                dc.Let(out int oked);

                var mkt = GrabTwin<int, Org>(mktid);

                h.TR_();
                h.TD(name);
                h.TD_();
                if (adapted > 0)
                {
                    h.ADIALOG_(nameof(adapted), "?utel=", mode: ToolAttribute.MOD_SHOW, false, css: "uk-link uk-button-link uk-flex-center").T(adapted)._A();
                }
                h._TD();
                h.TD_();
                if (oked > 0)
                {
                    h.ADIALOG_(nameof(oked), "?utel=", mode: ToolAttribute.MOD_SHOW, false, css: "uk-link uk-button-link uk-flex-center").T(oked)._A();
                }
                h._TD();
                h.TD_();
                if (adapted > 0)
                {
                    h.PICK(lotid);
                }
                h._TD();
                h._TR();
            }

            h._TABLE();

            h.TOOLBAR(subscript: intstamp, bottom: true);
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
}