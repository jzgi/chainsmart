using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;

namespace ChainSmart;

public abstract class MgtWork : WebWork
{
}

[UserAuthenticate, MgtAuthorize(0)]
[Ui("平台管理")]
public class AdmlyWork : MgtWork
{
    protected override void OnCreate()
    {
        // basic 

        CreateWork<AdmlyStdWork>("std", header: "常规");

        CreateWork<AdmlyRegWork>("reg");

        CreateWork<AdmlyMbrWork>("mbr");

        // biz

        CreateWork<AdmlyEstWork>("est", header: "业务");

        CreateWork<AdmlySupWork>("sup");

        CreateWork<AdmlyUserWork>("user");

        CreateWork<AdmlyCodeWork>("code");

        CreateWork<AdmlyAdWork>("ad");

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
            h.H1(Application.Nodal.name);
            h.P(prin.name);
            h._HEADER();

            h.PIC("/logo.webp", circle: true, css: "uk-width-small");

            h._TOPBARXL();

            h.WORKBOARD(accessTyp: -1);

            h.TOOLBAR(bottom: true);
        }, false, 900);
    }

    [Ui("参数", "参数", icon: "cog"), Tool(Modal.ButtonShow)]
    public void setg(WebContext wc)
    {
        string tel = null;

        var jo = Application.CustomConfig;

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

[UserAuthenticate]
[Ui("市场操作")]
public class MktlyWork : MgtWork
{
    protected override void OnCreate()
    {
        // id of either current user or the specified
        CreateVarWork<MktlyVarWork>((prin, key) =>
            {
                var orgid = key?.ToInt() ?? ((User)prin).mktid;
                return GrabTwin<int, Org>(orgid);
            }
        );
    }
}

[UserAuthenticate]
[Ui("供应操作")]
public class SuplyWork : MgtWork
{
    protected override void OnCreate()
    {
        // id of either current user or the specified
        CreateVarWork<SuplyVarWork>((prin, key) =>
            {
                var orgid = key?.ToInt() ?? ((User)prin).supid;
                return GrabTwin<int, Org>(orgid);
            }
        );
    }
}