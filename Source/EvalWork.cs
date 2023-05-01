using ChainFx.Web;

namespace ChainSmart
{
    public abstract class EvalWork<V> : WebWork where V : EvalVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    [OrglyAuthorize(Org.TYP_MKT)]
    [Ui("评估", "常规")]
    public class OrglyEvalWork : EvalWork<OrglyEvalVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }


    [OrglyAuthorize(Org.TYP_MKT)]
    [Ui("综合评估", "机构")]
    public class MktlyEvalWork : EvalWork<MktlyEvalVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [OrglyAuthorize(Org.TYP_CTR)]
    [Ui("综合评估", "机构")]
    public class CtrlyEvalWork : EvalWork<CtrlyEvalVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }
}