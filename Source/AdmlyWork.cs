using System.Threading.Tasks;
using SkyChain.Db;
using SkyChain.Web;

namespace Zhnt.Supply
{
    [UserAuthorize(admly: 1)]
    [Ui("平台管理")]
    public class AdmlyWork : ChainMgtWork
    {
        protected override void OnMake()
        {
            // op

            MakeWork<AdmlyPlanWork>("plan");

            MakeWork<AdmlyOrgWork>("org");

            MakeWork<AdmlyBuyWork>("buy");

            MakeWork<AdmlyPurchWork>("purch");

            MakeWork<AdmlyClearWork>("clear");

            // basic

            MakeWork<AdmlyAccessWork>("access");

            MakeWork<AdmlyRegWork>("reg");

            MakeWork<AdmlyUserWork>("user");

            MakeWork<AdmlyItemWork>("item");
        }

        public override void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            var o = Chain.Info;
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

                h.TASKUL();
            });
        }

        [UserAuthorize(admly: 15)]
        public override async Task setg(WebContext wc)
        {
            await base.setg(wc);
        }

        [UserAuthorize(admly: 15)]
        public override async Task fed(WebContext wc)
        {
            await base.fed(wc);
        }
    }
}