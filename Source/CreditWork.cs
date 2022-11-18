using ChainFx.Web;

namespace ChainMart
{
    public class CreditWork : WebWork
    {
    }

    [Ui("碳积分账户", "账号")]
    public class MyCreditWork : CreditWork
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