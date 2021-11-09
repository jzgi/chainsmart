using System.Threading.Tasks;
using Revital.Shop;
using Revital.Supply;
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
            short orgid = wc[0];
            var obj = Obtain<short, Org>(orgid);
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("修改基本设置");
                    h.LI_().TEXT("标语", nameof(obj.tip), obj.tip, max: 16)._LI();
                    h.LI_().TEXT("地址", nameof(obj.addr), obj.addr, max: 16)._LI();
                    h.LI_().SELECT("状态", nameof(obj.status), obj.status, _Bean.Statuses, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else
            {
                var o = await wc.ReadObjectAsync(inst: obj); // use existing object
                using var dc = NewDbContext();
                // update the db record
                await dc.ExecuteAsync("UPDATE orgs SET tip = @1, cttid = CASE WHEN @2 = 0 THEN NULL ELSE @2 END, status = @3 WHERE id = @4",
                    p => p.Set(o.tip).Set(o.status).Set(orgid));

                wc.GivePane(200);
            }
        }
    }


    [UserAuthorize(Org.TYP_SRC | Org.TYP_FRM, 1)]
    public class SrclyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            // MakeWork<SrclyGainWork>("purch");

            MakeWork<FrmlyYieldWork>("fyield");

            MakeWork<FrmlySubscribWork>("fsub");

            MakeWork<SrclyOrgWork>("org");

            MakeWork<SrclyYieldWork>("prod");

            MakeWork<SrclySubscribWork>("gain");

            MakeWork<OrglyClearWork>("clear");

            MakeWork<OrglyAccessWork>("access");
        }

        public void @default(WebContext wc)
        {
            short orgid = wc[0];
            var org = Obtain<int, Org>(orgid);
            var co = Obtain<int, Org>(org.sprid);

            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: prin.name + "（" + User.Orgly[prin.orgly] + "）");

                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body");
                h.LI_().FIELD("主体", org.name)._LI();
                h.LI_().FIELD("类型", Org.Typs[org.typ])._LI();
                h.LI_().FIELD("地址", org.addr)._LI();
                if (!org.IsMart)
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

    [UserAuthorize(Org.TYP_CTR_HLF, 1)]
    [Ui("供应中心操作")]
    public class CtrlyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            MakeWork<AgriCtrlySupplyWork, DietaryCtrlySupplyWork, HomeCtrlySupplyWork, CareCtrlySupplyWork, AdCtrlySupplyWork, CharityCtrlySupplyWork>("supply");

            MakeWork<AgriCtrlyDistribWork, DietaryCtrlyDistribWork, HomeCtrlyDistribWork, CareCtrlyDistribWork, AdCtrlyDistribWork, CharityCtrlyDistribWork>("distrib");

            MakeWork<CtrlySubscribWork>("subscrib");

            MakeWork<OrglyClearWork>("clear");

            MakeWork<OrglyAccessWork>("access");
        }

        public void @default(WebContext wc)
        {
            var o = wc[0].As<Org>();
            var regs = ObtainMap<short, Reg>();

            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: prin.name + "（" + User.Orgly[prin.orgly] + "）");

                h.UL_("uk-card uk-card-primary uk-card-body ul-list uk-list-divider");
                h.LI_().FIELD("主体名称", o.name)._LI();
                h.LI_().FIELD2("地址", regs[o.regid]?.name, o.addr)._LI();
                h.LI_().FIELD2("负责人", o.mgrname, o.mgrtel)._LI();
                h.LI_().FIELD("状态", _Bean.Statuses[o.status])._LI();
                h._UL();

                h.TASKUL();
            }, false, 3);
        }
    }


    [UserAuthorize(Org.TYP_BIZ | Org.TYP_MRT, 1)]
    [Ui("市集驿站端")]
    public class MrtlyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            MakeWork<AgriBizlyActWork, DietaryBizlyActWork>("act");

            MakeWork<BizlyDistribWork>("distrib");

            MakeWork<MrtlyOrgWork>("org");

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
                h.TOOLBAR(caption: prin.name + "（" + User.Orgly[prin.orgly] + "）");

                h.UL_("uk-card uk-card-primary uk-card-body uk-list uk-list-divider");
                h.LI_().FIELD("主体名称", org.name)._LI();
                h.LI_().FIELD("类型", Org.Typs[org.typ])._LI();
                h.LI_().FIELD("地址／编址", org.addr)._LI();
                if (!org.IsMart && org.sprid > 0)
                {
                    var spr = Obtain<int, Org>(org.sprid);
                    h.LI_().FIELD("市场／驿站", spr.name)._LI();
                }
                if (org.ctrid > 0)
                {
                    var ctr = Obtain<int, Org>(org.ctrid);
                    h.LI_().FIELD("供应中心", ctr.name)._LI();
                }
                h.LI_().FIELD2("负责人", org.mgrname, org.mgrtel)._LI();
                h.LI_().FIELD("授权代办", org.trust)._LI();
                h._UL();

                h.TASKUL();
            }, false, 3);
        }
    }
}