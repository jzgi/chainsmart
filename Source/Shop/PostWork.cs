using SkyChain.Web;

namespace Revital.Shop
{
    public abstract class PostWork : WebWork
    {
    }

    [Ui("货架")]
    public class BizlyPostWork : PostWork
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