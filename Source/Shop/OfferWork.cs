using SkyChain.Web;

namespace Revital.Shop
{
    [Ui("货架")]
    public class BizlyArticleWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyOfferVarWork>();
        }

        public void @default(WebContext wc, int page)
        {
        }
    }
}