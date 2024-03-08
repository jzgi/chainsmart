using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

namespace ChainSmart;

public abstract class AidWork<V> : WebWork where V : AidVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Aid> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name);

            // h.SPAN((Flow.Levels[o.level]), "uk-badge");
            h._HEADER();

            var org = GrabTwin<int, Org>(o.orgid);
            h.Q(org.name, "uk-width-expand");
            h.FOOTER_().SPAN2(o.tip, o.dataid).SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

[MgtAuthorize(0, User.ROL_MGT)]
[Ui("协助")]
public class AdmlyAidWork : AidWork<AdmlyAidVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Aid.Empty).T(" FROM aids WHERE orgid IS NULL AND status = 1");
        var arr = await dc.QueryAsync<Aid>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);
            if (arr == null)
            {
                h.ALERT("尚无新的协助任务");
                return;
            }
            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已处理", icon: "check", status: 2), Tool(Anchor)]
    public async Task adapted(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Aid.Empty).T(" FROM aids WHERE orgid IS NULL AND status = 4");
        var arr = await dc.QueryAsync<Aid>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 2);

            if (arr == null)
            {
                h.ALERT("尚无已处理的协助任务");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已作废", icon: "trash", status: 4), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Aid.Empty).T(" FROM aids WHERE orgid IS NULL AND status = 0");
        var arr = await dc.QueryAsync<Aid>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 4);
            if (arr == null)
            {
                h.ALERT("尚无已否决的协助任务");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    public async Task lst(WebContext wc)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Aid.Empty).T(" FROM aids WHERE parentid = @1 AND status = 4 ORDER BY typ");
        var arr = await dc.QueryAsync<Aid>(p => p.Set(mkt.id));

        const int PAGESIZ = 5;

        wc.GivePage(200, h =>
        {
            if (arr == null)
            {
                h.DIV_(css: "uk-position-center uk-text-xlarge uk-text-success").T("当前无检测记录公示")._DIV();
                return;
            }

            h.T("<main uk-slider=\"autoplay: true; autoplay-interval: 6000; center: true;\">");
            h.UL_("uk-slider-items uk-grid uk-child-width-1-1");

            short lasttyp = 0;
            int num = 0;

            foreach (var o in arr)
            {
                if (o.typ != lasttyp || num % PAGESIZ == 0)
                {
                    if (o.typ != lasttyp)
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
                    h.THEAD_().TH(Aid.Typs[o.typ]).TH("分值", css: "uk-width-medium uk-text-center").TH("结论", css: "uk-width-large uk-text-center")._THEAD();
                }

                // each row
                //
                h.TR_();
                h.TD_().T(o.name);
                if (o.orgid > 0)
                {
                    var org = GrabTwin<int, Org>(o.orgid);
                    h.SP().T('-').SP().T(org.name);
                }
                h._TD();
                h.TD(o.dataid, right: null);
                // h.TD(Flow.Levels[o.level]);
                h._TR();

                num++;

                lasttyp = o.typ;
            }

            h._TABLE();
            h._LI();
            h._UL();
            h._MAIN();
        }, false, 12, title: mkt.Cover, refresh: 60);
    }
}

[MgtAuthorize(Org.TYP_MKT)]
[Ui("协助")]
public class MktlyAidWork : AidWork<MktlyAidVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Aid.Empty).T(" FROM aids WHERE orgid = @1 AND status = 1");
        var arr = await dc.QueryAsync<Aid>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);
            if (arr == null)
            {
                h.ALERT("尚无新的协助任务");
                return;
            }
            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已处理", icon: "check", status: 2), Tool(Anchor)]
    public async Task adapted(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Aid.Empty).T(" FROM aids WHERE orgid IS NULL AND status = 4");
        var arr = await dc.QueryAsync<Aid>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 2);

            if (arr == null)
            {
                h.ALERT("尚无已处理的协助任务");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已作废", icon: "trash", status: 4), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Aid.Empty).T(" FROM aids WHERE orgid IS NULL AND status = 0");
        var arr = await dc.QueryAsync<Aid>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 4);
            if (arr == null)
            {
                h.ALERT("尚无已否决的协助任务");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }
}