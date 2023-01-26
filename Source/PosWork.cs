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
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Ware.Empty).T(" FROM wares WHERE shpid = @1 AND status = 4 ORDER BY id DESC");
            var arr = await dc.QueryAsync<int, Ware>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                int wareid = 0;
                decimal price = 0;
                decimal qty = 0, qtyx = 0;
                h.BOTTOMBAR_(large: true);

                h.FORM_(css: "uk-width-expand").FIELDSUL_();
                h.LI_().SELECT(null, nameof(wareid), wareid, arr, required: true).NUMBER(null, nameof(price), price)._LI();
                h.LI_();
                h.SELECT_(null, nameof(qtyx))._SELECT();
                h.NUMBER(null, nameof(qty), qty);
                h._LI();
                h._FIELDSUL()._FORM();

                decimal topay = 0;

                h.BUTTON_("", css: "uk-button-danger uk-width-medium uk-height-1-1", onclick: "return call_buy(this);").CNYOUTPUT(nameof(topay), topay)._BUTTON();

                h._BOTTOMBAR();
            }, false, 60);
        }

        [Ui(tip: "消费记录", icon: "list", group: 2), Tool(Anchor)]
        public async Task list(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE shpid = @1 AND typ = 1 AND status = 4 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("尚无消费记录");
                    return;
                }

                h.TABLE(arr, o =>
                {
                    h.TD(o.created);
                    h.TD(o.topay);
                });
            }, false, 6);
        }

        [Ui(tip: "已作废", icon: "trash", group: 4), Tool(Anchor)]
        public async Task aborted(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE shpid = @1 AND typ = 1 AND status = 8 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("尚无消费记录");
                }
            });
        }
    }
}