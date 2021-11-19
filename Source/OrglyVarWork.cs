using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class OrglyVarWork : WebWork
    {
        [UserAuthorize(orgly: 1)]
        [Ui("设置"), Tool(ButtonShow)]
        public async Task setg(WebContext wc)
        {
            var org = wc[0].As<Org>();
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("修改基本设置");
                    h.LI_().TEXT("标语", nameof(org.tip), org.tip, max: 16)._LI();
                    h.LI_().TEXT("地址", nameof(org.addr), org.addr, max: 16)._LI();
                    h.LI_().SELECT("状态", nameof(org.status), org.status, _Article.Statuses, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else
            {
                var o = await wc.ReadObjectAsync(inst: org); // use existing object
                using var dc = NewDbContext();
                // update the db record
                await dc.ExecuteAsync("UPDATE orgs SET tip = @1, cttid = CASE WHEN @2 = 0 THEN NULL ELSE @2 END, status = @3 WHERE id = @4",
                    p => p.Set(o.tip).Set(o.status).Set(org.id));

                wc.GivePane(200);
            }
        }
    }


    [UserAuthorize(Org.TYP_PRD, 1)]
    [Ui("产源操作")]
    public class SrclyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            MakeWork<FrmlyProductWork>("product");

            MakeWork<FrmlyBidWork>("bid");

            MakeWork<SrclyOrgWork>("org");

            MakeWork<OrglyClearWork>("clear");

            MakeWork<OrglyAccessWork>("access");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var co = Obtain<int, Org>(org.sprid);

            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                var role = prin.orgid != org.id ? "代办" : User.Orgly[prin.orgly];
                h.TOOLBAR(caption: prin.name + "（" + role + "）");

                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body");
                h.LI_().FIELD("主体", org.name)._LI();
                h.LI_().FIELD("类型", Org.Typs[org.typ])._LI();
                h.LI_().FIELD("地址", org.addr)._LI();
                if (!org.IsMrt)
                {
                    // h.LI_().FIELD("产源社", co.name)._LI();
                }
                h.LI_().FIELD2("负责人", org.mgrname, org.mgrtel)._LI();
                h._UL();
                h._FORM();

                h.TASKUL();
            }, false, 3);
        }
    }

    [UserAuthorize(Org.TYP_CTR, 1)]
    [Ui("供应中心操作")]
    public class CtrlyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            MakeWork<AgriCtrlyPlanWork, DietCtrlyPlanWork, FactCtrlyPlanWork, CareCtrlyPlanWork, AdvtCtrlyPlanWork, CharCtrlyPlanWork>("plan");

            MakeWork<CtrlyBidWork>("bid");

            MakeWork<AgriCtrlyBookWork, DietCtrlyBookWork, FactCtrlyBookWork, CareCtrlyBookWork, AdvtCtrlyBookWork, CharCtrlyBookWork>("book");

            MakeWork<OrglyClearWork>("clear");

            MakeWork<OrglyAccessWork>("access");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var regs = ObtainMap<short, Reg>();
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            wc.GivePage(200, h =>
            {
                var role = prin.orgid != org.id ? "代办" : User.Orgly[prin.orgly];
                h.TOOLBAR(caption: prin.name + "（" + role + "）");

                h.UL_("uk-card uk-card-primary uk-card-body ul-list uk-list-divider");
                h.LI_().FIELD("主体名称", org.name)._LI();
                h.LI_().FIELD2("地址", regs[org.regid]?.name, org.addr)._LI();
                h.LI_().FIELD2("负责人", org.mgrname, org.mgrtel)._LI();
                h.LI_().FIELD("状态", _Article.Statuses[org.status])._LI();
                h._UL();

                h.TASKUL();
            }, false, 3);
        }
    }


    [UserAuthorize(Org.TYP_BIZ, 1)]
#if ZHNT
    [Ui("市场操作")]
#else
    [Ui("驿站操作")]
#endif
    public class MrtlyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            MakeWork<MrtlyOrgWork>("org");

            MakeWork<MrtlyCustWork>("cust");

            MakeWork<AgriBizlyPostWork, DietBizlyPostWork>("post");

            MakeWork<AgriBizlyBuyWork, DietBizlyBuyWork>("buy");

            MakeWork<AgriBizlyBookWork, DietBizlyBookWork>("book");

            MakeWork<AgriBizlyPosWork>("pos");

            MakeWork<OrglyClearWork>("clear");

            MakeWork<OrglyAccessWork>("access", User.Orgly);
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            wc.GivePage(200, h =>
            {
                var role = prin.orgid != org.id ? "代办" : User.Orgly[prin.orgly];
                h.TOOLBAR(caption: prin.name + "（" + role + "）");

                h.UL_("uk-card uk-card-primary uk-card-body uk-list uk-list-divider");
                h.LI_().FIELD("主体名称", org.name)._LI();
                h.LI_().FIELD("类型", Org.Typs[org.typ])._LI();
                h.LI_().FIELD(org.IsMrt ? "地址" : "编址", org.addr)._LI();
                if (org.sprid > 0)
                {
                    var spr = Obtain<int, Org>(org.sprid);
#if ZHNT
                    h.LI_().FIELD("所在市场", spr.name)._LI();
#else
                    h.LI_().FIELD("所在驿站", spr.name)._LI();
#endif
                }
                if (org.ctrid > 0)
                {
                    var ctr = Obtain<int, Org>(org.ctrid);
                    h.LI_().FIELD("主供应中心", ctr.name)._LI();
                }
                h.LI_().FIELD2("负责人", org.mgrname, org.mgrtel)._LI();
                h.LI_().FIELD("委托代办", org.trust)._LI();
                h._UL();

                h.TASKUL();
            }, false, 3);
        }
    }
}