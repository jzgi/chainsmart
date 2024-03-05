using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;

namespace ChainSmart;

public abstract class SymWork<V> : WebWork where V : WebWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }
}

public class PublySymWork : SymWork<PublySymVarWork>
{
    public async Task @default(WebContext wc, int tracenum)
    {
        using var dc = NewDbContext();

        dc.Sql("SELECT lotid FROM lotops WHERE nend >= @1 AND nstart <= @1 ORDER BY nend ASC LIMIT 1");
        if (await dc.QueryTopAsync(p => p.Set(tracenum)))
        {
            dc.Let(out int lotid);

            const short Msk = 0xff | Entity.MSK_AUX;

            dc.Sql("SELECT ").collst(Lot.Empty, Msk).T(" FROM lots_vw WHERE id = @1");
            var lot = await dc.QueryTopAsync<Lot>(p => p.Set(lotid), Msk);

            if (lot == null)
            {
                wc.GivePage(200, h => { h.ALERT("无效的溯源产品批次"); });
                return;
            }

            Org src = null;
            if (lot.srcid > 0)
            {
                src = GrabTwin<int, Org>(lot.srcid);
            }

            wc.GivePage(200, h =>
            {
                h.TOPBARXL_();

                h.HEADER_("uk-width-expand uk-col uk-padding-small-left").H1(lot.name, css: "h1-lot")._HEADER();
                if (lot.icon)
                {
                    h.PIC("/lot/", lot.id, "/icon", circle: true, css: "uk-width-small");
                }
                else
                    h.PIC("/void.webp", circle: true, css: "uk-width-small");

                h._TOPBARXL();

                LotVarWork.ShowLot(h, lot, src, false, true, tracenum);

                h.FOOTER_("uk-col uk-flex-middle uk-padding-large");
                h.SPAN("金中关（北京）信息技术研究院", css: "uk-padding-small");
                h.SPAN("江西同其成科技有限公司", css: "uk-padding-small");
                h._FOOTER();
            }, true, 3600, title: "中惠农通产品溯源信息");
        }
        else
        {
            wc.GivePage(300, h => h.ALERT("此溯源码没有绑定产品"));
        }
    }
}

[Ui("标符")]
public class AdmlySymWork : SymWork<AdmlySymVarWork>
{
    static void MainGrid(HtmlBuilder h, IList<Sym> arr)
    {
    }

    [Ui(status: 1), Tool(Modal.Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Sym.Empty).T(" FROM syms WHERE status = 2 LIMIT 20 OFFSET @1");
        var arr = await dc.QueryAsync<Sym>(p => p.Set(20 & page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);

            if (arr == null)
            {
                h.ALERT("尚无新的标符");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }
}