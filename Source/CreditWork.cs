using ChainFx.Web;

namespace ChainSmart
{
    public class CreditWork : WebWork
    {
    }

    [Ui("碳积分账户", "常规")]
    public class OrglyCreditWork : CreditWork
    {
        public void @default(WebContext wc)
        {
            wc.GivePane(200, h =>
            {
                h.ALERT("该功能尚未开启");
                h.TOOLBAR(bottom: true);
            }, false, 7);
        }
    }
}