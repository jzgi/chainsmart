using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;
using static ChainSmart.MainUtility;

namespace ChainSmart;

public abstract class CodeVarWork : WebWork
{
    public virtual async Task @default(WebContext wc)
    {
        int id = wc[0];
        var tags = Grab<short, Tag>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE id = @1");
        var o = await dc.QueryTopAsync<Code>(p => p.Set(id));

        var org = wc[-2].As<Org>() ?? GrabTwin<int, Org>(o.orgid);

        wc.GivePane(200, h =>
        {
            h.H4("基本", css: "uk-padding");
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("档号", o.id, digits: 5)._LI();
            h.LI_().FIELD("名称", o.name).FIELD("类型", o.typ, tags)._LI();
            h.LI_().FIELD("申请方", org.name).FIELD("申请个数", o.num)._LI();
            h.LI_().FIELD("备注", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
            h.LI_().FIELD("起始码", o.nstart, digits: 7).FIELD("截止码", o.nend, digits: 7)._LI();
            h._UL();

            h.H4("状态", css: "uk-padding");
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("状态", o.status, Code.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("提交", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "发放", o.oker, o.oked, sep: "<br>")._LI();
            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }
}

public class PublyCodeVarWork : CodeVarWork
{
    public override async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();

        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE id = @1");
        var o = await dc.QueryTopAsync<Code>(p => p.Set(id));
        if (o == null)
        {
            wc.GivePage(200, h => { h.ALERT("无效的补仓单号"); });
            return;
        }

        Org src = null;

        wc.GivePage(200, h =>
            {
                h.TOPBARXL_();

                h.HEADER_("uk-width-expand uk-col uk-padding-small-left").H1(src.name, css: "h1-lot")._HEADER();
                if (src.icon)
                {
                    // h.PIC(ItemUrl, itm.id, "/icon", circle: true, css: "uk-width-small");
                }
                else
                {
                    h.PIC("/void.webp", circle: true, css: "uk-width-small");
                }
                h._TOPBARXL();


                h.ARTICLE_("uk-card uk-card-primary");
                h.H2("产品信息", "uk-card-header");
                h.SECTION_("uk-card-body");

                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品名", src.name)._LI();

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
                        h.PIC(OrgUrl, src.id, "/pic", css: "uk-width-1-1 uk-padding-bottom");
                    }
                    if (src.m1)
                    {
                        h.PIC(OrgUrl, src.id, "/m-1", css: "uk-width-1-1 uk-padding-bottom");
                    }
                    if (src.m2)
                    {
                        h.PIC(OrgUrl, src.id, "/m-2", css: "uk-width-1-1 uk-padding-bottom");
                    }
                    if (src.m3)
                    {
                        h.PIC(OrgUrl, src.id, "/m-3", css: "uk-width-1-1 uk-padding-bottom");
                    }
                    if (src.m4)
                    {
                        h.PIC(OrgUrl, src.id, "/m-4", css: "uk-width-1-1 uk-padding-bottom");
                    }
                }
                h._SECTION();
                h._ARTICLE();

                h.ARTICLE_("uk-card uk-card-primary");
                h.H2("批次检验", "uk-card-header");
                h.SECTION_("uk-card-body");

                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("批次编号", src.id, digits: 8)._LI();
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

                if (src.m1)
                {
                    h.PIC(ItemUrl, src.id, "/m-1", css: "uk-width-1-1 uk-padding-bottom");
                }
                if (src.m2)
                {
                    h.PIC(ItemUrl, src.id, "/m-2", css: "uk-width-1-1 uk-padding-bottom");
                }
                if (src.m3)
                {
                    h.PIC(ItemUrl, src.id, "/m-3", css: "uk-width-1-1 uk-padding-bottom");
                }
                if (src.m4)
                {
                    h.PIC(ItemUrl, src.id, "/m-4", css: "uk-width-1-1 uk-padding-bottom");
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

public class SrclyCodeVarWork : CodeVarWork
{
    [MgtAuthorize(Org.TYP_SRC, User.ROL_MGT)]
    [Ui(tip: "修改申请", icon: "pencil", status: 1), Tool(ButtonShow)]
    public async Task upd(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE id = @1");
            var o = await dc.QueryTopAsync<Code>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().NUMBER("申请数量", nameof(o.num), o.num)._LI();
                h.LI_().TEXT("收件地址", nameof(o.addr), o.addr, max: 40)._LI();
                h.LI_().TEXTAREA("备注", nameof(o.tip), o.tip, max: 30)._LI();

                h._FIELDSUL().BOTTOMBAR_().BUTTON("确认", nameof(upd))._BOTTOMBAR();
                h._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            int num = f[nameof(num)];
            string addr = f[nameof(addr)];
            string tip = f[nameof(tip)];

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE codes SET num = @1, addr = @2, tip = @3 WHERE id = @4 AND orgid = @5");
            await dc.ExecuteAsync(p => p.Set(num).Set(addr).Set(tip).Set(id).Set(org.id));

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(Org.TYP_SRC, User.ROL_MGT)]
    [Ui("提交", "提交申请", status: 1), Tool(ButtonConfirm)]
    public async Task adapt(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE codes SET status = 2, adapted = @1, adapter = @2 WHERE id = @3 AND orgid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [MgtAuthorize(Org.TYP_SRC, User.ROL_MGT)]
    [Ui("撤回", "撤回申请", status: 2), Tool(ButtonConfirm)]
    public async Task unadapt(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE codes SET status = 1, adapted = NULL, adapter = NULL WHERE id = @3 AND orgid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.GivePane(200);
    }
}

public class AdmlyCodeVarWork : CodeVarWork
{
    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui("发放", "发放溯源码", status: 2), Tool(ButtonShow)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE id = @1");
            var o = await dc.QueryTopAsync<Code>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().NUMBER("起始码", nameof(o.nstart), o.nstart)._LI();
                h.LI_().NUMBER("截止码", nameof(o.nend), o.nend)._LI();
                h.LI_().TEXT("运单号", nameof(o.ship), o.ship, max: 12)._LI();

                h._FIELDSUL().BOTTOMBAR_().BUTTON("确认", nameof(ok))._BOTTOMBAR();
                h._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();

            int nstart = f[nameof(nstart)];
            int nend = f[nameof(nend)];
            string ship = f[nameof(ship)];

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE codes SET status = 4, oked = @1, oker = @2, nstart = @3, nend = @4, ship = @5 WHERE id = @6 AND status = 1");
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(nstart).Set(nend).Set(ship).Set(id));

            wc.GivePane(200); // close dialog
        }
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui("撤回", tip: "撤回发放", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("UPDATE codes SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND status = 1");
        await dc.ExecuteAsync(p => p.Set(id));

        wc.GivePane(200);
    }
}