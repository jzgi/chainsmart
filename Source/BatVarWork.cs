using System;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

namespace ChainSmart;

public abstract class BatVarWork : WebWork
{
    public virtual async Task @default(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM tags WHERE id = @1 AND parentid = @2");
        var o = await dc.QueryTopAsync<Code>(p => p.Set(id).Set(org.id));

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("用户名", o.name)._LI();
            h.LI_().FIELD("受检商户", o.name)._LI();
            h.LI_().FIELD("说明", o.tip)._LI();
            h.LI_().FIELD("身份证号", o.nstart)._LI();
            h.LI_().FIELD("民生卡号", o.nend)._LI();

            h.LI_().FIELD("状态", o.status, Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "生效", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }
}

public class PublyBatVarWork : BatVarWork
{
    static readonly string
        SrcUrl = MainApp.WwwUrl + "/org/",
        LotUrl = MainApp.WwwUrl + "/lot/";

    public override async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();

        dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE id = @1");
        var o = await dc.QueryTopAsync<Bat>(p => p.Set(id));
        if (o == null)
        {
            wc.GivePage(200, h => { h.ALERT("无效的补仓单号"); });
            return;
        }

        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE id = @1");
        var itm = await dc.QueryTopAsync<Item>(p => p.Set(o.itemid));

        Org src = null;
        if (itm.srcid > 0)
        {
            src = GrabTwin<int, Org>(itm.srcid);
        }

        wc.GivePage(200, h =>
            {
                h.TOPBARXL_();

                h.HEADER_("uk-width-expand uk-col uk-padding-small-left").H1(itm.name, css: "h1-lot")._HEADER();
                if (itm.icon)
                {
                    h.PIC("/lot/", itm.id, "/icon", circle: true, css: "uk-width-small");
                }
                else
                {
                    h.PIC("/void.webp", circle: true, css: "uk-width-small");
                }
                h._TOPBARXL();


                h.ARTICLE_("uk-card uk-card-primary");
                h.H2("产品信息", "uk-card-header");
                h.SECTION_("uk-card-body");
                if (itm.pic)
                {
                    h.PIC(LotUrl, itm.id, "/pic", css: "uk-width-1-1");
                }
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品名", itm.name)._LI();

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
                    if (src.m4)
                    {
                        h.PIC(SrcUrl, src.id, "/m-4", css: "uk-width-1-1 uk-padding-bottom");
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


                // h.LI_().LABEL("本溯源码").SPAN($"{tracenum:0000 0000}", css: "uk-static uk-text-danger")._LI();
                // if (o.TryGetStockOp(offset, out var v))
                // {
                //     h.LI_().LABEL("生效日期").SPAN(v.dt, css: "uk-static uk-text-danger")._LI();
                // }
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

                h._SECTION();
                h._ARTICLE();


                h.FOOTER_("uk-col uk-flex-middle uk-padding-large");
                h.SPAN("金中关（北京）信息技术研究院", css: "uk-padding-small");
                h.SPAN("江西同其成科技有限公司", css: "uk-padding-small");
                h._FOOTER();
            }, true, 3600, title:
            "中惠农通产品溯源信息");
    }
}

public class RtllyBatVarWork : BatVarWork
{
    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改或调整消息", icon: "pencil", status: 1 | 2 | 4), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
    }
}

public class SuplyBatVarWork : BatVarWork
{
    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改或调整消息", icon: "pencil", status: 1 | 2 | 4), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE id = @1");
            var o = await dc.QueryTopAsync<Bat>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("消息类型", nameof(o.typ), o.typ, Bat.Typs)._LI();
                h.LI_().TEXT("标题", nameof(o.name), o.name, max: 12)._LI();
                // h.LI_().TEXTAREA("内容", nameof(o.content), o.content, max: 300)._LI();
                h.LI_().TEXTAREA("注解", nameof(o.tip), o.tip, max: 40)._LI();
                // h.LI_().SELECT("级别", nameof(o.rank), o.rank, Lotop.Ranks)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            var m = await wc.ReadObjectAsync(msk, new Bat
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE bats ")._SET_(Bat.Empty, msk).T(" WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(id).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }


    [MgtAuthorize(0, User.ROL_MGT)]
    [Ui("发布", "安排发布", status: 1 | 2 | 4), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE lotops SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4 RETURNING ").collst(Bat.Empty);
        var o = await dc.QueryTopAsync<Bat>(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        // org.EventPack.AddMsg(o);

        wc.GivePane(200);
    }
}

public class HublyBatVarWork : BatVarWork
{
}