using System.Threading.Tasks;
using SkyChain.Db;
using SkyChain.Web;

namespace Zhnt
{
    [UserAuthorize(admly: 1)]
    [Ui("联盟")]
    public class AdmlyWork : ChainWork
    {
        protected override void OnMake()
        {
            // management

            MakeWork<AdmlyAccessWork>("acc");

            MakeWork<AdmlyRegWork>("reg");

            MakeWork<AdmlyItemWork>("item");

            // sales & marketing

            MakeWork<AdmlyOrgWork>("dorg",
                state: 1,
                ui: new UiAttribute("商户"),
                authorize: new UserAuthorizeAttribute(admly: User.ADMLY_SAL)
            );

            MakeWork<AdmlyDProdWork>("dprod");

            MakeWork<AdmlyDOrdWork>("dord");

            // purchase

            MakeWork<AdmlyOrgWork>("uorg",
                state: 2,
                ui: new UiAttribute("产源"),
                authorize: new UserAuthorizeAttribute(admly: User.ADMLY_PUR)
            );

            MakeWork<AdmlyUProdWork>("uprod");

            MakeWork<AdmlyUOrdWork>("uord");

            // accounting

            // MakeWork<AdmlyClearWork>("cash");
        }

        public override void @default(WebContext wc)
        {
            bool inner = wc.Query[nameof(inner)];
            if (inner)
            {
                base.@default(wc);
            }
            else
            {
                wc.GiveFrame(200, false, 60, "平台管理");
            }
        }

        public override void friend(WebContext wc)
        {
            base.friend(wc);
        }

        [UserAuthorize(admly: 3)]
        public override Task upd(WebContext wc)
        {
            return base.upd(wc);
        }

        [UserAuthorize(admly: 3)]
        public override Task @new(WebContext wc)
        {
            return base.@new(wc);
        }
    }
}