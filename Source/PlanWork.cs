using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class PlanWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_CTR, User.ORGLY_OP)]
    public abstract class SuplyPlanWork : PlanWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyPlanVarWork>();
        }

        [Ui("当前", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Plan.Empty).T(" FROM plans WHERE orgid = @1 AND status > 0 ORDER BY cat, status DESC LIMIT 40 OFFSET 40 * @2");
            var arr = dc.Query<Plan>(p => p.Set(org.id).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.cat != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(Item.Cats[o.cat])._TD()._TR();
                    }
                    h.TR_();
                    h.TD(o.name);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TD_().CNY(o.price, true).T("／").T(o.unit)._TD();
                    h.TD(Plan.Fillgs[o.fillg]);
                    h.TD(_Article.Statuses[o.status]);
                    h._TR();

                    last = o.cat;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }


        [Ui("过往", group: 2), Tool(Anchor)]
        public void past(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Plan.Empty).T(" FROM plans WHERE typ = 1 AND ORDER BY status DESC LIMIT 40 OFFSET 40 * @1");
            var arr = dc.Query<Plan>(p => p.Set(page));
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
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(Item.Cats[o.typ])._TD()._TR();
                    }
                    h.TR_();
                    h.TD(_Article.Statuses[o.status]);
                    h.TD(Plan.Fillgs[o.fillg]);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h._TR();
                    last = o.typ;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("✚", "新建供应项目", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int sch)
        {
            var org = wc[-1].As<Org>();
            var items = ObtainMap<short, Item>();
            if (wc.IsGet)
            {
                var dt = DateTime.Today;
                var o = new Plan
                {
                    fillg = Plan.FIL_ONE,
                    starton = dt,
                    endon = dt,
                    fillon = dt
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("标准品目", nameof(o.itemid), o.itemid, items, Item.Cats, filter: x => x.typ == org.forkie, required: true)._LI();
                    h.LI_().TEXT("附加名", nameof(o.ext), o.ext, max: 10)._LI();
                    h.LI_().TEXTAREA("特色描述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().DATE("生效日", nameof(o.starton), o.starton)._LI();
                    h.LI_().DATE("截止日", nameof(o.endon), o.endon)._LI();
                    h.LI_().SELECT("交付模式", nameof(o.fillg), o.fillg, Plan.Fillgs)._LI();
                    h.LI_().DATE("远期交付", nameof(o.fillon), o.fillon)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Article.Statuses)._LI();

                    h._FIELDSUL().FIELDSUL_("终端参数");
                    h.LI_().TEXT("单位", nameof(o.dunit), o.dunit, max: 10, required: true).NUMBER("标准比", nameof(o.dunitx), o.dunitx, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.dmin), o.dmin).NUMBER("限订量", nameof(o.dmax), o.dmax, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.dstep), o.dstep)._LI();
                    h.LI_().NUMBER("价格", nameof(o.dprice), o.dprice, min: 0.00M, max: 10000.00M).NUMBER("优惠", nameof(o.doff), o.doff)._LI();
                    h._FIELDSUL();

                    h.BOTTOM_BUTTON("确定");

                    h._FORM();
                });
            }

            else // POST
            {
                // populate 
                var o = await wc.ReadObjectAsync(0, new Plan
                {
                    fillg = Plan.FIL_ONE,
                    orgid = org.id,
                });
                var item = items[o.itemid];
                o.cat = item.cat;
                o.name = item.name + '（' + o.ext + '）';

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO plans ").colset(Plan.Empty, 0)._VALUES_(Plan.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }

    [Ui("供应项目管理", "calendar", forkie: Item.TYP_AGRI)]
    public class AgriSuplyPlanWork : SuplyPlanWork
    {
    }

    [Ui("供应项目管理", "calendar", forkie: Item.TYP_DIET)]
    public class DietSuplyPlanWork : SuplyPlanWork
    {
    }

    [Ui("供应项目管理", "calendar", forkie: Item.TYP_FACT)]
    public class FactSuplyPlanWork : SuplyPlanWork
    {
    }

    [Ui("供应项目管理", "calendar", forkie: Item.TYP_CARE)]
    public class CareSuplyPlanWork : SuplyPlanWork
    {
    }

    [Ui("公益项目管理", "calendar", forkie: Item.TYP_CHAR)]
    public class CharSuplyPlanWork : SuplyPlanWork
    {
    }

    [Ui("传媒项目管理", "calendar", forkie: Item.TYP_ADVT)]
    public class AdvtSuplyPlanWork : SuplyPlanWork
    {
    }
}