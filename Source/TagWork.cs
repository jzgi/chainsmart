﻿using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public abstract class TagWork : WebWork
{
}

public class PublyTagWork : TagWork
{
    protected override void OnCreate()
    {
        CreateVarWork<PublyTagVarWork>();
    }

    public async Task @default(WebContext wc, int tracenum)
    {
        using var dc = NewDbContext();
        
        dc.Sql("SELECT id FROM lots_vw WHERE nend >= @1 AND nstart <= @1 ORDER BY nend ASC LIMIT 1");
        if (await dc.QueryTopAsync(p => p.Set(tracenum)))
        {
            dc.Let(out int lotid);

            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1");
            var lot = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

            if (lot == null)
            {
                wc.GivePage(200, h => { h.ALERT("无效的溯源产品批次"); });
                return;
            }

            var org = GrabTwin<int, Org>(lot.orgid);
            Fab fab = null;
            if (lot.fabid > 0)
            {
                fab = GrabTwin<int, Fab>(lot.fabid);
            }

            wc.GivePage(200, h =>
            {
                h.TOPBARXL_();
                h.HEADER_("uk-width-expand uk-col uk-padding-small-left").H2(lot.name)._HEADER();
                if (lot.icon)
                {
                    h.PIC("/lot/", lot.id, "/icon", circle: true, css: "uk-width-small");
                }
                else
                    h.PIC("/void.webp", circle: true, css: "uk-width-small");

                h._TOPBARXL();

                LotVarWork.LotShow(h, lot, org, fab, false, tracenum);

                h.FOOTER_("uk-col uk-flex-middle uk-margin-large-top uk-margin-bottom");
                h.SPAN("金中关（北京）信息技术研究院", css: "uk-padding-small");
                h.SPAN("江西同其成科技有限公司", css: "uk-padding-small");
                h._FOOTER();
            }, true, 3600, title: "中惠农通产品溯源信息");

            // wc.GiveRedirect("/lot/" + lotid + "/");
        }
        else
        {
            wc.GivePage(304, h => h.ALERT("此溯源码没有绑定产品"));
        }
    }
}