using ChainFx.Fabric;
using ChainFx.Web;

namespace ChainMart
{
    public class SysWork : WebWork
    {
    }

    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("数据维护", "系统")]
    public class AdmlyDatWork : SysWork
    {
    }

    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("联盟链管理", "系统")]
    public class AdmlyFedWork : FedWork
    {
    }
}