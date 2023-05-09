using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainSmart.WeixinUtility;
using static ChainFx.Application;
using static ChainFx.Entity;
using static ChainFx.Nodal.Nodality;

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
            h.UL_("uk-list uk-list-divider");


            if (o.IsPlat)
            {
                h.LI_().LABEL("买方").DIV_("uk-static").SPAN_().T(o.uname).SP().A_TEL(o.utel, o.utel)._SPAN().BR().T(string.IsNullOrEmpty(o.ucom) ? "非派送区" : o.ucom).T('-').T(o.uaddr)._DIV()._LI();
                h.LI_().FIELD("卖方", o.name)._LI();
            }
            h.LI_().FIELD("应付金额", o.topay, true).FIELD("实付金额", o.pay, true)._LI();

            if (o.creator != null) h.LI_().FIELD2("下单", o.created, o.creator)._LI();
            if (o.adapter != null) h.LI_().FIELD2(o.IsVoid ? "撤单" : "备发", o.adapted, o.adapter)._LI();
            if (o.oker != null) h.LI_().FIELD2(o.IsVoid ? "撤销" : "发货", o.oked, o.oker)._LI();

            h._UL();

            h.TABLE(o.items, d =>
            {
                h.TD_().T(d.name);
                if (d.unitx != 1)
                {
                    h.SP().SMALL_().T(d.unitx).T(d.unit).T("件")._SMALL();
                }

                h._TD();
                h.TD_(css: "uk-text-right").CNY(d.RealPrice).SP().SUB(d.unit)._TD();
                h.TD2(d.qty, d.unit, css: "uk-text-right");
                h.TD(d.SubTotal, true, true);
            });

            h.TOOLBAR(bottom: true, status: o.Status, state: o.State);
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
        dc.Sql("UPDATE buys SET oked = @1, oker = @2, status = 4 WHERE id = @3 AND uid = @4 AND status = 2 RETURNING rtlid, pay");
        if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(prin.id)))
        {
            dc.Let(out int rtlid);
            dc.Let(out decimal pay);

            NoticeBot.Put(rtlid, Notice.BUY_OKED, 1, pay);
        }

        wc.Give(200);
    }
}

public class RtllyBuyVarWork : BuyVarWork
{
    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("备发", "已备货并集中等待发货？", icon: "eye"), Tool(ButtonConfirm, status: 1)]
    public async Task adapt(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE buys SET adapted = @1, adapter = @2, status = 2 WHERE id = @3 AND rtlid = @4 AND status = 1");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.Give(204);
    }

    [OrglyAuthorize(0, User.ROL_LOG)]
    [Ui("发货", "确认自行发货？", icon: "arrow-right"), Tool(ButtonConfirm, status: 3)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE buys SET oked = @1, oker = @2, status = 4 WHERE id = @3 AND rtlid = @4 AND status BETWEEN 1 AND 2 RETURNING uim, pay");
        if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)))
        {
            dc.Let(out string uim);
            dc.Let(out decimal pay);

            await PostSendAsync(uim, "您的订单已经发货，请留意接收（" + org.name + "，单号 " + id.ToString("D8") + "，￥" + pay + "）");
        }

        wc.Give(204);
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("撤销", "确认撤销并且退款？", icon: "trash"), Tool(ButtonConfirm, status: 7, state: Buy.STA_CANCELL)]
    public async Task @void(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext(IsolationLevel.ReadCommitted);
        try
        {
            dc.Sql("UPDATE buys SET refund = pay, status = 0, adapted = @1, adapter = @2 WHERE id = @3 AND rtlid = @4 AND status BETWEEN 1 AND 4 RETURNING uim, topay, refund");
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

                // notify user
                await PostSendAsync(uim, "您的订单已经撤销，请查收退款通知（" + org.name + "，单号 " + id.ToString("D8") + "，￥" + topay + "）");
            }
        }
        catch (Exception)
        {
            dc.Rollback();
            Err("退款失败，订单号：" + id);
            return;
        }

        wc.Give(204);
    }
}

public class MktlyBuyVarWork : BuyVarWork
{
    public new async Task @default(WebContext wc)
    {
        string com = wc[0];
        var org = wc[-2].As<Org>();

        const short msk = 255 | MSK_EXTRA;
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty, msk).T(" FROM buys WHERE rtlid = @1 AND ucom = @2 ORDER BY uid");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id).Set(com), msk);
    }
}