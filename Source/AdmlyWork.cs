using System.Threading.Tasks;
using SkyChain.Db;
using SkyChain.Web;
using Zhnt.Market;
using Zhnt.Supply;

namespace Zhnt
{
    [UserAuthorize(admly: 1)]
    [Ui("联盟")]
    public class AdmlyWork : ChainWork
    {
        protected override void OnMake()
        {
            MakeWork<AdmlyAccessWork>("acc");

            MakeWork<AdmlyOrgWork>("org");

            MakeWork<AdmlyProdWork>("prod");

            MakeWork<AdmlyItemWork>("item");

            // order processing

            MakeWork<AdmlyUoWork>("uo");

            MakeWork<AdmlyDoWork>("do");

            // accounting

            MakeWork<AdmlyClearWork>("cash");
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