using System.Collections.Generic;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

public abstract class CarbWork : WebWork
{
    protected static void MainGrid(HtmlBuilder h, IList<Carb> lst)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");

            h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.dt).SPAN("", "uk-badge")._HEADER();
            h.Q(o.ToString(), "uk-width-expand");
            h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

[AdmlyAuthorize(User.ROL_MGT)]
[Ui("我的碳积分", "账号功能")]
public class MyCarbWork : CarbWork
{
    protected override void OnCreate()
    {
        CreateVarWork<AdmlyRegVarWork>();
    }

    [Ui("碳积分活动", group: 1), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Carb.Empty).T(" FROM carbs WHERE userid = @1 ORDER BY dt DESC");
        var arr = dc.Query<Carb>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            MainGrid(h, arr);
        }, false, 12);
    }
}