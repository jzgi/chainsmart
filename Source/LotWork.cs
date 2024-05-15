using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;
using static ChainFX.Web.ToolAttribute;

namespace ChainSmart;

public abstract class LotWork<V> : WebWork where V : LotVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Lot> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC("/void.webp", css: "uk-width-tiny");

            h.ASIDE_();
            h.HEADER_().H4(o.name);

            h.SPAN(Lot.Statuses[o.status], "uk-badge");
            h._HEADER();

            h.Q(o.tip, "uk-width-expand");
            h._ASIDE();

            h._A();
        });
    }
}

[MgtAuthorize(Org.TYP_HUB)]
[Ui("库存")]
public class HublyLotWork : LotWork<HublyLotVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE hubid = @1 AND status > 0 ORDER BY oked DESC limit 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Lot>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无产品仓管请求");
                return;
            }

            MainGrid(h, arr);

            h.PAGINATION(arr?.Length == 20);
        }, false, 6);
    }
}