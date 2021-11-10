using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public class BizlyPostVarWork : WebWork
    {
        [Ui("✎", "✎ 修改", @group: 2), Tool(AnchorShow)]
        public async Task upd(WebContext wc)
        {
        }
    }
}