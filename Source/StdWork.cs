using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.ToolAttribute;

namespace ChainSmart;

public abstract class StdWork<V> : WebWork where V : StdVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }


    protected static void Show<M>(HtmlBuilder h, Map<short, M> map) where M : Std
    {
        h.LIST(map, ety =>
        {
            var o = ety.Value;
            h.T(o.name);
        });
    }

    protected static void MainGrid(HtmlBuilder h, Std[] arr, int sub, string title)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/-", sub, MOD_OPEN, false, tip: title, css: "uk-card-body uk-flex");

            h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name).SPAN(Entity.Statuses[o.status], "uk-badge")._HEADER();
            h.Q(o.tip, "uk-width-expand");
            h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

public class PublyCatWork : StdWork<PublyCatVarWork>
{
    [Ui("品类", status: 1), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        var map = Grab<short, Cat>();

        wc.GivePane(200, h => { Show(h, map); }, false, 15);
    }
}

public class PublyEnvWork : StdWork<PublyEnvVarWork>
{
    [Ui("品类", status: 1), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        var map = Grab<short, Env>();

        wc.GivePane(200, h => { Show(h, map); }, false, 15);
    }
}

public class PublyTagWork : StdWork<PublyTagVarWork>
{
    [Ui("品类", status: 1), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        var map = Grab<short, Tag>();

        wc.GivePane(200, h => { Show(h, map); }, false, 15);
    }
}

public class PublySymWork : StdWork<PublySymVarWork>
{
    [Ui("品类", status: 1), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        var map = Grab<short, Sym>();

        wc.GivePane(200, h => { Show(h, map); }, false, 15);
    }
}

[Ui("标准项")]
public class AdmlyStdWork : StdWork<AdmlyStdVarWork>
{
    [Ui("品类", status: 1), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Std.Empty).T(" FROM cats ORDER BY typ, status DESC");
        var arr = dc.Query<Std>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);

            if (arr == null)
            {
                h.ALERT("尚无定义品类");
                return;
            }

            MainGrid(h, arr, 1, "品类");
        }, false, 15);
    }

    [Ui("环境", status: 2), Tool(Anchor)]
    public void env(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Std.Empty).T(" FROM envs ORDER BY typ, status DESC");
        var arr = dc.Query<Std>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 2);

            if (arr == null)
            {
                h.ALERT("尚无定义环境");
                return;
            }

            MainGrid(h, arr, 2, "环境");
        }, false, 15);
    }

    [Ui("溯源", status: 4), Tool(Anchor)]
    public void tag(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Std.Empty).T(" FROM tags ORDER BY typ, status DESC");
        var arr = dc.Query<Std>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 3);

            if (arr == null)
            {
                h.ALERT("尚无定义溯源");
                return;
            }

            MainGrid(h, arr, 3, "溯源");
        }, false, 15);
    }

    [Ui("标符", status: 8), Tool(Anchor)]
    public void sym(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Std.Empty).T(" FROM syms ORDER BY typ, status DESC");
        var arr = dc.Query<Std>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 4);

            if (arr == null)
            {
                h.ALERT("尚无定义标符");
                return;
            }

            MainGrid(h, arr, 4, "标符");
        }, false, 15);
    }

    [Ui("认证", status: 16), Tool(Anchor)]
    public void prog(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Std.Empty).T(" FROM cers ORDER BY typ, status DESC");
        var arr = dc.Query<Std>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 5);

            if (arr == null)
            {
                h.ALERT("尚无定义认证");
                return;
            }

            MainGrid(h, arr, 5, "认证");
        }, false, 15);
    }

    [Ui("新建", "创建标准项", icon: "plus", status: 255), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int sub)
    {
        var prin = (User)wc.Principal;
        var o = new Std
        {
            typ = (short)sub,
            created = DateTime.Now,
            creator = prin.name,
        };
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(Std.TitleOf(sub) + "属性");
                h.LI_().NUMBER("编号", nameof(o.typ), o.typ, min: 1, max: 99, required: true)._LI();
                h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, min: 2, max: 40)._LI();
                h.LI_().NUMBER("排序", nameof(o.idx), o.idx, min: 1, max: 99)._LI();
                h.LI_().SELECT("风格", nameof(o.style), o.style, Std.Styles)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new), subscript: sub)._FORM();
            });
        }
        else // POST
        {
            const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;

            o = await wc.ReadObjectAsync(msk, instance: o);
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO ").T(Std.DbTableOf(sub)).T(" ").colset(Std.Empty, msk)._VALUES_(Std.Empty, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

[Ui("认证")]
public class MktlyCerWork : StdWork<MktlyProgVarWork>
{
    [Ui(status: 1), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        
    }
}