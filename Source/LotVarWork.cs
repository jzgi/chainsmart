using System;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public class LotVarWork : WebWork
    {
        public async Task @default(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var items = GrabMap<int, int, Item>(org.id);
            var topOrgs = Grab<int, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1 AND srcid = @2");
            var m = await dc.QueryTopAsync<Lot>(p => p.Set(lotid).Set(org.id));

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品", items[m.itemid].ToString())._LI();
                h.LI_().FIELD("投放市场", topOrgs[m.ctrid].name)._LI();
                h.LI_().FIELD("物流经中控", m.ctring)._LI();
                h.LI_().FIELD("状态", Statuses[m.status])._LI();
                h.LI_().FIELD("单价", m.price)._LI();
                h.LI_().FIELD("直降", m.off)._LI();
                h.LI_().FIELD("起订量", m.min)._LI();
                h.LI_().FIELD("限订量", m.max)._LI();
                h.LI_().FIELD("递增量", m.step)._LI();
                h.LI_().FIELD("总量", m.cap)._LI();
                h.LI_().FIELD("剩余量", m.remain)._LI();
                h._UL();

                h.TOOLBAR(bottom: true);
            });
        }
    }

    public class SrclyLotVarWork : LotVarWork
    {
        [Ui("修改", "修改产品资料", icon: "pencil"), Tool(ButtonShow)]
        public async Task edit(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var topOrgs = Grab<int, Org>();
            var items = GrabMap<int, int, Item>(org.id);
            var prin = (User) wc.Principal;

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1 AND srcid = @2");
                var m = dc.QueryTop<Lot>(p => p.Set(lotid).Set(org.id));

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("销货参数");

                    h.LI_().NUMBER("单价", nameof(m.price), m.price, min: 0.00M, max: 99999.99M).NUMBER("直降", nameof(m.off), m.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(m.min), m.min).NUMBER("限订量", nameof(m.max), m.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(m.step), m.step)._LI();
                    h.LI_().NUMBER("总量", nameof(m.cap), m.cap).NUMBER("剩余量", nameof(m.remain), m.remain)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Statuses, filter: (k, v) => k > 0, required: true)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_EDIT;
                // populate 
                var m = await wc.ReadObjectAsync(0, new Item
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                });

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE products ")._SET_(Lot.Empty, msk).T(" WHERE id = @1 AND srcid = @2");
                await dc.ExecuteAsync(p =>
                {
                    m.Write(p, 0);
                    p.Set(lotid).Set(org.id);
                });

                wc.GivePane(200); // close dialog
            }
        }

        [Ui("删除", icon: "trash"), Tool(ButtonConfirm)]
        public async Task rm(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("DELETE FROM lots WHERE id = @1 AND srcid = @2");
            await dc.ExecuteAsync(p => p.Set(lotid).Set(org.id));

            wc.GivePane(200);
        }
    }

    public class CtrlyLotVarWork : LotVarWork
    {
        [Ui("贴标", "贴标", icon: "bookmark"), Tool(ButtonOpen)]
        public async Task label(WebContext wc)
        {
            int lotid = wc[0];
            var ctr = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1 AND ctrid = @2");
            var m = dc.QueryTop<Lot>(p => p.Set(lotid).Set(ctr.id));

            var src = GrabObject<int, Org>(m.srcid);

            wc.GivePane(200, h =>
            {
                int count = m.remain;
                h.UL_(grid: true, css: "uk-child-width-1-2@s");
                for (int i = 0; i < count; i++)
                {
                    h.LI_();
                    h.DIV_("uk-card uk-card-default uk-flex");
                    h.QRCODE(ChainMartApp.WwwUrl + "/lot/x-" + i, css: "uk-width-1-5");
                    h.DIV_("uk-width-expand uk-padding-small").P(src.name).T(i + 1)._DIV();
                    h._DIV();
                    h._LI();
                }
                h._UL();
            });
        }

        [Ui("验证", icon: "check"), Tool(ButtonOpen)]
        public async Task ok(WebContext wc)
        {
        }
    }
}