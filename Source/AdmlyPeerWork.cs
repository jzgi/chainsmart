using SkyChain.Db;
using SkyChain.Web;

namespace Zhnt.Supply
{
    [UserAuthorize(admly: User.ADMLY_IT)]
    [Ui("联盟节点", "⁂")]
    public class AdmlyPeerWork : ChainPeerWork
    {
    }
}