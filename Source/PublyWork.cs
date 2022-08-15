using CoChain.Web;

namespace CoSupply
{
    public class PublyWork : WebWork
    {
    }

    public class PublyShpWork : PublyWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyBizVarWork>();
        }
    }
}