using System;
using System.Data;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Storage;

namespace ChainSmart;

public abstract class LotVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        const short msk = 255 | MSK_AUX;

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty, msk).T(" FROM lotops WHERE id = @1 AND orgid = @2");
        var o = await dc.QueryTopAsync<Lot>(p => p.Set(id).Set(org.id), msk);

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("消息标题", o.name)._LI();
            // h.LI_().FIELD("内容", o.content)._LI();
            h.LI_().FIELD("注解", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
            h.LI_().FIELD("状态", o.status, Lot.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "发布", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }
}

public class HublyLotVarWork : LotVarWork
{
}

public class MchlyPurLotVarWork : LotVarWork
{
    //
    // NOTE: this page is made publicly cacheable, though under variable path
    //
    public async Task @default(WebContext wc, int hubid)
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
            var fee = FinanceUtility.supfee;

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
        var org = wc[-3].As<Org>();
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
            var fee = FinanceUtility.supfee;

            var o = new Pur(lot, org, sup)
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
            if (await dc.QueryTopAsync("SELECT id FROM purs WHERE orgid = @1 AND status = -1 LIMIT 1", p => p.Set(org.id)))
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