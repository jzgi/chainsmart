using System;
using System.Data;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Appear;
using static SkyChain.Web.Modal;
using static Zhnt.Mart.MartsUtility;
using static Zhnt.Mart.So;
using static Zhnt._Doc;
using static Zhnt.WeChatUtility;

namespace Zhnt.Mart
{
    public abstract class SoVarWork : WebWork
    {
    }

    public class MySoVarWork : SoVarWork
    {
        [Ui("✎", "✎ 填写日志"), Tool(ButtonShow)]
        public async Task log(WebContext wc, int dt)
        {
            int orderid = wc[0];
            if (wc.IsGet)
            {
                // list
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(OrderLg.Empty).T(" FROM orderlgs WHERE orderid = @1 AND dt = @2");
                var o = await dc.QueryTopAsync<OrderLg>(p => p.Set(orderid).Set(ToDateTime(dt)));
                wc.GivePane(200, h =>
                {
                    h.FORM_();
                    h.FIELDSUL_("相较调养之前的变化");
                    h.LI_().SELECT("肠胃排泄", nameof(o.digest), o.digest, OrderLg.Digests)._LI();
                    h.LI_().SELECT("睡眠气色", nameof(o.rest), o.rest, OrderLg.Rests)._LI();
                    h.LI_().SELECT("体重体型", nameof(o.fit), o.fit, OrderLg.Fits)._LI();
                    h.LI_().SELECT("血压血脂", nameof(o.blood), o.blood, OrderLg.Bloods)._LI();
                    h.LI_().SELECT("血糖指标", nameof(o.sugar), o.sugar, OrderLg.Sugars)._LI();
                    h.LI_().SELECT("饮食习惯", nameof(o.style), o.style, OrderLg.Styles)._LI();
                    h._FIELDSUL();
                    h.ALERT_(css: "uk-text-small").T("感谢您帮助营养师和人工智能精准掌握膳食调养规律！")._ALERT();
                    h._FORM();
                });
            }
            else // POST
            {
                const byte proj = OrderLg.LOG;
                var o = await wc.ReadObjectAsync<OrderLg>(proj);
                using var dc = NewDbContext();
                dc.Sql("UPDATE orderlgs")._SET_(OrderLg.Empty, proj).T(" WHERE orderid = @1 AND dt = @2");
                dc.Execute(p =>
                {
                    o.Write(p, proj);
                    p.Set(orderid).Set(ToDateTime(dt));
                });
                wc.GivePane(200); // close
            }
        }

        [Ui("标签", group: 1), Tool(ButtonShow)]
        public async Task tag(WebContext wc)
        {
            int orderid = wc[0];
            var prin = (User) wc.Principal;
            if (wc.IsGet)
            {
                // list
                using var dc = NewDbContext();
                dc.Sql("SELECT tag FROM orders WHERE id = @1 AND uid = @2");
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
            var diets = Fetch<Map<short, Biz>>();
            var diet = diets[(short) dietid];
            wc.GivePane(200, h =>
            {
                h.FORM_();
                h._FORM();
            });
        }

        [Ui("申诉", group: 2), Tool(ButtonShow)]
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
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("申诉类型");
                    for (int i = 0; i < Compls.Count; i++)
                    {
                        var e = Compls.EntryAt(i);
                        h.LI_().RADIO(nameof(appeal), e.Key, e.Value, @checked: e.Key == appeal)._LI();
                    }
                    h._FIELDSUL()._FORM();
                });
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

    public class OrglyBuyVarWork : SoVarWork
    {
        [Ui("☰", "☰ 明细"), Tool(ButtonOpen, Half)]
        public async Task dtl(WebContext wc)
        {
            short orgid = wc[-2];
            int orderid = wc[0];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(OrderLg.Empty).T(" FROM orderlgs WHERE orderid = @1 ORDER BY dt");
            var arr = await dc.QueryAsync<OrderLg>(p => p.Set(orderid));
            wc.GivePane(200, h =>
            {
                var today = DateTime.Today;
                h.TABLE(arr, o =>
                {
                    h.TD_().T(GetCircledDateString(o.dt, today))._TD();
                    h.TD_("uk-width-micro").T("&#10004;&#65039;", o.status > 0)._TD();
                });
                h.BOTTOM_BUTTON("退款", nameof(refund));
            });
        }

        [Ui("⥻", "⥻ 退款"), Tool(ButtonOpen, Small)]
        public async Task refund(WebContext wc)
        {
            short orgid = wc[-2];
            int orderid = wc[0];
            short level = 0;
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("请选择退款比例");
                    h.LI_().SELECT("退款比例", nameof(level), level, Levels);
                    h._FIELDSUL();
                    h.BOTTOM_BUTTON("退款", nameof(refund));
                    h._FORM();
                });
            }
            else // POST
            {
                level = (await wc.ReadAsync<Form>())[nameof(level)];

                using var dc = NewDbContext(IsolationLevel.ReadCommitted);
                try
                {
                    var percent = level * 0.10M;
                    dc.Sql("UPDATE orders SET refund = pay * @1, status = CASE WHEN @1 = 1 THEN ").T(STATUS_ABORTED).T(" ELSE status END WHERE id = @2 AND orgid = @3 AND status IN (").T(STATUS_ISSUED).T(", ").T(STATUS_ARGUED).T(") RETURNING refund");
                    var refund = (decimal) await dc.ScalarAsync(p => p.Set(percent).Set(orderid).Set(orgid));
                    if (refund <= 0)
                    {
                        wc.Give(403); // forbidden
                        return;
                    }

                    // remote call weixin
                    string orderno = orderid.ToString();
                    string err = await PostRefundAsync(orderno, refund, refund, orderno);
                    if (err != null) // not success
                    {
                        dc.Rollback();
                        ERR(err);
                    }
                }
                catch (Exception)
                {
                    dc.Rollback();
                    ERR("退款失败: orderid = " + orderid);
                    return;
                }
                wc.GivePane(200);
            }
        }
    }

    public class OrglyOrderVarWork : SoVarWork
    {
        [Ui("服务站"), Tool(ButtonOpen)]
        public async Task bypt(WebContext wc, int track)
        {
            short orgid = wc[-2];
            DateTime dt = wc[0];
            var orgs = Fetch<Map<short, Biz>>();
            using var dc = NewDbContext();
            dc.Sql("SELECT o.ptid, o.typ, count(o.id)  FROM orders o, orderlgs l WHERE l.orderid = o.id AND o.orgid = @1 AND o.status = ").T(STATUS_ISSUED).T(" AND l.dt = @2 AND l.track = @3 GROUP BY o.ptid, o.typ");
            await dc.QueryAsync(p => p.Set(orgid).Set(dt).Set(track));
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();
                short last = 0;
                while (dc.Next())
                {
                    dc.Let(out short ptid);
                    dc.Let(out short typ);
                    dc.Let(out short cnt);
                    var org = orgs[ptid];
                    if (ptid != last)
                    {
                        if (last != 0)
                        {
                            h._LI();
                        }
                    }
                    h.LI_("uk-flex uk-width-1-1");
                    h.P(ptid != last ? org.name : null, "uk-label uk-width-expand");
                    h.P(cnt, "uk-text-right uk-width-1-6");
                    h._LI();

                    last = ptid;
                }
                h._FIELDSUL()._FORM();
            });
        }

        [Ui("方案"), Tool(ButtonOpen)]
        public async Task bytyp(WebContext wc, int track)
        {
            short orgid = wc[-2];
            DateTime dt = wc[0];
            var orgs = Fetch<Map<short, Biz>>();
            using var dc = NewDbContext();
            dc.Sql("SELECT o.typ, o.ptid, count(o.id)  FROM orders o, orderlgs l WHERE l.orderid = o.id AND o.orgid = @1 AND o.status = ").T(STATUS_ISSUED).T(" AND l.dt = @2 AND l.track = @3 GROUP BY o.typ, o.ptid");
            await dc.QueryAsync(p => p.Set(orgid).Set(dt).Set(track));
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();
                short last = 0;
                while (dc.Next())
                {
                    dc.Let(out short typ);
                    dc.Let(out short ptid);
                    dc.Let(out short cnt);
                    var org = orgs[ptid];
                    if (typ != last)
                    {
                        if (last != 0)
                        {
                            h._LI();
                        }
                    }
                    h.LI_("uk-flex uk-width-1-1");
                    h.P(org.name, "uk-width-1-3");
                    h.P(cnt, "uk-text-right uk-width-1-6");
                    h._LI();

                    last = typ;
                }
                h._FIELDSUL()._FORM();
            });
        }

        [Ui("材料"), Tool(ButtonOpen)]
        public async Task mats(WebContext wc, int track)
        {
            var items = Fetch<Map<short, Item>>();

            Map<short, MatAgg> ComputeBom(short[] arr)
            {
                var map = new Map<short, MatAgg>(256);
                foreach (var ln in arr)
                {
                    var item = items[ln];
                    Add(item, 1);

                    // opt if any
                    // if (ln.qty > 0)
                    // {
                    //     var opt = items[ln.optid];
                    //     Add(opt, ln.qty);
                    // }
                }

                return map;

                void Add(Item it, int qty)
                {
                    var ings = it.ingrs;
                    if (ings == null) return;
                    foreach (var ing in ings)
                    {
                        var matid = ing.id;
                        var prep = map[matid];
                        if (prep == null)
                        {
                            map.Add(new MatAgg(matid, qty * ing.qty));
                        }
                        else
                        {
                            prep.Add(qty * ing.qty);
                        }
                    }
                }
            }
        }
    }

    public class OrglySoDistVarWork : SoVarWork
    {
        [Ui("明细"), Tool(ButtonOpen)]
        public async Task by(WebContext wc, int cmd)
        {
            short track = (short) (cmd % 10);
            int subcmd = cmd / 10;
            short ptid = wc[-2];
            DateTime dt = wc[0];
            string[] key = null;
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT o.id, o.typ, o.uname, o.utel, o.uim, l.track, l.status FROM orderlgs l, orders o WHERE l.orderid = o.id AND o.ptid = @1 AND o.status = ").T(STATUS_ISSUED).T(" AND l.dt = @2 AND l.track = @3");
                await dc.QueryAsync(p => p.Set(ptid).Set(dt).Set(track));
                wc.GivePane(200, h =>
                {
                    h.FORM_();
                    h.TABLE_();
                    while (dc.Next())
                    {
                        h.TR_();

                        dc.Let(out int orderid);
                        dc.Let(out short typ);
                        dc.Let(out string uname);
                        dc.Let(out string utel);
                        dc.Let(out string uim);
                        dc.Let(out short state);
                        dc.Let(out short status);

                        h.TDCHECK(uim, toolbar: false);
                        h.TD_().A_TEL(uname, utel)._LI();
                        h.TD_("uk-width-micro").T("&#10004;&#65039;", status > 0)._TD();
                        h._TR();
                    }
                    h._TABLE();
                    h.BOTTOMBAR_("uk-flex uk-flex-center uk-button-group", true);
                    h.BUTTON("通知", nameof(@by), 1);
                    h.BUTTON("标记", nameof(@by), 2);
                    h._BOTTOMBAR();
                    h._FORM();
                });
            }
            else // POST
            {
                var orgs = Fetch<Map<short, Biz>>();
                var org = orgs[ptid];
                key = (await wc.ReadAsync<Form>())[nameof(key)];
                if (subcmd == 1)
                {
                    foreach (var uim in key)
                    {
                        await PostSendAsync(uim, "【取餐通知】您的调养餐已到达" + org.Name + "服务站（联系电话 <a href=\"tel:" + org.ctttel + "\">" + org.ctttel + "</a>），请及时领取");
                    }
                }
                else
                {
                }
                wc.GivePane(200);
            }
        }
    }
}