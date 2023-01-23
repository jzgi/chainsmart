using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public abstract class PosWork<V> : WebWork where V : PosVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    [OrglyAuthorize(Org.TYP_SHP, User.ROL_OPN)]
    [Ui("线下消费终端", "商户")]
    public class ShplyPosWork : PosWork<ShplyPosVarWork>
    {
        [Ui("消费终端", group: 1), Tool(Anchor)]
        public void @default(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.ALERT("限在智能秤上使用");
            });
        }

        [Ui(tip: "消费记录", icon: "list", group: 2), Tool(Anchor)]
        public async Task list(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE shpid = @1 AND typ = 1 AND status = 1 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));


            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("限在智能秤上使用");
                    
                }
            });
        }
    }
}