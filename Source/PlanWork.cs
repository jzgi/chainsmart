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
    [Ui("供应项目管理", "calendar")]
    public class CtrlyPlanWork : PlanWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyPlanVarWork>();
        }

        [Ui("常规供应", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Plan.Empty).T(" FROM plans WHERE ctrid = @1 AND typ = 1 ORDER BY status DESC LIMIT 40 OFFSET 40 * @1");
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
                    h.TD(o.name);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TD(o.nprice, true);
                    h.TD(_Doc.Statuses[o.status]);
                    h._TR();
                    last = o.typ;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }


        [Ui("预期供应", group: 2), Tool(Anchor)]
        public void pre(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Plan.Empty).T(" FROM plans WHERE typ = 2 ORDER BY status DESC LIMIT 40 OFFSET 40 * @1");
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
                    h.TD(_Doc.Statuses[o.status]);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h._TR();
                    last = o.typ;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("✚", "新建常规供应项目", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int sch)
        {
            var org = wc[-1].As<Org>();
            var items = ObtainMap<short, Item>();
            if (wc.IsGet)
            {
                var dt = DateTime.Today;
                var o = new Plan
                {
                    started = dt,
                    ended = dt,
                    filled = dt
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("标准品目", nameof(o.itemid), o.itemid, items, Item.Cats, filter: x => x.typ == org.forkie, required: true)._LI();
                    h.LI_().TEXT("附加名", nameof(o.name), o.name, max: 10, required: true)._LI();
                    h.LI_().TEXTAREA("特色描述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().DATE("生效日", nameof(o.started), o.started)._LI();
                    h.LI_().DATE("截止日", nameof(o.ended), o.ended)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Doc.Statuses)._LI();

                    h._FIELDSUL().FIELDSUL_("下行参数");
                    h.LI_().TEXT("单位", nameof(o.dunit), o.name, max: 10, required: true).NUMBER("标准比", nameof(o.dx), o.dx, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.dmin), o.dmin, max: 10).NUMBER("限订量", nameof(o.dmax), o.dmax, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.dstep), o.dstep, max: 10)._LI();
                    h.LI_().NUMBER("价格", nameof(o.dprice), o.dprice, min: 0.01M, max: 10000.00M).NUMBER("优惠", nameof(o.doff), o.doff, max: 10)._LI();
                    h._FIELDSUL();

                    if (org.HasSubsrib)
                    {
                        h.FIELDSUL_("上行参数");
                        h.LI_().TEXT("单位", nameof(o.sunit), o.sunit, max: 10, required: true).NUMBER("标准比", nameof(o.sx), o.sx, min: 1, max: 1000)._LI();
                        h.LI_().NUMBER("起订量", nameof(o.smin), o.smin, max: 10).NUMBER("限订量", nameof(o.smax), o.smax, min: 1, max: 1000)._LI();
                        h.LI_().NUMBER("递增量", nameof(o.sstep), o.sstep, max: 10)._LI();
                        h.LI_().NUMBER("价格", nameof(o.sprice), o.sprice, min: 0.01M, max: 10000.00M).NUMBER("优惠", nameof(o.soff), o.soff, max: 10)._LI();
                        h._FIELDSUL();
                    }

                    if (org.HasNeed)
                    {
                        h.FIELDSUL_("终端参数");
                        h.LI_().TEXT("单位", nameof(o.nunit), o.nunit, max: 10, required: true).NUMBER("标准比", nameof(o.nx), o.nx, min: 1, max: 1000)._LI();
                        h.LI_().NUMBER("起订量", nameof(o.nmin), o.nmin, max: 10).NUMBER("限订量", nameof(o.nmax), o.nmax, min: 1, max: 1000)._LI();
                        h.LI_().NUMBER("递增量", nameof(o.nstep), o.nstep, max: 10)._LI();
                        h.LI_().NUMBER("价格", nameof(o.nprice), o.nprice, min: 0.01M, max: 10000.00M).NUMBER("优惠", nameof(o.noff), o.noff, max: 10)._LI();
                        h._FIELDSUL();
                    }

                    h.BOTTOM_BUTTON("确定");

                    h._FORM();
                });
            }

            else // POST
            {
                var o = await wc.ReadObjectAsync<Plan>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO plans ").colset(Plan.Empty, 0)._VALUES_(Plan.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }

        [Ui("✚", "新建预期供应项目", group: 2), Tool(ButtonOpen)]
        public async Task newpre(WebContext wc, int sch)
        {
            var org = wc[-1].As<Org>();
            var items = ObtainMap<short, Item>();
            if (wc.IsGet)
            {
                var dt = DateTime.Today;
                var o = new Plan
                {
                    started = dt,
                    ended = dt,
                    filled = dt
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("标准品目", nameof(o.itemid), o.itemid, items, Item.Cats, filter: x => x.typ == org.forkie, required: true)._LI();
                    h.LI_().TEXT("附加名", nameof(o.name), o.name, max: 10, required: true)._LI();
                    h.LI_().TEXT("特色描述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().DATE("起售日", nameof(o.started), o.started)._LI();
                    h.LI_().DATE("止售日", nameof(o.ended), o.ended)._LI();
                    if (sch > 1)
                    {
                        h.LI_().DATE("交付日", nameof(o.filled), o.filled)._LI();
                    }
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Doc.Statuses)._LI();

                    h._FIELDSUL().FIELDSUL_("销售参数");

                    h.LI_().TEXT("单位", nameof(o.nunit), o.name, max: 10, required: true).NUMBER("标准倍比", nameof(o.nx), o.nx, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.nmin), o.nmin, max: 10).NUMBER("限订量", nameof(o.nmax), o.nmax, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.nstep), o.nstep, max: 10)._LI();
                    h.LI_().NUMBER("价格", nameof(o.nprice), o.nprice, min: 0.01M, max: 10000.00M).NUMBER("优惠", nameof(o.noff), o.noff, max: 10)._LI();

                    h._FIELDSUL().FIELDSUL_("采购参数");

                    // h.LI_().TEXT("单位", nameof(o.uunit), o.uunit, max: 10, required: true).NUMBER("标准倍比", nameof(o.ux), o.ux, min: 1, max: 1000)._LI();
                    // h.LI_().NUMBER("价格", nameof(o.uprice), o.uprice, min: 0.01M, max: 10000.00M)._LI();

                    h._FIELDSUL();

                    h.BOTTOM_BUTTON("确定");

                    h._FORM();
                });
            }

            else // POST
            {
                var o = await wc.ReadObjectAsync<Plan>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO plans ").colset(Plan.Empty, 0)._VALUES_(Plan.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }

    [Ui("供应项管理", "calendar", forkie: Item.TYP_AGRI)]
    public class AgriCtrlyPlanWork : CtrlyPlanWork
    {
    }

    [Ui("供应项管理", "calendar", forkie: Item.TYP_DIETARY)]
    public class DietaryCtrlyPlanWork : CtrlyPlanWork
    {
    }

    public class HomeCtrlyPlanWork : CtrlyPlanWork
    {
    }

    public class CareCtrlyPlanWork : CtrlyPlanWork
    {
    }

    public class AdCtrlyPlanWork : CtrlyPlanWork
    {
    }

    public class CharityCtrlyPlanWork : CtrlyPlanWork
    {
    }
}