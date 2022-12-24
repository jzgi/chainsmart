using System;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public abstract class BookVarWork : WebWork
    {
        public async Task @default(WebContext wc)
        {
            int id = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM books WHERE id = @1");
            var o = await dc.QueryTopAsync<Book>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品名", o.name)._LI();
                h.LI_().FIELD("简介", o.tip)._LI();
                h.LI_().FIELD("计价单位", o.unit).FIELD("每件含量", o.unitx, false)._LI();
                h.LI_().FIELD("单价", o.price, money: true).FIELD("立减", o.off)._LI();
                h.LI_().FIELD("件数", o.qty).FIELD("支付", o.pay, money: true)._LI();
                h.LI_().FIELD("状态", o.status, Lot.Statuses).FIELD("状况", Lot.States[o.state])._LI();
                h.LI_().FIELD("买方", o.shpname).LABEL("电话").SPAN_("uk-static").A_TEL(o.shptel, o.shptel)._SPAN()._LI();
                h.LI_().FIELD2("下单", o.created, o.creator, "&nbsp;")._LI();
                if (o.adapter != null) h.LI_().FIELD2("发货", o.adapted, o.adapter, "&nbsp;")._LI();
                if (o.oker != null) h.LI_().FIELD2("收货", o.oked, o.oker, "&nbsp;")._LI();
                h._UL();

                h.TOOLBAR(bottom: true, status: o.status, state: o.state);
            });
        }
    }

    public class SrclyBookVarWork : BookVarWork
    {
        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("发货", "确认发货？", icon: "push"), Tool(ButtonConfirm, status: STU_CREATED)]
        public async Task snd(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("UPDATE books SET adapted = @1, adapter = @2, status = 2 WHERE id = @3 AND srcid = @4 AND status = 1");
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

            wc.Give(204);
        }
    }


    public class ShplyBookVarWork : BookVarWork
    {
        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("收货", "确认收货？", icon: "pull"), Tool(ButtonShow, status: STU_ADAPTED)]
        public async Task rcv(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            if (wc.IsGet)
            {
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("UPDATE books SET oked = @1, oker = @2, status = 4 WHERE id = @3 AND shpid = @4 AND status = 1");
                await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

                wc.GivePane(200);
            }
        }
    }

    public class CtrlyBookVarWork : BookVarWork
    {
    }


    public class MktlyBookVarWork : BookVarWork
    {
    }
}