using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Nodality;
using static ChainFX.Web.ToolAttribute;

namespace ChainSmart;

public abstract class SrcWork<V> : WebWork where V : SrcVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Src> lst)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

            if (o.icon)
            {
                h.PIC(MainApp.WwwUrl, "/src/", o.id, "/icon", css: "uk-width-1-5");
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

public class WwwSrcWork : SrcWork<WwwSrcVarWork>
{
    public void @default(WebContext wc)
    {
        wc.Give(300); // multiple choises
    }
}

[Ui("分管产源")]
public class SuplySrcOldWork : SrcWork<SuplySrcVarWork>
{
    [Ui("分管产源", status: 1), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        var arr = GrabTwinArray<int, Src>(org.id, filter: x => x.status == 4, sorter: (x, y) => x.oked.CompareTo(y.oked));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无上线的产源设施");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(icon: "cloud-download", status: 2), Tool(Anchor)]
    public void down(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        var arr = GrabTwinArray<int, Src>(org.id, filter: x => x.status is 1 or 2, sorter: (x, y) => x.oked.CompareTo(y.oked));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无下线的产源设施");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(icon: "trash", status: 4), Tool(Anchor)]
    public void @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        var arr = GrabTwinArray<int, Src>(org.id, filter: x => x.status == 0, sorter: (x, y) => x.adapted.CompareTo(y.adapted));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无删除的产源设施");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [UserAuthorize(Org.TYP_SUP, User.ROL_OPN)]
    [Ui("新建", "新建产源设施",icon: "plus", status: 2), Tool(ButtonOpen)]
    public async Task @new(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            var o = new Src
            {
                created = DateTime.Now,
                creator = prin.name
            };
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().TEXT("产源设施名", nameof(o.name), o.name, min: 2, max: 12)._LI();
                h.LI_().SELECT("类别", nameof(o.typ), o.typ, Src.Typs, required: true).SELECT("等级", nameof(o.rank), o.rank, Src.Ranks, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().TEXTAREA("说明", nameof(o.remark), o.remark, max: 200)._LI();
                h.LI_().TEXTAREA("规格", nameof(o.specs), o.specs, max: 300)._LI();
                h.LI_().NUMBER("碳积分因子", nameof(o.co2ekg), o.co2ekg, min: 0.00M, max: 99.99M)._LI();
                h.LI_().NUMBER("经度", nameof(o.x), o.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(o.y), o.y, min: -90.000, max: 90.000)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, new Src
            {
                orgid = org.id,
                created = DateTime.Now,
                creator = prin.name,
            });

            await GetTwinCache<SrcCache, int, Src>().CreateAsync(async dc =>
            {
                dc.Sql("INSERT INTO srcs_vw ").colset(Src.Empty, msk)._VALUES_(Src.Empty, msk).T(" RETURNING ").collst(Src.Empty);
                return await dc.QueryTopAsync<Src>(p => m.Write(p, msk));
            });

            wc.GivePane(200); // close dialog
        }
    }
}