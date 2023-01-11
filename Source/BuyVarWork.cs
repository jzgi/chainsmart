using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainMart.WeixinUtility;
using static ChainFx.Application;
using static ChainFx.Entity;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
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

                h.LI_().LABEL("买方").DIV_("uk-static").SPAN_().T(o.uname).SP().A_TEL(o.utel, o.utel)._SPAN().BR().SPAN(o.uaddr)._DIV()._LI();
                h.LI_().FIELD("卖方", o.name)._LI();
                h.LI_().FIELD("简介", o.tip)._LI();
                h.LI_().LABEL("商品明细").TABLE(o.details, d => h.TD(d.name).TD2(d.qty, "件").TD(d.SubTotal, true))._LI();
                h.LI_().FIELD("应付金额", o.topay, true).FIELD("实付金额", o.pay, true)._LI();
                h.LI_().FIELD("状态", o.status, Buy.Statuses)._LI();

                if (o.creator != null) h.LI_().FIELD2("下单", o.created, o.creator)._LI();
                if (o.adapter != null) h.LI_().FIELD2(o.status == STU_ABORTED ? "撤单" : "发货", o.adapted, o.adapter)._LI();
                if (o.oker != null) h.LI_().FIELD2("收货", o.oked, o.oker)._LI();
                h._UL();

                h.TOOLBAR(bottom: true, status: o.status, state: o.state);
            });
        }
    }

    public class MyBuyVarWork : BuyVarWork
    {
        [Ui("收货", "确认收货？", icon: "sign-in"), Tool(ButtonConfirm, status: STU_ADAPTED)]
        public async Task ok(WebContext wc)
        {
            int id = wc[0];
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("UPDATE buys SET oked = @1, oker = @2, status = 4 WHERE id = @3 AND uid = @4 AND status = 2 RETURNING shpid, pay");
            if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(prin.id)))
            {
                dc.Let(out int shpid);
                dc.Let(out decimal pay);

                NoticeBox.Put(shpid, Notice.BUY_OKED, 1, pay);
            }

            wc.Give(200);
        }


        [Ui("申诉", group: 2), Tool(ButtonOpen)]
        public async Task compl(WebContext wc)
        {
            int orderid = wc[0];
            var prin = (User) wc.Principal;
            short appeal;
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                await dc.QueryTopAsync("SELECT compl FROM orders WHERE id = @1 AND uid = @2", p => p.Set(orderid).Set(prin.id));
                dc.Let(out appeal);
                wc.GivePane(200, h => { });
            }
            else
            {
                appeal = (await wc.ReadAsync<Form>())[nameof(appeal)];
                using var dc = NewDbContext();
                dc.Sql("UPDATE orders SET appeal = @1 WHERE id = @2 AND uid = @3");
                dc.Execute(p => p.Set(appeal).Set(orderid).Set(prin.id));
                wc.GivePane(200); // close
            }
        }
    }

    public class ShplyBuyVarWork : BuyVarWork
    {
        [OrglyAuthorize(0, User.ROL_LOG)]
        [Ui("发货", "确认发货？", icon: "sign-out"), Tool(ButtonConfirm, status: STU_CREATED)]
        public async Task adapt(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("UPDATE buys SET adapted = @1, adapter = @2, status = 2 WHERE id = @3 AND shpid = @4 AND status = 1 RETURNING uim, pay");
            if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)))
            {
                dc.Let(out string uim);
                dc.Let(out decimal pay);

                await PostSendAsync(uim, "您的订单已经发货，请留意接收（" + org.name + "，单号 " + id.ToString("D8") + "，￥" + pay + "）");
            }

            wc.Give(204);
        }

        [OrglyAuthorize(0, User.ROL_LOG)]
        [Ui("收货", "确认收货？", icon: "sign-in"), Tool(ButtonConfirm, status: STU_ADAPTED)]
        public async Task ok(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("UPDATE buys SET oked = @1, oker = @2, status = 4 WHERE id = @3 AND shpid = @4 AND status = 2 RETURNING uim, pay");
            if (await dc.QueryTopAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)))
            {
                dc.Let(out string uim);
                dc.Let(out decimal pay);

                await PostSendAsync(uim, prin.name + "已替您做了收货操作（" + org.name + "，单号 " + id.ToString("D8") + "，￥" + pay + "）");
            }

            wc.Give(204);
        }


        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("撤单", "确认撤单并退款？", icon: "trash"), Tool(ButtonConfirm, status: STU_CREATED)]
        public async Task abort(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext(IsolationLevel.ReadCommitted);
            try
            {
                dc.Sql("UPDATE buys SET refund = pay, status = 8, adapted = @1, adapter = @2 WHERE id = @3 AND shpid = @4 AND status = 1 RETURNING uim, topay, refund");
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
    }
}