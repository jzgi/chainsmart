using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;
using static ChainFX.Web.ToolAttribute;
using static ChainSmart.MainUtility;

namespace ChainSmart;

public abstract class CodeWork<V> : WebWork where V : WebWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Code> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC("/void.webp", css: "uk-width-1-6");

            h.ASIDE_();
            h.HEADER_().H4_().T(o.name).SP().T(o.num).T(" 个")._H4();
            h.SPAN((Code.Statuses[o.status]), "uk-badge");
            h._HEADER();

            h.Q(o.tip, "uk-width-expand");

            var org = GrabTwin<int, Org>(o.orgid);

            h.FOOTER_();
            if (o.nstart > 0 || o.nend > 0)
            {
                h.SPAN_().T(o.nstart, digits: 8).T('-').T(o.nend, digits: 8)._SPAN();
            }
            h.SPAN(org.name, css: "uk-margin-auto-left")._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

public class PublyCodeWork : CodeWork<PublyCodeVarWork>
{
    public async Task @default(WebContext wc, int val)
    {
        var tag = (short)wc[0].Adscript;

        using var dc = NewDbContext();

        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE typ = @1 AND @2 BETWEEN nstart AND nend LIMIT 1");
        var code = await dc.QueryTopAsync<Code>(p => p.Set(tag).Set(val));

        // try to seek the batch that includes the given value
        dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE codeid = @1 AND nend >= @2 LIMIT 1");
        var bat = await dc.QueryTopAsync<Bat>(p => p.Set(code.id).Set(val));


        wc.GivePage(200, h =>
        {
            if (code == null)
            {
                h.ALERT("无效的或逾期的溯源码");
                return;
            }

            var src = GrabTwin<int, Org>(code.orgid);

            h.TOPBARXL_();
            h.HEADER_("uk-width-expand uk-padding-small-left").H1(src.name)._HEADER();
            h.IMG(OrgUrl, src.id, "/icon", circle: true, css: "uk-width-small");
            h._TOPBARXL();

            h.ARTICLE_("uk-card uk-card-primary");

            h.HEADER_("uk-card-header").H3(src.name)._HEADER();
            h.IMG(OrgUrl, src.id, "/pic", css: "uk-width-1-1");

            h.UL_("uk-card-body uk-list uk-list-divider");
            h.LI_().FIELD("简介语", src.tip)._LI();
            h.LI_().FIELD("工商登记", src.legal)._LI();
            h.LI_().FIELD("联系电话", src.tel)._LI();
            h.LI_().FIELD("地址", src.addr)._LI();
            h._UL();

            var cat = Grab<short, Cat>()?[src.cat];
            var sym = Grab<short, Sym>()?[src.sym];
            var tag = Grab<short, Tag>()?[src.tag];
            var env = Grab<short, Env>()?[src.env];

            h.UL_("uk-card-body uk-list uk-list-divider");
            h.LI_().FIELD_("分类");
            if (cat == null)
            {
                h.T("无");
            }
            else
            {
                h.T(cat.name).Q(cat.tip, css: "uk-margin-auto-left");
            }
            h._FIELD()._LI();

            h.LI_().FIELD_("标志");
            if (sym == null)
            {
                h.T("无");
            }
            else
            {
                h.T(sym.name).Q(sym.tip, css: "uk-margin-auto-left");
            }
            h._FIELD()._LI();

            h.LI_().FIELD_("溯源");
            if (tag == null)
            {
                h.T("无");
            }
            else
            {
                h.T(tag.name).Q(tag.tip, css: "uk-margin-auto-left");
            }
            h._FIELD()._LI();

            h.LI_().FIELD_("环境");
            if (env == null)
            {
                h.T("无");
            }
            else
            {
                h.T(env.name).Q(env.tip, css: "uk-margin-auto-left");
            }
            h._FIELD()._LI();

            h._UL();

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

            h._ARTICLE();

            // batch and item info
            if (bat != null)
            {
                h.ARTICLE_("uk-card uk-card-primary");

                h.HEADER_("uk-card-header").H3(src.name)._HEADER();
                h.IMG(OrgUrl, bat.itemid, "/pic", css: "uk-width-1-1");

                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("简介语", bat.name)._LI();
                h.LI_().FIELD("工商登记", src.legal)._LI();
                h.LI_().FIELD("联系电话", src.tel)._LI();
                h.LI_().FIELD("地址", src.addr)._LI();
                h._UL();

                h._ARTICLE();
            }
        }, true, 3600, title: "中惠农通产品溯源信息");
    }
}

[MgtAuthorize(Org.TYP_SRC)]
[Ui("溯源码")]
public class SrclyCodeWork : CodeWork<SrclyCodeVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE orgid = @1 ORDER BY created DESC LIMIT 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Code>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无申请");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }

    [Ui("申请", tip: "新建溯源码申请", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task @new(WebContext wc)
    {
        var prin = (User)wc.Principal;
        var org = wc[-1].As<Org>();
        var tags = Grab<short, Tag>();
        var now = DateTime.Now;


        var o = new Code
        {
            typ = org.tag,
            name = tags[org.tag]?.name,
            created = now,
            creator = prin.name,
            adapted = now,
            adapter = prin.name,
            orgid = org.id,
            status = STU_ADAPTED,
        };

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().FIELD("码类型", o.typ, tags)._LI();
                h.LI_().NUMBER("申请数量", nameof(o.num), o.num)._LI();
                h.LI_().TEXT("收件地址", nameof(o.addr), o.addr, max: 40)._LI();
                h.LI_().TEXTAREA("备注", nameof(o.tip), o.tip, max: 30)._LI();

                h._FIELDSUL().BOTTOMBAR_().BUTTON("确认", nameof(@new), 2)._BOTTOMBAR();
                h._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;

            await wc.ReadObjectAsync(msk, instance: o);

            using var dc = NewDbContext();
            dc.Sql("INSERT INTO codes ").colset(Code.Empty, msk)._VALUES_(o, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // ok
        }
    }
}

[MgtAuthorize(0, User.ROL_OPN)]
[Ui("溯源码")]
public class AdmlyCodeWork : CodeWork<AdmlyCodeVarWork>
{
    [Ui("溯源码申请", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE status = 1 LIMIT 20 OFFSET @1 * 20");
        var arr = await dc.QueryAsync<Code>(p => p.Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);
            if (arr == null)
            {
                h.ALERT("尚无产源提交的申请");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }

    [Ui(tip: "已发放", icon: "mail", status: 2), Tool(Anchor)]
    public async Task adapted(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE status = 2 LIMIT 20 OFFSET @1 * 20");
        var arr = await dc.QueryAsync<Code>(p => p.Set(page * 20));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 2);
            if (arr == null)
            {
                h.ALERT("尚无已发放");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }

    [Ui(tip: "已用完", icon: "check", status: 2), Tool(Anchor)]
    public async Task oked(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM codes WHERE status = 4 LIMIT 20 OFFSET @1 * 20");
        var arr = await dc.QueryAsync<Code>(p => p.Set(page * 20));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 2);
            if (arr == null)
            {
                h.ALERT("尚无已用完");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }
}