using SkyChain.Db;
using SkyChain.Web;

namespace Zhnt.Supply
{
    [UserAuthorize(admly: 1)]
    [Ui("联盟节点管理")]
    public class AdmlyPeerWork : ChainPeerWork
    {
    }
}