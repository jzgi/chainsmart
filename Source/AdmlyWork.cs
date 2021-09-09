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
            // information

            MakeWork<AdmlyAccessWork>("acc");

            MakeWork<AdmlyUserWork>("user");

            MakeWork<AdmlyRegWork>("reg");

            MakeWork<AdmlyItemWork>("item");

            MakeWork<AdmlyPeerWork>("peer");

            // operation

            MakeWork<AdmlyOrgWork>("org");

            MakeWork<AdmlyProdWork>("prod");

            MakeWork<AdmlyBuyWork>("buy");

            MakeWork<AdmlyPurchWork>("purch");

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

                h.OPLIST();
            });
        }

        [UserAuthorize(admly: 1)]
        public override Task setg(WebContext wc)
        {
            return base.setg(wc);
        }
    }
}