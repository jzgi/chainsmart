using ChainFX.Web;

namespace ChainSmart;

public static class ItemUtility
{
    static readonly string
        SrcUrl = MainApp.WwwUrl + "/org/",
        LotUrl = MainApp.WwwUrl + "/lot/";


    internal static void ShowLot(this HtmlBuilder h, Item itm, Org src, bool pricing, bool linking, int tracenum = 0)
    {
        h.ARTICLE_("uk-card uk-card-primary");
        h.H2("产品信息", "uk-card-header");
        h.SECTION_("uk-card-body");
        if (itm.pic)
        {
            h.PIC(LotUrl, itm.id, "/pic", css: "uk-width-1-1");
        }
        h.UL_("uk-list uk-list-divider");
        h.LI_().FIELD("产品名", itm.name)._LI();

        if (pricing)
        {
            h.LI_().FIELD("单位", itm.unit).FIELD("附注", itm.unitip, itm.unitip)._LI();
            h.LI_().FIELD2("整件", itm.unitx, itm.unit)._LI();
            h.LI_().FIELD("单价", itm.price, true).FIELD("优惠立减", itm.off, true)._LI();
            h.LI_().FIELD("起订件数", itm.min).FIELD("限订件数", itm.max)._LI();
        }

        h._UL();

        if (src != null)
        {
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("产源设施", src.name)._LI();
            h.LI_().FIELD(string.Empty, src.tip)._LI();
            // h.LI_().FIELD("等级", src.rank, Src.Ranks)._LI();
            h._UL();

            if (src.tip != null)
            {
                h.ALERT_().T(src.tip)._ALERT();
            }
            if (src.pic)
            {
                h.PIC(SrcUrl, src.id, "/pic", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (src.m1)
            {
                h.PIC(SrcUrl, src.id, "/m-1", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (src.m2)
            {
                h.PIC(SrcUrl, src.id, "/m-2", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (src.m3)
            {
                h.PIC(SrcUrl, src.id, "/m-3", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (src.scene)
            {
                h.PIC(SrcUrl, src.id, "/m-scene", css: "uk-width-1-1 uk-padding-bottom");
            }
        }
        h._SECTION();
        h._ARTICLE();

        h.ARTICLE_("uk-card uk-card-primary");
        h.H2("批次检验", "uk-card-header");
        h.SECTION_("uk-card-body");

        h.UL_("uk-list uk-list-divider");
        h.LI_().FIELD("批次编号", itm.id, digits: 8)._LI();
        // if (o.steo > 0 && o.nend > 0)
        // {
        //     h.LI_().FIELD2("批次溯源码", $"{o.steo:0000 0000}", $"{o.nend:0000 0000}", "－")._LI();
        // }

        var offset = tracenum - itm.step;
        if (offset > 0)
        {
            h.LI_().LABEL("本溯源码").SPAN($"{tracenum:0000 0000}", css: "uk-static uk-text-danger")._LI();
            // if (o.TryGetStockOp(offset, out var v))
            // {
            //     h.LI_().LABEL("生效日期").SPAN(v.dt, css: "uk-static uk-text-danger")._LI();
            // }
        }
        h._LI();
        h._UL();

        if (itm.m1)
        {
            h.PIC(LotUrl, itm.id, "/m-1", css: "uk-width-1-1 uk-padding-bottom");
        }
        if (itm.m2)
        {
            h.PIC(LotUrl, itm.id, "/m-2", css: "uk-width-1-1 uk-padding-bottom");
        }
        if (itm.m3)
        {
            h.PIC(LotUrl, itm.id, "/m-3", css: "uk-width-1-1 uk-padding-bottom");
        }
        if (itm.m4)
        {
            h.PIC(LotUrl, itm.id, "/m-4", css: "uk-width-1-1 uk-padding-bottom");
        }

        if (linking)
        {
            h.UL_("uk-list uk-list-divider");
            if (!string.IsNullOrEmpty(itm.link))
            {
                h.LI_("uk-flex-center").A_(itm.link, css: "uk-button uk-icon-link").ICON("file-text").SP().T("合格证")._A()._LI();
            }
            h._UL();
        }

        h._SECTION();
        h._ARTICLE();
    }
}