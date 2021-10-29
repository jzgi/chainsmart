using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static Revital.Supply.User_;
using static SkyChain.Web.Modal;

namespace Revital.Supply
{
    [UserAuthorize(admly: ADMLY_SUPLLY_MGT)]
    [Ui("产品供应管理", "calendar")]
    public class CtrlySupplyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyOfferVarWork>();
        }

        [Ui("未生效", group: 1), Tool(Anchor)]
        public void pre(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Supply_.Empty).T(" FROM plans ORDER BY typ, status < 2 LIMIT 40 OFFSET 40 * @1");
            var arr = dc.Query<Supply_>(p => p.Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.typ != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(Item.Typs[o.typ])._TD()._TR();
                    }
                    h.TR_();
                    h.TD(o.name);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TD(o.rprice, true);
                    h.TD(Supply_.Statuses[o.status]);
                    h._TR();
                    last = o.typ;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("进行中", group: 2), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Supply_.Empty).T(" FROM plans ORDER BY typ, status >= 2 LIMIT 40 OFFSET 40 * @1");
            var arr = dc.Query<Supply_>(p => p.Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.typ != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(Item.Typs[o.typ])._TD()._TR();
                    }
                    h.TR_();
                    h.TD(Supply_.Statuses[o.status]);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h._TR();
                    last = o.typ;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("已结束", group: 4), Tool(Anchor)]
        public void post(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Supply_.Empty).T(" FROM plans ORDER BY typ, status DESC LIMIT 40 OFFSET 40 * @1");
            var arr = dc.Query<Supply_>(p => p.Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.typ != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(Item.Typs[o.typ])._TD()._TR();
                    }
                    h.TR_();
                    h.TD(Supply_.Statuses[o.status]);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h._TR();
                    last = o.typ;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("✚", "新建供应计划", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int sch)
        {
            var items = ObtainMap<int, Item>();
            if (wc.IsGet)
            {
                if (sch == 0) // display type selection
                {
                    wc.GivePane(200, h =>
                    {
                        h.FORM_().FIELDSUL_("请选择供应类型");
                        for (int i = 0; i < Supply_.Schemes.Count; i++)
                        {
                            var key = Supply_.Schemes.KeyAt(i);
                            var scheme = Supply_.Schemes.ValueAt(i);
                            h.LI_("uk-flex").A_HREF_(nameof(@new) + "-" + key, end: true, css: "uk-button uk-button-secondary uk-width-1-1").T(scheme)._A()._LI();
                        }
                        h._FIELDSUL();
                        h._FORM();
                    });
                }
                else
                {
                    var dt = DateTime.Today;
                    var o = new Supply_
                    {
                        started = dt,
                        ended = dt,
                        delivered = dt
                    };
                    wc.GivePane(200, h =>
                    {
                        h.FORM_().FIELDSUL_("基本信息");

                        h.LI_().SELECT_ITEM("品类", nameof(o.itemid), o.itemid, items, Item.Typs)._LI();
                        h.LI_().TEXT("品名", nameof(o.name), o.name, max: 10, required: true)._LI();
                        h.LI_().TEXT("特色描述", nameof(o.tip), o.tip, max: 40)._LI();
                        h.LI_().DATE("起售日", nameof(o.started), o.started)._LI();
                        h.LI_().DATE("止售日", nameof(o.ended), o.ended)._LI();
                        if (sch > 1)
                        {
                            h.LI_().DATE("交付日", nameof(o.delivered), o.delivered)._LI();
                        }
                        h.LI_().SELECT("状态", nameof(o.status), o.status, Supply_.Statuses)._LI();

                        h._FIELDSUL().FIELDSUL_("销售参数");

                        h.LI_().TEXT("单位", nameof(o.runit), o.name, max: 10, required: true).NUMBER("标准倍比", nameof(o.runitx), o.runitx, min: 1, max: 1000)._LI();
                        h.LI_().NUMBER("起订量", nameof(o.rmin), o.rmin, max: 10).NUMBER("限订量", nameof(o.rmax), o.rmax, min: 1, max: 1000)._LI();
                        h.LI_().NUMBER("递增量", nameof(o.rstep), o.rstep, max: 10)._LI();
                        h.LI_().NUMBER("价格", nameof(o.rprice), o.rprice, min: 0.01M, max: 10000.00M).NUMBER("优惠", nameof(o.roff), o.roff, max: 10)._LI();

                        h._FIELDSUL().FIELDSUL_("采购参数");

                        h.LI_().TEXT("单位", nameof(o.punit), o.punit, max: 10, required: true).NUMBER("标准倍比", nameof(o.punitx), o.punitx, min: 1, max: 1000)._LI();
                        h.LI_().NUMBER("价格", nameof(o.pprice), o.pprice, min: 0.01M, max: 10000.00M)._LI();

                        h._FIELDSUL();

                        h.BOTTOM_BUTTON("确定");

                        h._FORM();
                    });
                }
            }

            else // POST
            {
                var o = await wc.ReadObjectAsync<Supply_>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO plans ").colset(Supply_.Empty, 0)._VALUES_(Supply_.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }
}