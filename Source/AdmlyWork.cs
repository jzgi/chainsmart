using System.Threading.Tasks;
using SkyChain.Db;
using SkyChain.Web;

namespace Zhnt.Supply
{
    [UserAuthorize(admly: 1)]
    [Ui("平台管理")]
    public class AdmlyWork : ChainWork
    {
        protected override void OnMake()
        {
            // management

            MakeWork<AdmlyRegWork>("reg");

            MakeWork<AdmlyOrgWork>("org",
                state: 0,
                ui: new UiAttribute("机构管理"),
                authorize: new UserAuthorizeAttribute(admly: User.ADMLY_MGT)
            );

            MakeWork<AdmlyItemWork>("item");

            MakeWork<AdmlyAccessWork>("access");

            MakeWork<AdmlyPeerWork>("peer");

            // sales & marketing

            MakeWork<AdmlyOrgWork>("biztm",
                state: 1,
                ui: new UiAttribute("商户社管理"),
                authorize: new UserAuthorizeAttribute(admly: User.ADMLY_SAL)
            );

            MakeWork<AdmlyDownWork>("down");

            MakeWork<AdmlyDownBuyWork>("downbuy");

            // purchase

            MakeWork<AdmlyOrgWork>("srctm",
                state: 2,
                ui: new UiAttribute("产源社管理"),
                authorize: new UserAuthorizeAttribute(admly: User.ADMLY_PUR)
            );

            MakeWork<AdmlyUpWork>("up");

            MakeWork<AdmlyUpBuyWork>("upbuy");

            // accounting

            MakeWork<AdmlyClearWork>("clear");
        }

        public override void @default(WebContext wc)
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