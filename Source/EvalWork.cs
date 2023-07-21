using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart
{
    public abstract class EvalWork<V> : WebWork where V : EvalVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    [Ui("评检")]
    public class OrglyEvalWork : EvalWork<OrglyEvalVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }


    [OrglyAuthorize(Org.TYP_MKT)]
    [Ui("评检管理")]
    public class MktlyEvalWork : EvalWork<MktlyEvalVarWork>
    {
        protected static void MainGrid(HtmlBuilder h, IList<Eval> arr)
        {
            h.MAINGRID(arr, o =>
            {
                h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
                h.PIC("/void.webp", css: "uk-width-1-5");

                h.ASIDE_();
                h.HEADER_().H4(o.name);

                // h.SPAN(Statuses[o.status], "uk-badge");
                h._HEADER();

                h.Q(o.tip, "uk-width-expand");
                h.FOOTER_().SPAN2("未用量", o.level).SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                h._ASIDE();

                h._A();
            });
        }


        [Ui("当前上线", status: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Eval.Empty).T(" FROM evals WHERE orgid = @1 AND status = 4");
            var arr = await dc.QueryAsync<Eval>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无上线检测");
                    return;
                }
                MainGrid(h, arr);
            }, false, 12);
        }

        [Ui(tip: "下线商品", icon: "cloud-download", status: 2), Tool(Anchor)]
        public async Task down(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Eval.Empty).T(" FROM evals WHERE orgid = @1 AND status BETWEEN 1 AND 2 ORDER BY adapted DESC");
            var arr = await dc.QueryAsync<Eval>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无下线检测");
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
            dc.Sql("SELECT ").collst(Eval.Empty).T(" FROM evals WHERE orgid = @1 AND status = 0 ORDER BY adapted DESC");
            var arr = await dc.QueryAsync<Eval>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无作废检测");
                    return;
                }

                MainGrid(h, arr);
            }, false, 4);
        }
    }

    [OrglyAuthorize(Org.TYP_CTR)]
    [Ui("评检管理")]
    public class CtrlyEvalWork : EvalWork<CtrlyEvalVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }
}