using ChainFx.Web;

namespace ChainSmart
{
    public abstract class TestWork<V> : WebWork where V : TestVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    [OrglyAuthorize(Org.TYP_MKT, 1)]
    [Ui("检测报告", "机构")]
    public class MktlyTestWork : TestWork<MktlyTestVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [OrglyAuthorize(Org.TYP_CTR, 1)]
    [Ui("检测报告", "机构")]
    public class CtrlyTestWork : TestWork<CtrlyTestVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }
}