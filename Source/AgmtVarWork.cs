using System.Threading.Tasks;
using ChainFx.Web;

namespace ChainSmart;

public class AgntVarWork : WebWork
{
}

public class PublyAgmtVarWork : AgntVarWork
{
}

[Ui("用户协议", "账号功能")]
public class MyAgmtVarWork : AgntVarWork
{
    [Ui("￥", "微信领款"), Tool(Modal.ButtonOpen)]
    public async Task @default(WebContext wc, int dt)
    {
        int orderid = wc[0];
        if (wc.IsGet)
        {
        }
        else // POST
        {
            wc.GivePane(200); // close
        }
    }
}