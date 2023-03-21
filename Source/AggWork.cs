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
    [Ui("消费业务汇总表", "财务")]
    public class AdmlyBuyAggWork : AggWork<AdmlyBuyAggVarWork>
    {
        [Ui("消费业务", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM buyaggs WHERE typ = 1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = dc.Query<Agg>(p => p.Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD(Buy.Typs[(short)o.acct]);
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
            dc.Sql("SELECT * FROM buyaggs WHERE typ = 1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = dc.Query<Agg>(p => p.Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD(Buy.Typs[(short)o.acct]);
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
    [Ui("供应业务汇总表", "财务")]
    public class AdmlyBookAggWork : AggWork<AdmlyBookAggVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [Ui("销售汇总", "商户")]
    public class ShplyBuyAggWork : AggWork<OrglyAggrVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM rpts WHERE typ = 1 AND orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = dc.Query<Agg>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD(Buy.Typs[(short)o.acct]);
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


    [Ui("销售报表", "商户")]
    public class SrclyBookAggWork : AggWork<OrglyAggrVarWork>
    {
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM rpts WHERE typ = 2 AND orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = await dc.QueryAsync<Agg>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    var item = GrabObject<int, Asset>(o.acct);
                    h.TD(item.name);
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
    [Ui("销售报表", "机构")]
    public class CtrlyBookAggWork : AggWork<OrglyAggrVarWork>
    {
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM bookaggs WHERE typ = 3 AND orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = await dc.QueryAsync<Agg>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    var item = GrabObject<int, Asset>(o.acct);
                    h.TD(item.name);
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
}