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

        protected static void MainGrid(HtmlBuilder h, Lot[] arr)
        {
            h.MAINGRID(arr, o =>
            {
                h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                h.PIC(MainApp.WwwUrl, "/item/", o.itemid, "/icon", css: "uk-width-1-5");

                h.ASIDE_();
                h.HEADER_().H4(o.name).SPAN(Lot.Statuses[o.status], "uk-badge")._HEADER();
                h.Q(o.tip, "uk-width-expand");
                h.FOOTER_().T("每件").SP().T(o.unitx).SP().T(o.unit).SPAN_("uk-margin-auto-left").CNY(o.price)._SPAN()._FOOTER();
                h._ASIDE();

                h._A();
            });
        }
    }

    public class PublyLotWork : LotWork<PublyLotVarWork>
    {
    }


    [OrglyAuthorize(Org.TYP_SRC, 1)]
    [Ui("销售批次", "商户")]
    public class SrclyLotWork : LotWork<SrclyLotVarWork>
    {
        [Ui("在线销售批次", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE srcid = @1 AND status = 4 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("暂无销售批次");
                    return;
                }

                MainGrid(h, arr);
            }, false, 12);
        }

        [Ui(icon: "cloud-download", group: 2), Tool(Anchor)]
        public async Task off(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE srcid = @1 AND status BETWEEN 1 AND 2 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("暂无销售批次");
                    return;
                }

                MainGrid(h, arr);
            }, false, 12);
        }

        [Ui(icon: "trash", group: 4), Tool(Anchor)]
        public async Task aborted(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE srcid = @1 AND status = 8 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("暂无销售批次");
                    return;
                }

                MainGrid(h, arr);
            }, false, 12);
        }

        static readonly string[] Units = {"斤", "包", "箱", "桶"};


        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("新建", "新建销售批次", icon: "plus", group: 1), Tool(ButtonOpen)]
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
                min = 1,
                max = 200,
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
                    h.FORM_().FIELDSUL_("销售批次信息");

                    h.LI_().SELECT("已上线产品", nameof(o.itemid), o.itemid, items, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, tip: "可选", max: 40)._LI();
                    h.LI_().SELECT("限域投放", nameof(o.targs), o.targs, topOrgs, filter: (k, v) => v.EqCenter, capt: v => v.Ext, size: 2, required: true)._LI();
                    h.LI_().SELECT("交货条款", nameof(o.term), o.term, Lot.Terms, required: true).DATE("交货日期", nameof(o.dated), o.dated)._LI();
                    h.LI_().TEXT("基准单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true, datalst: Units).NUMBER("批发件含量", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();
                    h.LI_().NUMBER("基准单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("优惠立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订件数", nameof(o.min), o.min).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("批次总件数", nameof(o.cap), o.cap)._LI();

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
    [Ui("销售批次统一盘存", "中库")]
    public class CtrlyLotWork : LotWork<CtrlyLotVarWork>
    {
    }

    public class ShplyBookLotWork : LotWork<ShplyBookLotVarWork>
    {
    }
}