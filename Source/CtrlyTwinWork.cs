using ChainFx.Nodal;
using ChainFx.Web;

namespace ChainSmart;

[OrglyAuthorize(Org.TYP_CTR)]
[Ui("物联动态")]
public class CtrlyTwinWork : TwinWork
{
    public void @default(WebContext wc)
    {
        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
        });
        
    }
}