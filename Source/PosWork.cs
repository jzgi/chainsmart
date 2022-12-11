using ChainFx.Web;

namespace ChainMart
{
    public abstract class PosWork<V> : WebWork where V : PosVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    [OrglyAuthorize(Org.TYP_SHP, User.ROL_OPN)]
    [Ui("现场消费终端", "商户")]
    public class ShplyPosWork : PosWork<ShplyPosVarWork>
    {
        public void @default(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.ALERT("限在智能秤上使用");
            });
        }
    }
}