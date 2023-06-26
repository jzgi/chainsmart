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
            h.LI_().FIELD("简介", o.tip)._LI();
            h.LI_().FIELD("基准单位", o.unit).FIELD("每件含量", o.unitx)._LI();
            h.LI_().FIELD("基准单价", o.price, money: true).FIELD("件数", o.QtyX)._LI();
            h.LI_().FIELD("支付", o.pay, money: true).FIELD("状态", o.status, Pur.Statuses)._LI();

            if (o.creator != null) h.LI_().FIELD2("下单", o.created, o.creator)._LI();
            if (o.adapter != null) h.LI_().FIELD2(o.IsVoid ? "撤单" : "备发", o.adapted, o.adapter)._LI();
            if (o.oker != null) h.LI_().FIELD2("发货", o.oked, o.oker)._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.State);
        });
    }
}

public class RtllyPurVarWork : PurVarWork
{
}

public class SuplyPurVarWork : PurVarWork
{
    bool IsSpotTyp => (short)Parent.State == Pur.TYP_NORM;

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

            // put a notice to the relevant center
            var ctr = GrabTwin<int, Org>(ctrid);
            ctr.Notices.Put(OrgNoticePack.PUR_ADAPTED, 1, topay);
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
            dc.Sql("UPDATE purs SET status = 0, ret = qty, refund = pay, adapted = @1, adapter = @2 WHERE id = @3 AND supid = @4 AND status BETWEEN 1 AND 2 RETURNING lotid, qty, rtlid, topay, refund");
            if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)))
            {
                dc.Let(out int lotid);
                dc.Let(out int qty);
                dc.Let(out int rtlid);
                dc.Let(out decimal topay);
                dc.Let(out decimal refund);

                // adjust the stock
                dc.Sql("UPDATE lots SET avail = avail + @1 WHERE id = @2");
                await dc.ExecuteAsync(p => p.Set(qty).Set(lotid));

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
}

public class MktlyPurVarWork : PurVarWork
{
}