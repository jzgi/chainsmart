using System;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSMart
{
    public abstract class ItemWork<V> : WebWork where V : ItemVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }

        protected static void MainGrid(HtmlBuilder h, Item[] arr)
        {
            h.MAINGRID(arr, o =>
            {
                h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                if (o.icon)
                {
                    h.PIC(MainApp.WwwUrl, "/item/", o.id, "/icon", css: "uk-width-1-5");
                }
                else
                    h.PIC("/void.webp", css: "uk-width-1-5");

                h.ASIDE_();
                h.HEADER_().H4(o.name).SPAN(Item.Statuses[o.status], "uk-badge")._HEADER();
                h.Q(o.tip, "uk-width-expand");
                h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                h._ASIDE();

                h._A();
            });
        }
    }

    public class PublyItemWork : ItemWork<PublyItemVarWork>
    {
        public void @default(WebContext wc)
        {
            wc.Give(300); // multiple choises
        }
    }

    [OrglyAuthorize(Org.TYP_SRC, 1)]
    [Ui("产品设置", "商户")]
    public class SrclyItemWork : ItemWork<SrclyItemVarWork>
    {
        [Ui("在线产品", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE srcid = @1 AND status = 4 ORDER BY ended DESC");
            var arr = await dc.QueryAsync<Item>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("尚无产品");
                    return;
                }

                MainGrid(h, arr);
            }, false, 6);
        }

        [Ui(icon: "cloud-download", group: 2), Tool(Anchor)]
        public async Task off(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE srcid = @1 AND status BETWEEN 1 AND 2 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Item>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    return;
                }

                MainGrid(h, arr);
            }, false, 6);
        }

        [Ui(icon: "trash", group: 4), Tool(Anchor)]
        public async Task aborted(WebContext wc)
        {
            var src = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE srcid = @1 AND status = 8 ORDER BY ended DESC");
            var arr = await dc.QueryAsync<Item>(p => p.Set(src.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    return;
                }

                MainGrid(h, arr);
            }, false, 6);
        }

        [OrglyAuthorize(Org.TYP_SRC, User.ROL_OPN)]
        [Ui("新建", "新建产品", icon: "plus", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int state)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var cats = Grab<short, Cat>();

            if (wc.IsGet)
            {
                var o = new Item
                {
                    created = DateTime.Now,
                    state = (short) state,
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("填写产品资料");

                    h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 12)._LI();
                    h.LI_().SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().TEXT("基地", nameof(o.origin), o.origin, tip: "自产可不填")._LI();
                    h.LI_().SELECT("贮藏方法", nameof(o.store), o.store, Item.Stores, required: true).NUMBER("保存天数", nameof(o.duration), o.duration, min: 1, required: true)._LI();
                    h.LI_().TEXTAREA("规格参数", nameof(o.specs), o.specs, max: 100)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_BORN | MSK_EDIT;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Item
                {
                    srcid = org.id,
                    created = DateTime.Now,
                    creator = prin.name,
                });

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO items ").colset(Item.Empty, msk)._VALUES_(Item.Empty, msk);
                await dc.ExecuteAsync(p => m.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }
    }
}