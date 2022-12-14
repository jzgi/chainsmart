using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class TagWork : WebWork
    {
    }

    public class PublyTagWork : TagWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyTagVarWork>();
        }
    }
}