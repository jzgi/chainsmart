using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;
using static ChainFx.Web.ToolAttribute;

namespace ChainMart
{
    public abstract class LotWork<V> : WebWork where V : LotVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    public class PublyLotWork : LotWork<PublyLotVarWork>
    {
        public async Task @default(WebContext wc, int id)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT id FROM lots WHERE nend >= @1 AND nstart <= @1 ORDER BY nend ASC LIMIT 1");
            if (await dc.QueryTopAsync(p => p.Set(id)))
            {
                dc.Let(out int lotid);
                wc.GiveRedirect(lotid + "/");
            }
            else
            {
                wc.GivePage(304, h => h.ALERT("此溯源码没有绑定产品"));
            }
        }
    }


    [OrglyAuthorize(Org.TYP_SRC, 1)]
    [Ui("产品销售批次", "产源")]
    public class SrclyLotWork : LotWork<SrclyLotVarWork>
    {
        [Ui("产品销售批次", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE srcid = @1 AND status > 0 ORDER BY status DESC, id DESC");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("暂无销售批次");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN("")._HEADER();
                    h.P(o.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            });
        }

        [Ui(icon: "history", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE srcid = @1 AND status = 8 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN("")._HEADER();
                    h.P(o.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            });
        }

        [Ui("新建", "新建产品销售批次", icon: "plus", group: 3), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var topOrgs = Grab<int, Org>();

            var zon = org.prtid == 0 ? org : topOrgs[org.prtid];

            var o = new Lot
            {
                status = Entity.STU_CREATED,
                srcid = org.id,
                srcname = org.name,
                zonid = zon.id,
                unitx = 1.0M,
                created = DateTime.Now,
                creator = prin.name,
                min = 1, max = 200, step = 1,
            };

            if (wc.IsGet)
            {
                using var dc = NewDbContext();

                await dc.QueryAsync("SELECT id, name FROM items_vw WHERE srcid = @1 AND status = 4", p => p.Set(org.id));
                var items = dc.ToIntMap();

                if (items == null)
                {
                    wc.GivePane(200, h => h.ALERT("尚无上线的产品"));
                    return;
                }

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("产品销售批次信息");

                    h.LI_().SELECT("已上线产品", nameof(o.itemid), o.itemid, items, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, tip: "可选", max: 50)._LI();
                    h.LI_().DATE("预售交割", nameof(o.dated), o.dated)._LI();
                    h.LI_().TEXT("计价单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("每件含量", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订件数", nameof(o.min), o.min).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增", nameof(o.step), o.step)._LI();
                    h.LI_().NUMBER("批次总件数", nameof(o.cap), o.cap).NUMBER("可售件数", nameof(o.avail), o.avail)._LI();

                    h._FIELDSUL();

                    h.BOTTOM_BUTTON("确认", nameof(@new));

                    h._FORM();
                });
            }
            else // POST
            {
                const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
                // populate 
                await wc.ReadObjectAsync(msk, instance: o);

                var item = GrabObject<int, Item>(o.itemid);
                o.name = item.name;
                o.typ = item.typ;

                // db insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO lots ").colset(Lot.Empty, msk)._VALUES_(Lot.Empty, msk);
                await dc.ExecuteAsync(p => o.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }
    }

    [OrglyAuthorize(Org.TYP_CTR, 1)]
    [Ui("产品销售批次统一盘存", "中库")]
    public class CtrlyLotWork : LotWork<CtrlyLotVarWork>
    {
    }

    public class ShplyBookLotWork : LotWork<ShplyBookLotVarWork>
    {
    }
}