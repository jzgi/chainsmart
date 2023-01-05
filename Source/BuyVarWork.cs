using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainMart.WeixinUtility;
using static ChainFx.Application;
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
                h.LI_().FIELD("产品名", o.name)._LI();
                h.LI_().FIELD("简介", o.tip)._LI();
                h.LI_().FIELD("状态", o.status, Lot.Statuses).FIELD("状况", Lot.States[o.state])._LI();
                h.LI_().FIELD2("下单", o.created, o.creator, "&nbsp;")._LI();
                if (o.adapter != null) h.LI_().FIELD2("发货", o.adapted, o.adapter, "&nbsp;")._LI();
                if (o.oker != null) h.LI_().FIELD2("收货", o.oked, o.oker, "&nbsp;")._LI();
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

        [Ui("协议", group: 2), Tool(ButtonOpen)]
        public void agrmt(WebContext wc, int dietid)
        {
            var diets = Grab<short, Org>();
            var diet = diets[(short) dietid];
            wc.GivePane(200, h =>
            {
                h.FORM_();
                h._FORM();
            });
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
        [Ui("备好", icon: "bag"), Tool(ButtonConfirm)]
        public async Task ready(WebContext wc)
        {
        }


        [Ui("退款", icon: "close"), Tool(ButtonConfirm)]
        public async Task refund(WebContext wc)
        {
            short orgid = wc[-2];
            int orderid = wc[0];
            short level = 0;

            level = (await wc.ReadAsync<Form>())[nameof(level)];

            using var dc = NewDbContext(IsolationLevel.ReadCommitted);
            try
            {
                var percent = level * 0.10M;
                dc.Sql("UPDATE orders SET refund = pay * @1, status = CASE WHEN @1 = 1 THEN 2 ELSE status END WHERE id = @2 AND orgid = @3 AND status IN ) RETURNING refund");
                var refund = (decimal) await dc.ScalarAsync(p => p.Set(percent).Set(orderid).Set(orgid));
                if (refund <= 0)
                {
                    wc.Give(403); // forbidden
                    return;
                }

                // remote call weixin
                string orderno = orderid.ToString();
                string err = await PostRefundAsync(SC: false, orderno, refund, refund, orderno);
                if (err != null) // not success
                {
                    dc.Rollback();
                    Err(err);
                }
            }
            catch (Exception)
            {
                dc.Rollback();
                Err("退款失败: orderid = " + orderid);
                return;
            }
            wc.GivePane(200);
        }
    }

    public class MktlyBuyVarWork : BuyVarWork
    {
    }
}