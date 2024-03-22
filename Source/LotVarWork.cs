using System;
using System.Data;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

namespace ChainSmart;

public abstract class LotVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        const short msk = 255 | MSK_AUX;

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Flow.Empty, msk).T(" FROM lotops WHERE id = @1 AND orgid = @2");
        var o = await dc.QueryTopAsync<Flow>(p => p.Set(id).Set(org.id), msk);

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("消息标题", o.name)._LI();
            // h.LI_().FIELD("内容", o.content)._LI();
            h.LI_().FIELD("注解", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
            h.LI_().FIELD("状态", o.status, Flow.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "发布", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }
}

public class SuplyLotVarWork : LotVarWork
{
    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改或调整消息", icon: "pencil", status: 1 | 2 | 4), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Flow.Empty).T(" FROM lotops WHERE id = @1");
            var o = await dc.QueryTopAsync<Flow>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("消息类型", nameof(o.typ), o.typ, Flow.Typs)._LI();
                h.LI_().TEXT("标题", nameof(o.name), o.name, max: 12)._LI();
                // h.LI_().TEXTAREA("内容", nameof(o.content), o.content, max: 300)._LI();
                h.LI_().TEXTAREA("注解", nameof(o.tip), o.tip, max: 40)._LI();
                // h.LI_().SELECT("级别", nameof(o.rank), o.rank, Lotop.Ranks)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            var m = await wc.ReadObjectAsync(msk, new Flow
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE lotops ")._SET_(Flow.Empty, msk).T(" WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(id).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }


    [MgtAuthorize(0, User.ROL_MGT)]
    [Ui("发布", "安排发布", status: 1 | 2 | 4), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE lotops SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4 RETURNING ").collst(Flow.Empty);
        var o = await dc.QueryTopAsync<Flow>(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        // org.EventPack.AddMsg(o);

        wc.GivePane(200);
    }
}

public class HublyLotVarWork : LotVarWork
{
}


public class RtllyPurLotVarWork : LotVarWork
{
    //
    // NOTE: this page is made publicly cacheable, though under variable path
    //
    public  async Task @default(WebContext wc, int hubid)
    {
        int lotid = wc[0];


        const short Msk = 0xff | MSK_EXTRA;
        Item o;
        if (hubid > 0)
        {
            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Item.Empty, alias: "o").T(", d.stock FROM lots_vw o, lotinvs d WHERE o.id = d.lotid AND o.id = @1");
            o = await dc.QueryTopAsync<Item>(p => p.Set(lotid), Msk);
        }
        else
        {
            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Item.Empty, Msk).T(" FROM lots_vw WHERE id = @1");
            o = await dc.QueryTopAsync<Item>(p => p.Set(lotid), Msk);
        }

        Org src = null;
        if (o.srcid > 0)
        {
            src = GrabTwin<int, Org>(o.srcid);
        }

        wc.GivePane(200, h =>
        {
            // h.ShowLot(o, src, false, false);

            // bottom bar
            //
            var fee = BankUtility.supfee;

            var realprice = o.RealPrice;
            int qtyx = 1;
            short unitx = o.unitx;
            int qty = qtyx * unitx;
            decimal topay = qty * o.RealPrice + fee;

            h.BOTTOMBAR_();
            h.FORM_("uk-flex uk-flex-middle uk-width-1-1 uk-height-1-1", oninput: $"qty.value = (qtyx.value * {unitx}).toFixed(); topay.value = ({realprice} * qty.value + (fee ? parseFloat(fee.value) : 0) ).toFixed(2);");

            h.HIDDEN(nameof(realprice), realprice);

            h.SELECT_(null, nameof(qtyx), css: "uk-width-small");
            // for (int i = 1; i <= Math.Min(o.max, o.StockX); i += (i >= 120 ? 5 : i >= 60 ? 2 : 1))
            // {
            //     h.OPTION_(i).T(i).SP().T('件')._OPTION();
            // }
            h._SELECT().SP();
            h.SPAN_("uk-width-expand uk-padding").T("共").SP().OUTPUT(nameof(qty), qty).SP().T(o.unit);
            // if (o.IsOnHub)
            // {
            //     h.H6_("uk-margin-auto-left").T("到市场运费 +").OUTPUT(nameof(fee), fee)._H6();
            // }
            h._SPAN();

            // pay button
            h.BUTTON_(nameof(pur), onclick: "return $pur(this);", css: "uk-button-danger uk-width-medium uk-height-1-1").CNYOUTPUT(nameof(topay), topay)._BUTTON();

            h._FORM();
            h._BOTTOMBAR();
        }, true, 120); // NOTE publicly cacheable though within a private context
    }

    public async Task pur(WebContext wc, int cmd)
    {
        var rtl = wc[-3].As<Org>();
        int lotid = wc[0];

        var prin = (User)wc.Principal;

        // submitted values
        var f = await wc.ReadAsync<Form>();
        short qtyx = f[nameof(qtyx)];

        using var dc = NewDbContext(IsolationLevel.ReadCommitted);
        try
        {
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM lots_vw WHERE id = @1");
            var lot = await dc.QueryTopAsync<Item>(p => p.Set(lotid));

            var qty = qtyx * lot.unitx;
            var sup = GrabTwin<int, Org>(lot.orgid);
            var fee = BankUtility.supfee;

            var o = new Pur(lot, rtl, sup)
            {
                created = DateTime.Now,
                creator = prin.name,
                qty = qty,
                fee = fee,
                topay = lot.RealPrice * qty + fee,
                status = -1
            };

            // check and try to use an existing record
            int purid = 0;
            if (await dc.QueryTopAsync("SELECT id FROM purs WHERE rtlid = @1 AND status = -1 LIMIT 1", p => p.Set(rtl.id)))
            {
                dc.Let(out purid);
            }

            // make use of any existing abandoned record
            const short msk = MSK_BORN | MSK_EDIT | MSK_STATUS;
            if (purid == 0)
            {
                dc.Sql("INSERT INTO purs ").colset(Pur.Empty, msk)._VALUES_(Pur.Empty, msk).T(" RETURNING id, topay");
                await dc.QueryTopAsync(p => o.Write(p));
            }
            else
            {
                dc.Sql("UPDATE purs ")._SET_(Pur.Empty, msk).T(" WHERE id = @1 RETURNING id, topay");
                await dc.QueryTopAsync(p =>
                {
                    o.Write(p);
                    p.Set(purid);
                });
            }
            dc.Let(out purid);
            dc.Let(out decimal topay);

            // call WeChatPay to prepare order there
            string trade_no = (purid + "-" + topay).Replace('.', '-');
            var (prepay_id, err_code) = await WeChatUtility.PostUnifiedOrderAsync(sup: true,
                trade_no,
                topay,
                prin.im, // the payer
                wc.RemoteIpAddress.ToString(),
                MainApp.MgtUrl + "/" + nameof(MgtService.onpay),
                o.ToString()
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
        catch (Exception e)
        {
            dc.Rollback();
            Application.Err(e.Message);
            wc.Give(500);
        }
    }
}