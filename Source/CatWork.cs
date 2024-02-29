using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.ToolAttribute;

namespace ChainSmart;

public abstract class CatWork : WebWork
{
    protected static void MainGrid(HtmlBuilder h, Cat[] arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");

            h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name).SPAN(Entity.Statuses[o.status], "uk-badge")._HEADER();
            h.Q(o.tip, "uk-width-expand");
            h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

[Ui("品类")]
public class AdmlyCatWork : CatWork
{
    protected override void OnCreate()
    {
        CreateVarWork<AdmlyCatVarWork>();
    }

    [Ui(status: 1), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Cat.Empty).T(" FROM cats ORDER BY typ, status DESC");
        var arr = dc.Query<Cat>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            MainGrid(h, arr);
        }, false, 15);
    }


    [Ui("新建", "创建新的品类", icon: "plus", status: 7), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int typ)
    {
        var prin = (User)wc.Principal;
        var o = new Cat
        {
            typ = (short)typ,
            created = DateTime.Now,
            creator = prin.name,
        };
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("品类信息");
                h.LI_().NUMBER("品类编号", nameof(o.typ), o.typ, min: 1, max: 99, required: true)._LI();
                h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, min: 2, max: 40)._LI();
                h.LI_().NUMBER("排序", nameof(o.idx), o.idx, min: 1, max: 99)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new), subscript: typ)._FORM();
            });
        }
        else // POST
        {
            const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;

            o = await wc.ReadObjectAsync(msk, instance: o);
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO Cats ").colset(Cat.Empty, msk)._VALUES_(Cat.Empty, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}