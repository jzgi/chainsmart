using CoChain.Web;

namespace Revital
{
    public class LotWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_CTR, 1)]
    [Ui("产源产品批次管理")]
    public class SrclyLotWork : LotWork
    {
        public void @default(WebContext wc)
        {
            wc.GivePane(200, h =>
            {
                //
                h.TOOLBAR();
            });
        }
    }

    [UserAuthorize(Org.TYP_CTR, 1)]
    [Ui("中枢产品批次管理")]
    public class CtrlyLotWork : LotWork
    {
        public void @default(WebContext wc)
        {
        }
    }
}