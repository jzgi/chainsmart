using System.Threading.Tasks;
using SkyChain.Db;
using SkyChain.Web;

namespace Zhnt
{
    [UserAuthorize(admly: 1)]
    [Ui("平台管理")]
    public class AdmlyWork : ChainWork
    {
        protected override void OnMake()
        {
            // management

            MakeWork<AdmlyAccessWork>("acc");

            MakeWork<AdmlyRegWork>("reg");

            MakeWork<AdmlyItemWork>("item");

            // sales & marketing

            MakeWork<AdmlyOrgWork>("biz",
                state: 1,
                ui: new UiAttribute("商户管理"),
                authorize: new UserAuthorizeAttribute(admly: User.ADMLY_SAL)
            );

            MakeWork<AdmlyDProdWork>("dprod");

            MakeWork<AdmlyDOrdWork>("dord");

            // purchase

            MakeWork<AdmlyOrgWork>("src",
                state: 2,
                ui: new UiAttribute("产源管理"),
                authorize: new UserAuthorizeAttribute(admly: User.ADMLY_PUR)
            );

            MakeWork<AdmlyUProdWork>("uprod");

            MakeWork<AdmlyUOrdWork>("uord");

            MakeWork<AdmlyPeerWork>("peer");

            // accounting

            // MakeWork<AdmlyClearWork>("cash");
        }

        public void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            var o = ChainEnviron.Info;
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: prin.name + "（" + User.Admly[prin.admly] + "）");
                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body");
                if (o != null)
                {
                    h.LI_().FIELD("节点编号", o.Id)._LI();
                    h.LI_().FIELD("名称", o.Name)._LI();
                    h.LI_().FIELD("连接地址", o.Domain)._LI();
                    h.LI_().FIELD("状态", Peer.Statuses[o.Status])._LI();
                    h.LI_().FIELD("当前区块", o.CurrentBlockId)._LI();
                }
                h._UL();
                h._FORM();

                h.FORM_("uk-card uk-card-primary");
                h.OPLIST();
                h._FORM();
            });
        }

        [UserAuthorize(admly: 1)]
        public override Task setg(WebContext wc)
        {
            return base.setg(wc);
        }
    }
}