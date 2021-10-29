using SkyChain.Chain;
using SkyChain.Web;

namespace Revital.Supply
{
    [UserAuthorize(admly: User_.ADMLY_SUPLLY_)]
    [Ui("供应平台管理")]
    public class AdmlyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeWork<AdmlyRegWork>("reg");

            MakeWork<AdmlyOrgWork>("org");

            MakeWork<AdmlyUserWork>("user");

            MakeWork<AdmlyItemWork>("item");

            MakeWork<AdmlyClearWork>("clear");

            MakeWork<AdmlyAccessWork>("access");

            MakeWork<ChainWork>("chain", authorize: new UserAuthorizeAttribute(admly: User_.ADMLY_SUPLLY_MGT));
        }

        public void @default(WebContext wc)
        {
            var prin = (User_) wc.Principal;
            var o = Chain.Info;
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: prin.name + "（" + User_.Admly[prin.admly] + "）");
                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body");
                if (o != null)
                {
                    h.LI_().FIELD("系统名称", o.Name)._LI();
                    h.LI_().FIELD("描述", o.Tip)._LI();
                    h.LI_().FIELD("连接地址", o.Uri)._LI();
                }
                h._UL();
                h._FORM();

                h.TASKUL();
            });
        }
    }
}