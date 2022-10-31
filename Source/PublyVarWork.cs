using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainMart
{
    public class PublyVarWork : WebWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyVarVarWork>();
        }

        public async Task @default(WebContext wc, int sec)
        {
            int mrtid = wc[0];
            var mrt = GrabObject<int, Org>(mrtid);
            var regs = Grab<short, Reg>();

            if (sec == 0) // when default sect
            {
                wc.Subscript = sec = regs.First(v => v.IsSection).id;
            }

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE (prtid = @1 OR id = @1) AND regid = @2 AND status > 0 ORDER BY addr");
            var arr = await dc.QueryAsync<Org>(p => p.Set(mrtid).Set(sec));

            wc.GivePage(200, h =>
            {
                h.NAVBAR(regs, string.Empty, sec, filter: (k, v) => v.IsSection);

                if (arr == null)
                {
                    h.ALERT("尚无商户");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");

                    if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(ChainMartApp.WwwUrl).T("/org/").T(o.id).T("/icon")._PIC();
                    }
                    else
                    {
                        h.PIC("/void.webp", css: "uk-width-1-5");
                    }

                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name);
                    h.P(o.tip);
                    h._DIV();

                    h._A();
                });
            }, true, 900, mrt.name);
        }
    }

    public class PublyVarVarWork : WebWork
    {
        public async Task @default(WebContext wc)
        {
            int bizid = wc[0];
            var biz = GrabObject<int, Org>(bizid);

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares WHERE shpid = @1 AND status > 0 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Ware>(p => p.Set(biz.id));

            wc.GivePage(200, h =>
            {
                h.FORM_().FIELDSUL_();
                if (arr == null)
                {
                    return;
                }

                foreach (var o in arr)
                {
                    h.LI_("uk-card uk-card-default");
                    h.HEADER_("uk-card-header").T(o.name)._HEADER();
                    h.DIV_();
                    h.SELECT(null, nameof(o.name), 1, new int[] {1, 2, 3});
                    h._DIV();
                    h._LI();
                }
                h._FIELDSUL();
                h.BOTTOMBAR_().BUTTON("付款")._BOTTOMBAR();
                h._FORM();
            }, true, 900, title: biz.name);
        }
    }


    public class PublyCtrVarWork : WebWork
    {
        /// <summary>
        /// To display territories marked by centers.
        /// </summary>
        public async Task @default(WebContext wc)
        {
            int ctrid = wc[0];
            var topOrgs = Grab<int, Org>();
            var ctr = topOrgs[ctrid];
            var cats = Grab<short, Cat>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE ctrid = @1 AND status > 0");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(ctrid));

            wc.GivePage(200, h =>
            {
                if (arr == null)
                {
                    h.ALERT("没有在批发的产品");
                    return;
                }

                h.NAVBAR(cats, "", 0);

                h.TABLE_();
                var last = 0;
                for (var i = 0; i < arr?.Length; i++)
                {
                    var o = arr[i];
                    // if (o.prvid != last)
                    // {
                    //     var spr = topOrgs[o.prvid];
                    //     h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
                    // }
                    h.TR_();
                    h.TD(o.name);
                    // h.TD(o.price, true);
                    h._TR();

                    // last = o.prvid;
                }
                h._TABLE();
            }, title: ctr.tip);
        }

        public async Task lot(WebContext wc, int lotid)
        {
            int prvid = wc[0];
            var topOrgs = Grab<int, Org>();
            var prv = topOrgs[prvid];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1 AND status > 0");
            var obj = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

            wc.GivePage(200, h =>
            {
                h.PIC("/prod/", obj.id, "/icon");
                h.SECTION_();
                h.T(obj.name);

                h._SECTION();

                h.BOTTOMBAR_().BUTTON("付款")._BOTTOMBAR();
            }, title: prv.tip);
        }

        [UserAuthorize]
        [Ui("付款"), Tool(Modal.ButtonPickScript)]
        public async Task book(WebContext wc, int cmd)
        {
            var prin = (User) wc.Principal;
            var orgs = Grab<int, Org>();

            // get pt and shop
            short ptid = wc[0];
            Org pt = null, shop = null;
            if (ptid > 0)
            {
                pt = orgs[ptid];
                // shop = pt.IsShop ? pt : orgs[pt.refid];
                if (shop == null)
                {
                    wc.Give(400, shared: true, maxage: 60);
                    return;
                }
            }

            if (wc.IsGet)
            {
                // get focus item info
                var q = wc.Query;
                int orderid = q[nameof(orderid)];
                int id = q[nameof(id)];
                short qty = q[nameof(qty)];

                using var dc = NewDbContext(IsolationLevel.ReadCommitted);

                // handling commands
                if (orderid > 0 && id > 0)
                {
                    if (qty == -1) // remove a line item
                    {
                        dc.Sql("DELETE FROM orderlns WHERE orderid = @1 AND id = @2 AND status = 0");
                        await dc.ExecuteAsync(p => p.Set(orderid).Set(id));
                    }
                    else // update qty
                    {
                        dc.Sql("UPDATE orderlns SET qty = @1 WHERE orderid = @2 AND id = @3 AND status = 0");
                        await dc.ExecuteAsync(p => p.Set(qty).Set(orderid).Set(id));
                    }
                }

                // get cart id if absent
                if (orderid <= 0)
                {
                    await dc.QueryTopAsync("SELECT id FROM orders WHERE uid = @1 AND status = 0 AND shopid = @2 AND ptid = @3 LIMIT 1", p => p.Set(prin.id).Set(shop.id).Set(ptid));
                    dc.Let(out orderid);
                }

                // display the lines
                wc.GivePane(200, h =>
                {
                    // output list 
                    h.ARTICLE_("uk-card uk-card-default");
                    decimal amt = 0;
                    h.UL_("uk-card-body");
                    h._UL();

                    h.FORM_("uk-card-footer", post: false);
                    h.SPAN_("uk-width-expand uk-text-small").H4(shop.name)._SPAN();
                    h.HIDDEN((nameof(orderid)), orderid);
                    h.SPAN_("uk-width-1-6 uk-flex-right").CNY(amt, true)._SPAN();
                    h._FORM();

                    h._ARTICLE();
                });
            }
            else // POST
            {
                if (cmd == 1) // clear order
                {
                    int orderid = (await wc.ReadAsync<Form>())[nameof(orderid)];
                    using (var dc = NewDbContext())
                    {
                        dc.Sql("DELETE FROM orderlns WHERE status = 0 AND orderid = @1");
                        await dc.ExecuteAsync(p => p.Set(orderid));
                    }
                    wc.GiveRedirect(nameof(book));
                }
                else if (cmd == 2) // to pay
                {
                    int orderid = wc.Query[nameof(orderid)]; // order id
                    var u = (User) wc.Principal;
                    using var dc = NewDbContext(IsolationLevel.ReadCommitted);
                    try
                    {
                        // update line status and compute total 
                        await dc.QueryTopAsync("SELECT sum((itemp + staplep + plusp) * qty) AS topay FROM orderlns WHERE status = 0 AND orderid = @1", p => p.Set(orderid));
                        dc.Let(out decimal topay);
                        // update order master record
                        await dc.ExecuteAsync("UPDATE orders SET topay = @1 WHERE id = @2", p => p.Set(topay).Set(orderid));
                        // call WeChatPay to prepare order there
                        string trade_no = (orderid + "-" + topay).Replace('.', '-');
                        var (prepay_id, _) = await WeixinUtility.PostUnifiedOrderAsync(
                            trade_no,
                            topay,
                            u.im, // the payer
                            wc.RemoteIpAddress.ToString(),
                            WeixinUtility.url + "/" + nameof(MgtService.onpay),
                            WeixinUtility.url
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
                        Application.Err(e.Message);
                        wc.Give(500);
                    }
                }
            }
        }
    }
}