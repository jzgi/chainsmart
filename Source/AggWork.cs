using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart
{
    public abstract class AggWork<V> : WebWork where V : AggVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }

        protected static void ClearTable(HtmlBuilder h, IEnumerable<Clear> arr)
        {
            // h.TABLE_();
            // var last = 0;
            // foreach (var o in arr)
            // {
            //     if (o.since != last)
            //     {
            //         var spr = GrabObject<int, Org>(o.since);
            //         h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
            //     }
            //     h.TR_();
            //     h.TD(o.till);
            //     h.TD(o.name);
            //     h._TR();
            //
            //     last = o.since;
            // }
            // h._TABLE();
        }
    }


    [AdmlyAuthorize(User.ROL_FIN)]
    [Ui("市场业务日总", "财务")]
    public class AdmlyBuyAggWork : AggWork<AdmlyBuyAggVarWork>
    {
        [Ui("消费业务", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Agg.Empty).T(" FROM buyaggs ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = dc.Query<Agg>(p => p.Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD(Buy.Typs[o.typ]);
                    h.TD_("uk-text-right").T(o.trans)._TD();
                    h.TD_("uk-text-right").CNY(o.amt)._TD();
                }, thead: () =>
                {
                    h.TH("日期", css: "uk-width-medium");
                    h.TH("类型");
                    h.TH("笔数");
                    h.TH("金额");
                });
                h.PAGINATION(arr?.Length == 30);
            }, false, 60);
        }

        [Ui(tip: "历史记录", icon: "history", group: 2), Tool(Anchor)]
        public void past(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Agg.Empty).T(" FROM buyaggs WHERE typ = 1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = dc.Query<Agg>(p => p.Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD(Buy.Typs[o.typ]);
                    h.TD_("uk-text-right").T(o.trans)._TD();
                    h.TD_("uk-text-right").CNY(o.amt)._TD();
                }, thead: () =>
                {
                    h.TH("日期", css: "uk-width-medium");
                    h.TH("类型");
                    h.TH("笔数");
                    h.TH("金额");
                });
                h.PAGINATION(arr?.Length == 30);
            }, false, 60);
        }
    }


    [AdmlyAuthorize(User.ROL_FIN)]
    [Ui("供应业务日总", "财务")]
    public class AdmlyBookAggWork : AggWork<AdmlyBookAggVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [Ui("销售日总", "商户")]
    public class ShplyBuyAggWork : AggWork<ShplyBuyAggVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Agg.Empty).T(" FROM buyaggs WHERE orgid = @1 ORDER BY dt DESC, typ LIMIT 30 OFFSET 30 * @2");
            var arr = dc.Query<Agg>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                if (arr == null)
                {
                    h.ALERT("暂无生成报表");
                    return;
                }

                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD(Buy.Typs[o.typ]);
                    h.TD_("uk-text-right").T(o.trans)._TD();
                    h.TD_("uk-text-right").CNY(o.amt)._TD();
                }, thead: () =>
                {
                    h.TH("日期", css: "uk-width-medium");
                    h.TH("类型");
                    h.TH("笔数");
                    h.TH("金额");
                });
                h.PAGINATION(arr?.Length == 30);
            }, false, 60);
        }
    }

    [Ui("采购日总", "商户")]
    public class ShplyBookAggWork : AggWork<ShplyBookAggVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Agg.Empty).T(" FROM bookaggs WHERE orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = dc.Query<Agg>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                if (arr == null)
                {
                    h.ALERT("暂无生成报表");
                    return;
                }

                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD(Buy.Typs[o.typ]);
                    h.TD_("uk-text-right").T(o.trans)._TD();
                    h.TD_("uk-text-right").CNY(o.amt)._TD();
                }, thead: () =>
                {
                    h.TH("日期", css: "uk-width-medium");
                    h.TH("类型");
                    h.TH("笔数");
                    h.TH("金额");
                });
                h.PAGINATION(arr?.Length == 30);
            }, false, 60);
        }
    }

    [Ui("销售日总", "商户")]
    public class SrclyBookAggWork : AggWork<SrclyBookAggVarWork>
    {
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM bookaggs WHERE orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = await dc.QueryAsync<Agg>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                if (arr == null)
                {
                    h.ALERT("暂无生成报表");
                    return;
                }

                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD(Buy.Typs[o.typ]);
                    h.TD_("uk-text-right").T(o.trans)._TD();
                    h.TD_("uk-text-right").CNY(o.amt)._TD();
                }, thead: () =>
                {
                    h.TH("日期", css: "uk-width-medium");
                    h.TH("类型");
                    h.TH("笔数");
                    h.TH("金额");
                });
                h.PAGINATION(arr?.Length == 30);
            }, false, 60);
        }
    }

    [OrglyAuthorize(Org.TYP_CTR)]
    [Ui("销售日总", "机构")]
    public class CtrlyBookAggWork : AggWork<CtrlyBookAggVarWork>
    {
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM bookaggs WHERE orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = await dc.QueryAsync<Agg>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                if (arr == null)
                {
                    h.ALERT("暂无生成报表");
                    return;
                }

                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD(Buy.Typs[o.typ]);
                    h.TD_("uk-text-right").T((int)o?.trans)._TD();
                    h.TD_("uk-text-right").CNY((decimal)o?.amt)._TD();
                }, thead: () =>
                {
                    h.TH("日期", css: "uk-width-medium");
                    h.TH("类型");
                    h.TH("笔数");
                    h.TH("金额");
                });
                h.PAGINATION(arr?.Length == 30);
            }, false, 60);
        }
    }
}