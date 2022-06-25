using CoChain.Web;

namespace Revital
{
    public class PublyWork : WebWork
    {
    }

    public class PublyBizWork : PublyWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyBizVarWork>();
        }
    }
}