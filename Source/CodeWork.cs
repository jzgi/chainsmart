using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

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
            h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name);

            h.SPAN((Code.Typs[o.typ]), "uk-badge");
            h._HEADER();

            h.Q(o.nstart, "uk-width-expand");
            // h.FOOTER_().SPAN(o.nend).SPAN_("uk-margin-auto-left").T(o.bal)._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

/// <summary>
/// To search for traceability tags.
/// </summary>
public class PublyCodeWork : CodeWork<PublyCodeVarWork>
{
    public async Task @default(WebContext wc, int tracenum)
    {
        using var dc = NewDbContext();

        int itemid = 0;
        int srcid = 0;
        if (await dc.QueryTopAsync("SELECT itemid, srcid FROM lotops WHERE nend >= @1 AND nstart <= @1 ORDER BY nend ASC LIMIT 1", p => p.Set(tracenum)))
        {
            dc.Let(out itemid);
            dc.Let(out srcid);
        }

        if (itemid > 0)
        {
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE id = @1");
            var itm = await dc.QueryTopAsync<Item>(p => p.Set(itemid));

            if (itm == null)
            {
                wc.GivePage(200, h => { h.ALERT("无效的溯源产品批次"); });
                return;
            }

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

                h.ShowLot(itm, src, false, true, tracenum);

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


    static readonly string[] TAGS = { "tag", "label" };

    [Ui("硬质标签")]
    public void tag(WebContext wc)
    {
        int num = 0;
        wc.GivePage(200, h =>
        {
            h.TOPBAR_("uk-padding-left").SUBNAV(TAGS)._TOPBAR();
            //
            h.FORM_("uk-card uk-card-primary").FIELDSUL_("预制的硬质标签");
            h.LI_().NUMBER("溯源编号", nameof(num), num)._LI();
            h.LI_("uk-flex-center").BUTTON("查询")._LI();
            h._FIELDSUL();
            h._FORM();
        }, shared: true, 120, title: "产品批次溯源查询");
    }

    [Ui("纸质贴标")]
    public void label(WebContext wc)
    {
        int num = 0;
        wc.GivePage(200, h =>
        {
            h.TOPBAR_("uk-padding-left").SUBNAV(TAGS)._TOPBAR();
            //
            h.FORM_("uk-card uk-card-primary").FIELDSUL_("印制的纸质贴标");
            h.LI_().NUMBER("溯源编号", nameof(num), num)._LI();
            h.LI_("uk-flex-center").BUTTON("查询")._LI();
            h._FIELDSUL();
            h._FORM();
        }, shared: true, 120, title: "产品批次溯源查询");
    }
}

[MgtAuthorize(Org.TYP_SRC)]
[Ui("溯源码申领")]
public class SuplyCodeWork : CodeWork<SuplyCodeVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM tags WHERE orgid = @1 AND (status = 1 OR status = 2) LIMIT 20 OFFSET @1");
        var arr = await dc.QueryAsync<Code>(p => p.Set(org.id).Set(page * 20));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);
            if (arr == null)
            {
                h.ALERT("尚无新的溯源码申请");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }

    [Ui(tip: "已接收", icon: "check", status: 2), Tool(Anchor)]
    public async Task ok(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM tags WHERE orgid = @1 AND status = 0 ORDER BY oked DESC");
        var arr = await dc.QueryAsync<Code>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无已接收的申请");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }


    [Ui(tip: "已作废", icon: "trash", status: 4), Tool(Anchor)]
    public async Task @void(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM tags WHERE orgid = @1 AND status = 0 ORDER BY oked DESC");
        var arr = await dc.QueryAsync<Code>(p => p.Set(org.id).Set(page * 20));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无已拒绝的申请");
                return;
            }

            MainGrid(h, arr);
        }, false, 12);
    }

    [Ui("新建", tip: "新建溯源码申请", icon: "plus", status: 1), Tool(ButtonOpen)]
    public async Task @new(WebContext wc)
    {
        var prin = (User)wc.Principal;
        var org = wc[-1].As<Org>();

        var o = new Code
        {
            name = org.name,
            typ = org.tagtyp,
            orgid = org.id,
            created = DateTime.Now,
            creator = prin.name,
            status = 1,
        };

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();
                h.LI_().FIELD("码类型", Code.Typs[o.typ])._LI();
                h.LI_().NUMBER("码个数", nameof(o.num), o.num, @readonly: true)._LI();
                h._FIELDSUL();
                h.BOTTOMBAR_().BUTTON("确认", nameof(@new), 2)._BOTTOMBAR();
                h._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            o.Read(f);

            using var dc = NewDbContext();
            dc.Sql("INSERT INTO tags ").colset(Code.Empty)._VALUES_(o);
            await dc.ExecuteAsync(p => o.Write(p));

            wc.GivePane(200); // ok
        }
    }
}

[MgtAuthorize(0, User.ROL_OPN)]
[Ui("溯源码发放")]
public class AdmlyCodeWork : CodeWork<AdmlyCodeVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM tags WHERE status = 1 LIMIT 20 OFFSET @1");
        var arr = await dc.QueryAsync<Code>(p => p.Set(page * 20));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);
            if (arr == null)
            {
                h.ALERT("尚无新的申请");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }

    [Ui(tip: "已发放", icon: "arrow-right", status: 2), Tool(Anchor)]
    public async Task sent(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Code.Empty).T(" FROM tags WHERE (status = 2 OR status = 4) LIMIT 20 OFFSET @1");
        var arr = await dc.QueryAsync<Code>(p => p.Set(page * 20));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 2);
            if (arr == null)
            {
                h.ALERT("尚无发放的溯源码");
                return;
            }
            MainGrid(h, arr);
            h.PAGINATION(arr.Length == 20);
        }, false, 12);
    }
}