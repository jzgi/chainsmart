using ChainFx;
using ChainFx.Web;

namespace ChainSmart;

[UserAuthenticate, AdmlyAuthorize]
[Ui("平台管理")]
public class AdmlyWork : WebWork
{
    protected override void OnCreate()
    {
        // basic 

        CreateWork<AdmlyAccessWork>("access", header: "常规");

        CreateWork<AdmlyRegWork>("reg");

        // biz

        CreateWork<AdmlyOrgWork>("org", header: "业务");

        CreateWork<AdmlyUserWork>("user");

        // fin

        CreateWork<AdmlyBuyApWork>("buyap", header: "财务");

        CreateWork<AdmlyPurApWork>("purap");

        CreateWork<AdmlyBuyLdgWork>("buyldg");

        CreateWork<AdmlyPurLdgWork>("purldg");
    }

    public void @default(WebContext wc)
    {
        var prin = (User)wc.Principal;

        wc.GivePage(200, h =>
        {
            h.TOPBARXL_();

            h.HEADER_("uk-width-expand uk-col uk-padding-left");
            h.H1(Application.Name);
            h.P2(prin.name, User.Orgly[wc.Role], brace: true);
            h._HEADER();

            h.PIC("/logo.webp", circle: true, css: "uk-width-small");

            h._TOPBARXL();

            h.WORKBOARD();

            h.TOOLBAR(bottom: true);
        }, false, 900);
    }

    [Ui("参数", "参数", icon: "cog"), Tool(Modal.ButtonShow)]
    public void setg(WebContext wc)
    {
        string tel = null;

        var jo = Application.Program;

        jo.Get(nameof(tel), ref tel);

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider uk-large");

            h.LI_().FIELD("监督电话", tel)._LI();
            h._UL();

            h.TOOLBAR(bottom: true);
        });
    }
}