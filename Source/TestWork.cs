using ChainFx.Web;

namespace ChainMart
{
    public abstract class TestWork<V> : WebWork where V : TestVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    [Ui("检测管理", "市场")]
    public class MktlyTestWork : TestWork<MktlyTestVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [Ui("检测管理", "供区")]
    public class ZonlyTestWork : TestWork<ZonlyTestVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }
}