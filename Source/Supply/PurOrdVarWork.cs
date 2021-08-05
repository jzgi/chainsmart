using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static System.Data.IsolationLevel;
using static System.String;
using static SkyChain.Web.Modal;
using static SkyChain.Web.ToolAttribute;

namespace Zhnt.Supply
{
    public abstract class LotVarWork : WebWork
    {
        public abstract Task act(WebContext wc, int cmd);

        const int PIC_MAX_AGE = 3600 * 24;

        public void icon(WebContext wc, int forced = 0)
        {
            short id = wc[this];
            using var dc = NewDbContext();
            if (dc.QueryTop("SELECT icon FROM lots WHERE id = @1", p => p.Set(id)))
            {
                dc.Let(out byte[] bytes);
                if (bytes == null) wc.Give(204); // no content 
                else wc.Give(200, new StaticContent(bytes), shared: (forced == 0) ? true : (bool?) null, PIC_MAX_AGE);
            }
            else wc.Give(404, shared: true, maxage: 3600 * 24); // not found
        }

        public void img(WebContext wc, int forced = 0)
        {
            short id = wc[this];
            using var dc = NewDbContext();
            if (dc.QueryTop("SELECT img FROM lots WHERE id = @1", p => p.Set(id)))
            {
                dc.Let(out byte[] bytes);
                if (bytes == null) wc.Give(204); // no content 
                else wc.Give(200, new StaticContent(bytes), shared: (forced == 0) ? true : (bool?) null, PIC_MAX_AGE);
            }
            else wc.Give(404, shared: true, maxage: 3600 * 24); // not found
        }
    }

    public class PubLotVarWork : LotVarWork
    {
        [UserAuthorize]
        [Ui(group: 1), Tool(ButtonOpen, Appear.Refresh)]
        public override async Task act(WebContext wc, int cmd)
        {
            var regs = Fetch<Map<short, Reg>>();
            var orgs = Fetch<Map<short, Org>>();
            int lotid = wc[0];
            var prin = (User) wc.Principal;
            short qty;
            string addr = null;
            short ptid;
            if (wc.IsGet)
            {
                ptid = wc.Query[nameof(ptid)];
                if (cmd == 0) // pick a pt
                {
                    ptid = wc.Cookies[nameof(ptid)].ToShort();
                    short regid = wc.Query[nameof(regid)];
                    if (regid == 0)
                    {
                        regid = wc.Cookies[nameof(regid)].ToShort();
                        if (regid == 0)
                        {
                            regid = regs.KeyAt(0);
                        }
                    }
                    else
                    {
                        wc.SetCookie(nameof(regid), regid.ToString(), maxage: 3600 * 300);
                    }
                    wc.GivePane(200, h =>
                    {
                        h.FORM_();
                        if (ptid > 0)
                        {
                            var o = orgs[ptid];
                            h.FIELDSUL_("您默认的服务站").LI_();
                            h.SPAN_("uk-width-expand").RADIO(nameof(ptid), o.id, o.name, true, tip: o.addr, required: true)._SPAN();
                            h.SPAN_("uk-badge").A_POI(o.x, o.y, o.name, o.addr)._SPAN();
                            h._LI()._FIELDSUL();
                        }
                        h.FIELDSUL_("请选择就近的服务站");
                        h.LI_().SELECT("切换城市", nameof(regid), regid, regs, refresh: true)._LI();
                        bool exist = false;
                        for (int i = 0; i < orgs.Count; i++)
                        {
                            var o = orgs.ValueAt(i);
                            if (o.IsPt && o.regid == regid)
                            {
                                exist = true;
                                h.LI_("uk-flex");
                                h.SPAN_("uk-width-expand").RADIO(nameof(ptid), o.id, o.name, false, tip: o.addr, required: true).SP().SP().A_TEL("☏", o.Tel, css: "uk-small")._SPAN();
                                // h.A_TEL_ICON(o.name, o.Tel);
                                h.SPAN_("uk-margin-auto-left");
                                if (o.x > 0 && o.y > 0)
                                {
                                    h.A_POI(o.x, o.y, o.name, o.addr, o.Tel);
                                }
                                else
                                {
                                    h.T("<span class=\"uk-icon-link uk-inactive\" uk-icon=\"location\"></span>");
                                }
                                h._SPAN();
                                h._LI();
                            }
                        }
                        if (!exist)
                        {
                            h.LI_().T("（暂无服务站）")._LI();
                        }
                        h._FIELDSUL();
                        h.BOTTOMBAR_().BUTTON("下一步", nameof(act), subscript: 1, post: false)._BOTTOMBAR();
                        h._FORM();
                    }, false, 15);
                }
                else // acting form fill-out
                {
                    using var dc = NewDbContext();

                    dc.Sql("SELECT ").collst(Purchase.Empty).T(" FROM lots_vw WHERE id = @1");
                    var m = await dc.QueryTopAsync<Purchase>(p => p.Set(lotid));
                    var descr = LotDescr.All[m.typ];

                    dc.Sql("SELECT ").collst(LotJn.Empty).T(" FROM lotjns WHERE lotid = @1 AND uid = @2");
                    var d = await dc.QueryTopAsync<LotJn>(p => p.Set(lotid).Set(prin.id));

                    wc.GivePane(200, h =>
                    {
                        h.ViewLotTop(m, nameof(icon), nameof(img));

                        if (d?.status > 0)
                        {
                            h.ALERT_().SPAN_().T("您先前已经").T(descr.act).T("；请参看「").A_GOTO("我的拼团", href: "/my//lot/").T("」")._SPAN()._ALERT();
                        }
                        else
                        {
                            h.FORM_(oninput: descr.IsPayBefore ? "out.value = (parseFloat(price.value) * parseInt(qty.value)).toFixed(2)" : null).FIELDSUL_();
                            h.LI_().LABEL("价格").SPAN_("uk-input");
                            if (m.price > 0)
                            {
                                h.CNY(m.price);
                            }
                            else
                            {
                                h.T("免费");
                            }
                            h._SPAN()._LI();

                            h.LI_().SELECT_("数量（" + m.unit + "）", nameof(qty));
                            int i = m.least;
                            int c = 0;
                            while (i <= m.max - m.qtys)
                            {
                                h.OPTION_(i).T(i)._OPTION();
                                if (c >= 30)
                                {
                                    i += m.step * 5;
                                }
                                else
                                {
                                    i += m.step;
                                }
                                c++;
                            }
                            h._SELECT()._LI();
                            if (ptid <= 0 && IsNullOrEmpty(m.addr))
                            {
                                h.LI_().TEXTAREA(descr.addrlbl, nameof(addr), addr, tip: "省市／区县／详址", min: 4, max: 30, required: true)._LI();
                            }
                            h._FIELDSUL();
                            h.HIDDEN(nameof(m.price), m.price);
                            h.HIDDEN(nameof(ptid), ptid);
                            var amt = descr.IsPayBefore ? m.price * m.least : 0.00M;
                            h.BOTTOMBAR_().TOOL(nameof(act), 3, caption: descr.act + " ￥<output name=\"out\">" + amt + "</output>", tool: BUTTON_PICK_SCRIPT, css: "uk-button-danger")._BOTTOMBAR();
                            h._FORM();
                        }
                    });
                }
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();

                using var dc = NewDbContext(ReadCommitted);

                dc.Sql("SELECT ").collst(Purchase.Empty).T(" FROM lots_vw WHERE id = @1 AND status = ");
                var m = await dc.QueryTopAsync<Purchase>(p => p.Set(lotid));
                if (m == null)
                {
                    dc.Rollback();
                    wc.Give(500);
                    return;
                }
                var descr = LotDescr.All[m.typ];

                var d = new LotJn
                {
                    lotid = lotid,
                    uid = prin.id,
                    uname = prin.name,
                    utel = prin.tel,
                    uim = prin.im,
                    uacct = prin.acct,
                    qty = f[nameof(qty)],
                    ptid = f[nameof(ptid)],
                    uaddr = f[nameof(addr)],
                    inited = DateTime.Now,
                };

                dc.Sql("INSERT INTO lotjns ").colset(d, 0)._VALUES_(d, 0).T(" ON CONFLICT (lotid, uid) DO UPDATE SET uaddr = @uaddr, ptid = @ptid, qty = @qty, inited = @inited");
                await dc.ExecuteAsync(p => d.Write(p));

                if (descr.IsPayBefore) // invoke payment and let the callback confirm the lotjn
                {
                    // call WeChatPay to prepare order there
                    var topay = m.price * d.qty;
                    var (prepay_id, _) = await WeChatUtility.PostUnifiedOrderAsync(
                        d.GetTradeNo,
                        topay,
                        prin.im, // the payer
                        wc.RemoteAddr.ToString(),
                        WeChatUtility.url + "/" + nameof(ZhntService.onlotjn),
                        SupplyUtility.LOTS + " - " + m.name
                    );
                    if (prepay_id != null)
                    {
                        wc.Give(200, WeChatUtility.BuildPrepayContent(prepay_id));
                    }
                    else
                    {
                        dc.Rollback();
                        wc.Give(500);
                    }
                }
                else // confirm lotjn directly
                {
                    if (!await dc.AddLotJnAsync(lotid, prin.id, 0.00M, orgs))
                    {
                        dc.Rollback();
                        wc.Give(500);
                    }
                    else
                    {
                        wc.Give(201);
                    }
                }
            }
        }
    }

    public class MyLotVarWork : LotVarWork
    {
        [Ui(group: 1), Tool(ButtonOpen)]
        public override async Task act(WebContext wc, int cmd)
        {
            int lotid = wc[0];
            var prin = (User) wc.Principal;

            if (wc.IsGet)
            {
                using var dc = NewDbContext();

                dc.Sql("SELECT ").collst(Purchase.Empty).T(" FROM lots_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Purchase>(p => p.Set(lotid));
                var descr = LotDescr.All[m.typ];

                dc.Sql("SELECT ").collst(LotJn.Empty).T(" FROM lotjns WHERE lotid = @1 AND uid = @2");
                var d = await dc.QueryTopAsync<LotJn>(p => p.Set(lotid).Set(prin.id));

                wc.GivePane(200, h =>
                {
                    h.ViewLotTop(m, nameof(icon), nameof(img));

                    h.FORM_().FIELDSUL_();
                    if (m.price > 0.00M)
                    {
                        h.LI_().FIELD("价格", m.price, currency: true)._LI();
                    }
                    h.LI_().FIELD2("数量", d.qty, m.unit)._LI();
                    if (d.pay > 0.00M)
                    {
                        h.LI_().FIELD("已预付款", d.pay, currency: true)._LI();
                    }
                    if (d.ptid > 0)
                    {
                        var orgs = Fetch<Map<short, Org>>();
                        h.LI_().FIELD("服务站", orgs[d.ptid].name)._LI();
                    }
                    else if (!IsNullOrEmpty(d.uaddr))
                    {
                        h.LI_().FIELD(descr.addrlbl, d.uaddr)._LI();
                    }
                    h._FIELDSUL();
                    h.BOTTOM_BUTTON("撤销" + descr.act, nameof(act), subscript: 1, disabled: m.IsLocked());
                    h._FORM();
                }, false, 6);
            }
            else // POST
            {
                if (cmd == 1)
                {
                    using var dc = NewDbContext(ReadCommitted);
                    try
                    {
                        var err = await dc.RemoveLotJnAsync(lotid, prin.id, "主动撤销");
                        if (err != null)
                        {
                            dc.Rollback();
                        }
                    }
                    catch
                    {
                        dc.Rollback();
                    }
                }
                wc.GivePane(200);
            }
        }
    }


    public class OrglyLotVarWork : LotVarWork
    {
        [Ui, Tool(ButtonOpen)]
        public override async Task act(WebContext wc, int cmd)
        {
            int lotid = wc[0];
            short orgid = wc[-2];
            if (wc.IsGet)
            {
                var orgs = Fetch<Map<short, Org>>();

                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Purchase.Empty).T(" FROM lots_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Purchase>(p => p.Set(lotid));
                var descr = LotDescr.All[m.typ];

                dc.Sql("SELECT ").collst(LotJn.Empty).T(" FROM lotjns WHERE lotid = @1");
                var arr = await dc.QueryAsync<LotJn>(p => p.Set(lotid));
                wc.GivePane(200, h =>
                {
                    if (arr == null)
                    {
                        h.ALERT("暂无" + descr.act);
                        return;
                    }
                    h.FORM_();

                    h.TABLE(arr, d =>
                    {
                        h.TDCHECK(d.uid, toolbar: false);
                        h.TD_("uk-width-expand").P_().T(d.uname).T('（').A_TEL(d.utel, d.utel).T('）').T(d.qty).SP().T(m.unit)._P();
                        h.P_("uk-text-small");
                        if (d.ptid > 0)
                        {
                            h.T(orgs[d.ptid].name);
                        }
                        h.T(d.uaddr);
                        h._P();
                        h._TD();
                    });

                    h.BOTTOMBAR_("uk-flex uk-flex-center uk-button-group", true);
                    h.BUTTON("撤销", nameof(act), 1);
                    h.BUTTON("通知", nameof(act), 2);
                    h._BOTTOMBAR();

                    h._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                int[] key = f[nameof(key)];
                if (cmd == 1)
                {
                    using var dc = NewDbContext(ReadCommitted);
                    try
                    {
                        foreach (int uid in key)
                        {
                            await dc.RemoveLotJnAsync(lotid, uid, "商家撤销");
                        }
                    }
                    catch (Exception e)
                    {
                        ERR(e.Message);
                        dc.Rollback();
                    }
                }
                else
                {
                }

                wc.GiveRedirect(nameof(act));
            }
        }

        [Ui("修改", group: 1), Tool(ButtonOpen)]
        public async Task upd(WebContext wc)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-2];
            var org = Fetch<Map<short, Org>>()[orgid];
            int id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Purchase.Empty).T(" FROM lots_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Purchase>(p => p.Set(id));
                var descr = LotDescr.All[m.typ];

                dc.Sql("SELECT sum(qty) FROM lotjns WHERE lotid = @1 AND status = ");
                await dc.QueryTopAsync<Purchase>(p => p.Set(id));
                dc.Let(out m.qtys);

                wc.GivePane(200, h =>
                {
                    if (m.qtys > 0)
                    {
                        h.ALERT_().T("已经有").T(descr.act).T("，不能修改")._ALERT();
                    }
                    else
                    {
                        h.FORM_();
                        h.HIDDEN(nameof(m.typ), m.typ);
                        h.FIELDSUL_(descr.name);
                        descr.FormView(h, descr, m, org);
                        h.BOTTOMBAR_().BUTTON("发布", nameof(@upd))._BOTTOMBAR();
                        h._FORM();
                    }
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                short typ = f[nameof(typ)];
                var m = new Purchase
                {
                    orgid = orgid,
                    typ = typ,
                    status = _Doc.STATUS_DRAFT,
                    author = prin.name
                };
                m.Read(f);
                using var dc = NewDbContext();
                dc.Sql("UPDATE lots ")._SET_(m, 0).T(" WHERE id = @1");
                await dc.ExecuteAsync(p =>
                {
                    m.Write(p, 0);
                    p.Set(id);
                });

                wc.GivePane(201);
            }
        }

        [Ui("图标", group: 1), Tool(ButtonCrop, Appear.Small)]
        public async Task icon(WebContext wc)
        {
            int id = wc[0];
            if (wc.IsGet)
            {
                base.icon(wc);
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ArraySegment<byte> img = f[nameof(img)];
                using var dc = NewDbContext();
                if (await dc.ExecuteAsync("UPDATE lots SET icon = @1 WHERE id = @2", p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else wc.Give(500); // internal server error
            }
        }

        [Ui("图片", group: 1), Tool(ButtonCrop, Appear.Small)]
        public async Task img(WebContext wc)
        {
            int id = wc[0];
            if (wc.IsGet)
            {
                base.img(wc);
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ArraySegment<byte> img = f[nameof(img)];
                using var dc = NewDbContext();
                if (await dc.ExecuteAsync("UPDATE lots SET img = @1 WHERE id = @2", p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else wc.Give(500); // internal server error
            }
        }

        // [Ui("核实"), Tool(Modal.ButtonShow)]
        public async Task apprv(WebContext wc)
        {
            short orgid = wc[-2];
            var orgs = Fetch<Map<short, Org>>();
            var org = orgs[orgid];
            long job = wc[0];
            bool ok;
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("核实申请");
                    h.LI_().CHECKBOX(null, nameof(ok), true, tip: "我确定此项申请情况属实，同意奖励数字珍珠", required: true)._LI();
                    h._FORM()._FIELDSUL();
                });
            }
            else
            {
                ok = (await wc.ReadAsync<Form>())[nameof(ok)];
                if (ok)
                {
                    using var dc = NewDbContext(ReadCommitted);

                    // var tx = new ChainTransaction(1)
                        // .Row()

                    // await dc.ExecuteAsync("", org.Acct);
                }
                wc.GivePane(200);
            }
        }
    }

    public class OrglyLotDistVarWork : LotVarWork
    {
        [Ui(group: 1), Tool(ButtonOpen)]
        public override async Task act(WebContext wc, int cmd)
        {
        }
    }
}