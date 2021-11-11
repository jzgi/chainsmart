using SkyChain.Web;

namespace Revital
{
    public abstract class PostWork : WebWork
    {
    }

    public class PublyPostWork : PostWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PublyPostVarWork>();
        }

        public void @default(WebContext wc, int page)
        {
        }
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