using System;
using System.Threading.Tasks;
using ChainFx.Nodal;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart
{
    public abstract class AssetWork<V> : WebWork where V : AssetVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }

        protected static void MainGrid(HtmlBuilder h, Asset[] arr)
        {
            h.MAINGRID(arr, o =>
            {
                h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                if (o.icon)
                {
                    h.PIC(MainApp.WwwUrl, "/asset/", o.id, "/icon", css: "uk-width-1-5");
                }
                else
                    h.PIC("/void.webp", css: "uk-width-1-5");

                h.ASIDE_();
                h.HEADER_().H4(o.name).SPAN(Statuses[o.status], "uk-badge")._HEADER();
                h.Q(o.tip, "uk-width-expand");
                h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                h._ASIDE();

                h._A();
            });
        }
    }

    public class PublyAssetWork : AssetWork<PublyAssetVarWork>
    {
        public void @default(WebContext wc)
        {
            wc.Give(300); // multiple choises
        }
    }

    [Ui("产源设施", "商户")]
    public class SrclyAssetWork : AssetWork<OrglyAssetVarWork>
    {
        [Ui("产源设施", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Asset.Empty).T(" FROM assets_vw WHERE orgid = @1 AND status = 4 ORDER BY oked DESC");
            var arr = await dc.QueryAsync<Asset>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("尚无产源设施");
                    return;
                }

                MainGrid(h, arr);
            }, false, 6);
        }

        [Ui(icon: "cloud-download", group: 2), Tool(Anchor)]
        public async Task off(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Asset.Empty).T(" FROM assets_vw WHERE orgid = @1 AND status BETWEEN 1 AND 2 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Asset>(p => p.Set(org.id));

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
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Asset.Empty).T(" FROM assets_vw WHERE orgid = @1 AND status = 0 ORDER BY adapted DESC");
            var arr = await dc.QueryAsync<Asset>(p => p.Set(org.id));

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

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("新建", "新建产源设施", icon: "plus", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int state)
        {
            var org = wc[-1].As<Org>();
            var prin = (User)wc.Principal;

            if (wc.IsGet)
            {
                var o = new Asset
                {
                    created = DateTime.Now,
                    creator = prin.name
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_();

                    h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 12)._LI();
                    h.LI_().SELECT("类别", nameof(o.typ), o.typ, Asset.Typs, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().NUMBER("经度", nameof(o.x), o.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(o.y), o.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().TEXTAREA("规格参数", nameof(o.specs), o.specs, max: 100)._LI();
                    h.LI_().SELECT("碳减排项目", nameof(o.cern), o.cern, Cern.Typs)._LI();
                    // h.LI_().NUMBER("碳减排因子", nameof(o.factor), o.factor, min: 0.01)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_BORN | MSK_EDIT;
                // populate 
                var m = await wc.ReadObjectAsync(msk, new Asset
                {
                    orgid = org.id,
                    created = DateTime.Now,
                    creator = prin.name,
                });

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO assets ").colset(Asset.Empty, msk)._VALUES_(Asset.Empty, msk);
                await dc.ExecuteAsync(p => m.Write(p, msk));

                wc.GivePane(200); // close dialog
            }
        }
    }
}