using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

public abstract class CerWork : WebWork
{
    protected static void MainGrid(HtmlBuilder h, Reg[] arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");

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

[AdmlyAuthorize(User.ROL_MGT)]
[Ui("我的碳积分", "账号功能")]
public class MyCerWork : CerWork
{
    protected override void OnCreate()
    {
        CreateVarWork<AdmlyRegVarWork>();
    }

    [Ui("省份", group: 1), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE typ = ").T(Reg.TYP_PROVINCE).T(" ORDER BY id, status DESC");
        var arr = dc.Query<Reg>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: Reg.TYP_PROVINCE);

            MainGrid(h, arr);
        }, false, 12);
    }
}