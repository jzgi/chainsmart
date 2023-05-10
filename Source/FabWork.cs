using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

public abstract class FabWork<V> : WebWork where V : FabVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Fab> lst)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

            if (o.icon)
            {
                h.PIC(MainApp.WwwUrl, "/fab/", o.id, "/icon", css: "uk-width-1-5");
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

public class PublyFabWork : FabWork<PublyFabVarWork>
{
    public void @default(WebContext wc)
    {
        wc.Give(300); // multiple choises
    }
}

[Ui("产品源", "商户")]
public class SuplyFabWork : FabWork<SuplyFabVarWork>
{
    [Ui("产品源", group: 1), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        var arr = GrabTwinArray<int, int, Fab>(org.id, cond: x => x.status == 4, comp: (x, y) => x.oked.CompareTo(y.oked));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无上线产品源");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(icon: "cloud-download", group: 2), Tool(Anchor)]
    public void down(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        var arr = GrabTwinArray<int, int, Fab>(org.id, cond: x => x.status is 1 or 2, comp: (x, y) => x.oked.CompareTo(y.oked));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无下线产品源");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(icon: "trash", group: 4), Tool(Anchor)]
    public void @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        var arr = GrabTwinArray<int, int, Fab>(org.id, cond: x => x.status == 0, comp: (x, y) => x.adapted.CompareTo(y.adapted));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无删除产品源");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [OrglyAuthorize(0, User.ROL_OPN, ulevel: 2)]
    [Ui("新建", "新建产品源", icon: "plus", group: 1), Tool(ButtonOpen)]
    public async Task @new(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            var o = new Fab
            {
                created = DateTime.Now,
                creator = prin.name
            };
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();

                h.LI_().TEXT("产品源名称", nameof(o.name), o.name, min: 2, max: 12)._LI();
                h.LI_().SELECT("类别", nameof(o.typ), o.typ, Fab.Typs, required: true)._LI();
                h.LI_().TEXTAREA("简述", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().NUMBER("经度", nameof(o.x), o.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(o.y), o.y, min: -90.000, max: 90.000)._LI();
                h.LI_().SELECT("等级", nameof(o.rank), o.rank, Fab.Ranks, required: true)._LI();
                h.LI_().TEXTAREA("说明", nameof(o.remark), o.remark, max: 100)._LI();
                h.LI_().TEXTAREA("规格参数", nameof(o.specs), o.specs, max: 100)._LI();
                // h.LI_().SELECT("碳减排项目", nameof(o.piece), o.piece, Cern.Typs)._LI();
                // h.LI_().NUMBER("碳减排因子", nameof(o.co2ekg), o.co2ekg, min: 0.00M)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, new Fab
            {
                orgid = org.id,
                created = DateTime.Now,
                creator = prin.name,
            });

            // insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO fabs ").colset(Fab.Empty, msk)._VALUES_(Fab.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}