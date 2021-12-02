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
    public abstract class CtrlyPlanWork : PlanWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyPlanVarWork>();
        }

        [Ui("期前", group: 1), Tool(Anchor)]
        public void pre(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Plan.Empty).T(" FROM plans WHERE orgid = @1 AND status > 0 AND starton > @2 ORDER BY cat, status DESC LIMIT 40 OFFSET 40 * @3");
            var arr = dc.Query<Plan>(p => p.Set(org.id).Set(DateTime.Today).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: Label);

                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.cat != last)
                    {
                        h.TR_().TD_("uk-label", colspan: 6).T(Item.Cats[o.cat])._TD()._TR();
                    }
                    h.TR_();
                    h.TD_().VARTOOL(o.Key, nameof(CtrlyPlanVarWork.upd), caption: o.name).SP()._TD();
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TD_().CNY(o.price, true).T("／").T(o.unit)._TD();
                    h.TD(Plan.Typs[o.postg]);
                    h.TD(_Article.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                    h._TR();

                    last = o.cat;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }


        [Ui("期中", group: 2), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Plan.Empty).T(" FROM plans WHERE orgid = @1 AND status > 0 AND @2 BETWEEN starton AND endon ORDER BY cat, status DESC LIMIT 40 OFFSET 40 * @3");
            var arr = dc.Query<Plan>(p => p.Set(org.id).Set(DateTime.Today).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: Label);

                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.cat != last)
                    {
                        h.TR_().TD_("uk-label", colspan: 6).T(Item.Cats[o.cat])._TD()._TR();
                    }
                    h.TR_();
                    h.TD_().VARTOOL(o.Key, nameof(CtrlyPlanVarWork.upd), caption: o.name).SP()._TD();
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TD_().CNY(o.price, true).T("／").T(o.unit)._TD();
                    h.TD(Plan.Typs[o.postg]);
                    h.TD(_Article.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                    h._TR();

                    last = o.cat;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("期后", group: 4), Tool(Anchor)]
        public void post(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Plan.Empty).T(" FROM plans WHERE orgid = @1 AND status > 0 AND endon < @2 ORDER BY cat, status DESC LIMIT 40 OFFSET 40 * @3");
            var arr = dc.Query<Plan>(p => p.Set(org.id).Set(DateTime.Today).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: Label);

                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.cat != last)
                    {
                        h.TR_().TD_("uk-label", colspan: 6).T(Item.Cats[o.cat])._TD()._TR();
                    }
                    h.TR_();
                    h.TD_().VARTOOL(o.Key, nameof(CtrlyPlanVarWork.upd), caption: o.name).SP()._TD();
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TD_().CNY(o.price, true).T("／").T(o.unit)._TD();
                    h.TD(Plan.Typs[o.postg]);
                    h.TD(_Article.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                    h._TR();

                    last = o.cat;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("关闭", group: 8), Tool(Anchor)]
        public void gone(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Plan.Empty).T(" FROM plans WHERE orgid = @1 AND status = 0 ORDER BY cat, endon DESC LIMIT 40 OFFSET 40 * @3");
            var arr = dc.Query<Plan>(p => p.Set(org.id).Set(DateTime.Today).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: Label);

                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.cat != last)
                    {
                        h.TR_().TD_("uk-label", colspan: 6).T(Item.Cats[o.cat])._TD()._TR();
                    }
                    h.TR_();
                    h.TD_().VARTOOL(o.Key, nameof(CtrlyPlanVarWork.upd), caption: o.name).SP()._TD();
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TD_().CNY(o.price, true).T("／").T(o.unit)._TD();
                    h.TD(Plan.Typs[o.postg]);
                    h.TD(_Article.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                    h._TR();

                    last = o.cat;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("✚", "新建供应项目", group: 3), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var items = ObtainMap<short, Item>();
            if (wc.IsGet)
            {
                var dt = DateTime.Today;
                var o = new Plan
                {
                    status = _Article.STA_DISABLED,
                    starton = dt,
                    endon = dt,
                    fillon = dt,
                    postg = Plan.TYP_ONE,
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("标准品目", nameof(o.itemid), o.itemid, items, Item.Cats, filter: x => x.typ == org.fork, required: true).TEXT("附加名", nameof(o.ext), o.ext, max: 10)._LI();
                    h.LI_().TEXTAREA("特色描述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().DATE("起始日", nameof(o.starton), o.starton).DATE("截止日", nameof(o.endon), o.endon)._LI();
                    h.LI_().SELECT("交付日约束", nameof(o.typ), o.typ, Plan.Typs, required: true).DATE("指定交付日", nameof(o.fillon), o.fillon)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Article.Statuses, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("规格参数");

                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("标准比", nameof(o.unitx), o.unitx, min: 1, max: 1000, required: true)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step).NUMBER("最大容量", nameof(o.cap), o.cap)._LI();
                    h.LI_().SELECT("市场价约束", nameof(o.postg), o.postg, Plan.Postgs, required: true).NUMBER("市场价", nameof(o.postprice), o.postprice, min: 0.00M, max: 10000.00M)._LI();

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
                    created = DateTime.Now,
                    creator = prin.name,
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

    [Ui("供应项目管理", "calendar", fork: Item.TYP_AGRI)]
    public class CtrlyAgriPlanWork : CtrlyPlanWork
    {
    }

    [Ui("供应项目管理", "calendar", fork: Item.TYP_DIET)]
    public class CtrlyDietPlanWork : CtrlyPlanWork
    {
    }

    [Ui("供应项目管理", "calendar", fork: Item.TYP_FACT)]
    public class CtrlyFactPlanWork : CtrlyPlanWork
    {
    }

    [Ui("供应项目管理", "calendar", fork: Item.TYP_CARE)]
    public class CtrlyCarePlanWork : CtrlyPlanWork
    {
    }

    [Ui("公益项目管理", "calendar", fork: Item.TYP_CHAR)]
    public class CtrlyCharPlanWork : CtrlyPlanWork
    {
    }

    [Ui("传媒项目管理", "calendar", fork: Item.TYP_ADVT)]
    public class CtrlyAdvtPlanWork : CtrlyPlanWork
    {
    }
}