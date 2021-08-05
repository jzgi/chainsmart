using System.Threading.Tasks;
using SkyChain.Db;
using SkyChain.Web;
using Zhnt.Mart;
using Zhnt.Supply;

namespace Zhnt
{
    [UserAuthorize(admly: 1)]
    [Ui("联盟链")]
    public class AdmlyWork : ChainWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyVarWork>();

            MakeWork<AdmlyAccessWork>("acc");

            MakeWork<AdmlyRegWork>("reg");

            MakeWork<AdmlyBizWork>("biz");

            MakeWork<AdmlyOrgWork>("org");

            MakeWork<AdmlyUserWork>("user");

            MakeWork<AdmlyClearWork>("cash");

            MakeWork<AdmlyMatWork>("mat");

            MakeWork<AdmlyItemWork>("item");
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

    public class AdmlyVarWork : ChainVarWork
    {
        [UserAuthorize(admly: 3)]
        public override Task upd(WebContext wc)
        {
            return base.upd(wc);
        }
    }
}