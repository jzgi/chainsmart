using System;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class WareWork<V> : WebWork where V : WareVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    public class PublyWareWork : WareWork<PublyWareVarWork>
    {

    }

    [OrglyAuthorize(Org.TYP_SHP, 1)]
    [Ui("零售商品", "商户")]
    public class ShplyWareWork : WareWork<ShplyWareVarWork>
    {
        [Ui("零售商品", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares_vw WHERE shpid = @1 AND status > 0 ORDER BY status DESC, id");
            var arr = await dc.QueryAsync<Ware>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: STA_FINE);

                if (arr == null)
                {
                    h.ALERT("暂无商品");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
                    if (o.itemid > 0)
                    {
                        h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.itemid).T("/icon")._PIC();
                    }
                    else if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/ware/").T(o.id).T("/icon")._PIC();
                    }
                    else
                    {
                        h.PIC("/void.webp", css: "uk-width-1-5");
                    }

                    h.ASIDE_();
                    h.HEADER_().H4(o.name).SPAN(Ware.Statuses[o.status], "uk-badge")._HEADER();
                    h.Q(o.tip, "uk-width-expand");
                    h.FOOTER_().T("每件").SP().T(o.unitx).SP().T(o.unit).SPAN_("uk-margin-auto-left").CNY(o.price)._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            });
        }

        [Ui(icon: "ban", group: 2), Tool(Anchor)]
        public async Task ban(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares_vw WHERE shpid = @1 AND status = 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Ware>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: STA_VOID);
                if (arr == null)
                {
                    h.ALERT("暂无商品");
                    return;
                }
                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
                    if (o.itemid > 0)
                    {
                        h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/item/").T(o.id).T("/icon")._PIC();
                    }
                    else if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/ware/").T(o.id).T("/icon")._PIC();
                    }
                    else
                    {
                        h.PIC("/void.webp", css: "uk-width-1-5");
                    }

                    h.ASIDE_();
                    h.HEADER_().H4(o.name).SPAN(Ware.Statuses[o.status], "uk-badge")._HEADER();
                    h.Q(o.tip, "uk-width-expand");
                    h.FOOTER_().T("每件").SP().T(o.unitx).SP().T(o.unit).SPAN_("uk-margin-auto-left").CNY(o.price)._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            });
        }

        [OrglyAuthorize(Org.TYP_SHP, User.ROL_MGT)]
        [Ui("自定", "自定义商品", icon: "plus", group: 7), Tool(ButtonOpen)]
        public async Task def(WebContext wc, int state)
        {
            var org = wc[-1].As<Org>();

            var prin = (User) wc.Principal;
            var cats = Grab<short, Cat>();

            if (wc.IsGet)
            {
                var o = new Ware
                {
                    created = DateTime.Now,
                    state = (short) state,
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("产品和销售信息");

                    h.LI_().TEXT("产品名", nameof(o.name), o.name, max: 12).SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().TEXT("计价单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("每件含量", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("大客户立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订件数", nameof(o.min), o.min).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增", nameof(o.step), o.step)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(def))._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_BORN | MSK_EDIT;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Ware
                {
                    shpid = org.id,
                    created = DateTime.Now,
                    creator = prin.name,
                });

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO wares ").colset(Ware.Empty, msk)._VALUES_(Ware.Empty, msk);
                await dc.ExecuteAsync(p => m.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }

        [Ui("导入", "平台导入商品", icon: "cloud-download", group: 7), Tool(ButtonOpen)]
        public async Task imp(WebContext wc, int state)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT DISTINCT itemid, name FROM books WHERE shpid = @1 AND status = 4 AND itemid NOT IN (SELECT itemid FROM wares WHERE shpid = @1)");
                await dc.QueryAsync(p => p.Set(org.id));
                var map = dc.ToIntMap();

                var o = new Ware
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    unitx = 1.0M,
                    min = 1, max = 30, step = 1,
                };

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("产品和销售信息");

                    h.LI_().SELECT("供应链产品", nameof(o.itemid), o.itemid, map, required: true)._LI();
                    h.LI_().TEXT("计价单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("每件含量", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("大客户立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订件数", nameof(o.min), o.min).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增", nameof(o.step), o.step)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(imp))._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_BORN | MSK_EDIT;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Ware
                {
                    shpid = org.id,
                    created = DateTime.Now,
                    creator = prin.name,
                });
                var item = GrabObject<int, Item>(m.itemid);
                m.typ = item.typ;
                m.name = item.name;
                m.tip = item.tip;

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO wares ").colset(Ware.Empty, msk)._VALUES_(Ware.Empty, msk);
                await dc.ExecuteAsync(p => m.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }
    }
}