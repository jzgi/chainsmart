using ChainFx.Web;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public class DistribWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_CTR, 1)]
    [Ui("产源批发管理")]
    public class SrclyDistribWork : DistribWork
    {
        [Ui("自销", "自行接单，货品整批次发到中枢由其控运", group: 1), Tool(Anchor)]
        public void @default(WebContext wc)
        {
            wc.GivePane(200, h =>
            {
                //
                h.TOOLBAR();
            });
        }

        [Ui("转让", "物权转移给某中枢", group: 2), Tool(Anchor)]
        public void sell(WebContext wc)
        {
            wc.GivePane(200, h =>
            {
                //
                h.TOOLBAR();
            });
        }

        [Ui("自控", "自行接单，货品自行控运直对客户", group: 3), Tool(Anchor)]
        public void direct(WebContext wc)
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
    public class CtrlyDistribWork : DistribWork
    {
        public void @default(WebContext wc)
        {
        }
    }
}