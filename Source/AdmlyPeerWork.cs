using SkyChain.Db;
using SkyChain.Web;

namespace Zhnt
{
    [UserAuthorize(admly: 1)]
    [Ui("联盟友节点管理")]
    public class AdmlyPeerWork : ChainPeerWork
    {
    }
}