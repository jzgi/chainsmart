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

                h.LI_().FIELD("消费者", o.uname)._LI();
                h.LI_().FIELD("商户", o.name)._LI();
                h.LI_().FIELD("简介", o.tip)._LI();
                h.LI_().FIELD("状态", o.status, Buy.Statuses)._LI();

                // details
                for (var i = 0; i < o.details?.Length; i++)
                {
                    var dtl = o.details[i];
                    //
                }

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
        [Ui("✎", "✎ 填写日志"), Tool(ButtonOpen)]
        public async Task log(WebContext wc, int dt)
        {
            int orderid = wc[0];
            if (wc.IsGet)
            {
            }
            else // POST
            {
                wc.GivePane(200); // close
            }
        }

        [Ui("标签", group: 1), Tool(ButtonOpen)]
        public async Task tag(WebContext wc)
        {
            int orderid = wc[0];
            var prin = (User) wc.Principal;
            if (wc.IsGet)
            {
                // list
                using var dc = NewDbContext();
                dc.Sql("SELECT tag FROM buys WHERE id = @1 AND uid = @2");
                var o = (string) await dc.ScalarAsync(p => p.Set(orderid).Set(prin.id));
                wc.GivePane(200, h =>
                {
                    h.FORM_();
                    h.FIELDSUL_("相较调养之前");
                    h.LI_().TEXT("肠胃排泄", nameof(o), o)._LI();
                    h._FIELDSUL();
                    h._FORM();
                });
            }
            else // POST
            {
                string tag = (await wc.ReadAsync<Form>())[nameof(tag)];
                using var dc = NewDbContext();
                dc.Sql("UPDATE orders SET tag = @1 WHERE id = @1 AND uid = @2");
                dc.Execute(p => p.Set(tag).Set(orderid).Set(prin.id));

                wc.GivePane(200); // close
            }
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
        [Ui("发货", "确认发货？", icon: "push"), Tool(ButtonConfirm, status: STU_CREATED)]
        public async Task snd(WebContext wc)
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


        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("撤单", "确认撤单并退款？", icon: "close"), Tool(ButtonConfirm, status: STU_CREATED)]
        public async Task refund(WebContext wc)
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
                    string err = await PostRefundAsync(sc: false, trade_no, refund, refund, trade_no);
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