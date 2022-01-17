using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    public class OrglyReportWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_PRV, 1)]
    [Ui("供应｜供应业务报告")]
    public class PrvlyReportWork : OrglyReportWork
    {
        public async Task @default(WebContext wc)
        {
        }
    }

    [UserAuthorize(Org.TYP_MRT, 1)]
    [Ui("［市场］运营报告")]
    public class MrtlyReportWork : OrglyReportWork
    {
        public async Task @default(WebContext wc)
        {
        }
    }
}