using SkyChain.Web;

namespace Revital.Shop
{
    [Ui("货架")]
    public class BizlyPostWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyPostVarWork>();
        }

        public void @default(WebContext wc, int page)
        {
        }
    }
}