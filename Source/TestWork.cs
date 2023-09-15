using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart;

public abstract class TestWork<V> : WebWork where V : TestVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Test> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name);

            h.SPAN((Test.Levels[o.level]), "uk-badge");
            h._HEADER();

            var org = GrabTwin<int, Org>(o.orgid);
            h.Q(org.name, "uk-width-expand");
            h.FOOTER_().SPAN2(o.tip, o.val).SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }


    [Ui("公示检测", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Test.Empty).T(" FROM tests WHERE upperid = @1 AND status = 4");
        var arr = await dc.QueryAsync<Test>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 4);
            if (arr == null)
            {
                h.ALERT("尚无公示记录");
                return;
            }
            MainGrid(h, arr);
        }, false, 12);
    }

    [Ui(tip: "已下线", icon: "cloud-download", status: 2), Tool(Anchor)]
    public async Task adapted(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Test.Empty).T(" FROM tests WHERE upperid = @1 AND status BETWEEN 1 AND 2 ORDER BY adapted DESC");
        var arr = await dc.QueryAsync<Test>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);

            if (arr == null)
            {
                h.ALERT("尚无已下线的检测记录");
                return;
            }

            MainGrid(h, arr);
        }, false, 4);
    }

    [Ui(tip: "已作废", icon: "trash", status: 8), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Test.Empty).T(" FROM tests WHERE upperid = @1 AND status = 0 ORDER BY adapted DESC");
        var arr = await dc.QueryAsync<Test>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无已作废的检测记录");
                return;
            }

            MainGrid(h, arr);
        }, false, 4);
    }

    [Ui("新建", tip: "新建检测记录", icon: "plus", status: 1 | 2), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int stu)
    {
        var prin = (User)wc.Principal;

        var regs = Grab<short, Reg>();
        var org = wc[-1].As<Org>();
        var orgs = GrabTwinSet<int, Org>(org.id);

        var o = new Test
        {
            upperid = org.id,
            created = DateTime.Now,
            creator = prin.name,
            level = 3,
            status = (short)stu
        };
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);
                h.LI_().SELECT("检测类型", nameof(o.typ), o.typ, Test.Typs)._LI();
                h.LI_().TEXT("受检商品", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().LABEL("受检商户").SELECT_ORG(nameof(o.orgid), o.orgid, orgs, regs)._LI();
                h.LI_().TEXTAREA("说明", nameof(o.tip), o.tip, min: 2, max: 20)._LI();
                h.LI_().NUMBER("分值", nameof(o.val), o.val)._LI();
                h.LI_().SELECT("结论", nameof(o.level), o.level, Test.Levels)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;

            o = await wc.ReadObjectAsync(msk, instance: o);
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO tests ").colset(Test.Empty, msk)._VALUES_(Test.Empty, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

[OrglyAuthorize(Org.TYP_MKT)]
[Ui("检测管理")]
public class MktlyTestWork : TestWork<MktlyTestVarWork>
{
    public async Task lst(WebContext wc)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT typ, name, tip, orgid, val, level FROM tests WHERE upperid = @1 AND status = 4 ORDER BY typ");
        await dc.QueryAsync(p => p.Set(mkt.id));

        const int PAGESIZ = 5;

        wc.GivePage(200, h =>
        {
            h.T("<main uk-slider=\"autoplay: true; utoplay-interval: 6000; pause-on-hover: true; center: true\">");
            h.UL_("uk-slider-items uk-grid uk-child-width-1-1");

            short lasttyp = 0;
            int num = 0;

            while (dc.Next())
            {
                dc.Let(out short typ);
                dc.Let(out string name);
                dc.Let(out string tip);
                dc.Let(out int orgid);
                dc.Let(out decimal val);
                dc.Let(out short level);

                if (typ != lasttyp || num % PAGESIZ == 0)
                {
                    if (typ != lasttyp)
                    {
                        num = 0; // reset
                    }
                    if (lasttyp > 0 || (num % PAGESIZ == 0 && num > 0))
                    {
                        h._TABLE();
                        h._LI();
                    }
                    h.LI_();
                    h.TABLE_(dark: true);
                    h.THEAD_().TH(Test.Typs[typ]).TH("分值", css: "uk-width-medium uk-text-center").TH("结论", css: "uk-width-large uk-text-center")._THEAD();
                }

                // each row
                //
                h.TR_();
                h.TD_().T(name);
                if (orgid > 0)
                {
                    var org = GrabTwin<int, Org>(orgid);
                    h.SP().T('-').SP().T(org.name);
                }
                h._TD();
                h.TD(val, right: null);
                h.TD(Test.Levels[level]);
                h._TR();

                num++;

                lasttyp = typ;
            }

            if (num % PAGESIZ == 0 && num > 0)
            {
                h._TABLE();
                h._LI();
            }
            h._UL();

            if (num == 0)
            {
                h.DIV_(css: "uk-position-center uk-text-xlarge uk-text-success").T("当前无检测记录公示")._DIV();
            }

            h._MAIN();
        }, false, 12, title: mkt.Cover, refresh: 60);
    }
}

[OrglyAuthorize(Org.TYP_CTR)]
[Ui("检测管理")]
public class CtrlyTestWork : TestWork<CtrlyTestVarWork>
{
}