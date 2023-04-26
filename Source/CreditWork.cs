using ChainFx.Web;

namespace ChainSmart
{
    public abstract class CreditWork<V> : WebWork where V : CreditVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    [OrglyAuthorize(Org.TYP_MKT)]
    [Ui("综合评估", "机构")]
    public class MktlyCreditWork : CreditWork<MktlyCreditVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [OrglyAuthorize(Org.TYP_CTR)]
    [Ui("综合评估", "机构")]
    public class CtrlyCreditWork : CreditWork<CtrlyCreditVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }
}