using System.Threading.Tasks;
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
                    h.LI_().SELECT("状态", nameof(obj.status), obj.status, Item.Statuses, filter: (k, v) => k > 0)._LI();
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


    [UserAuthorize(Org.TYP_BIZ | Org.TYP_BIZCO, 1)]
    [Ui("商户端")]
    public class BizlyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            // MakeWork<BizlyBookWork>("buy");

            MakeWork<OrglyClearWork>("clear");

            MakeWork<BizColyOrgWork>("org");

            MakeWork<OrglyAccessWork>("access", User.Orgly);
        }

        public void @default(WebContext wc)
        {
            short orgid = wc[0];
            var org = Obtain<short, Org>(orgid);
            var co = Obtain<int, Org>(org.sprid);
            var ctr = Obtain<int, Org>(org.ctrid);

            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: prin.name + "（" + User.Orgly[prin.orgly] + "）");

                h.UL_("uk-card uk-card-primary uk-card-body uk-list uk-list-divider");
                h.LI_().FIELD("主体名称", org.name)._LI();
                h.LI_().FIELD("协作类型", Org.Typs[org.typ])._LI();
                h.LI_().FIELD("地址", org.addr)._LI();
                if (!org.IsBizCo)
                {
                    h.LI_().FIELD("商户社", co.name)._LI();
                }
                h.LI_().FIELD("分拣中心", ctr.name)._LI();
                h.LI_().FIELD2("负责人", org.mgrname, org.mgrtel)._LI();
                h.LI_().FIELD("授权代办", org.trust)._LI();
                h._UL();

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
            MakeWork<CtrlySupplyWork>("supply");

            MakeWork<CtrlyUpWork>("up");

            MakeWork<CtrlyGainWork>("gain");

            MakeWork<OrglyClearWork>("clear");

            MakeWork<OrglyAccessWork>("access");
        }

        public void @default(WebContext wc)
        {
            int orgid = wc[0];
            var o = Obtain<int, Org>(orgid);
            var regs = ObtainMap<string, Reg>();

            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: prin.name + "（" + User.Orgly[prin.orgly] + "）");

                h.UL_("uk-card uk-card-primary uk-card-body ul-list uk-list-divider");
                h.LI_().FIELD("主体名称", o.name)._LI();
                h.LI_().FIELD2("地址", regs[o.regid]?.name, o.addr)._LI();
                h.LI_().FIELD2("负责人", o.mgrname, o.mgrtel)._LI();
                h.LI_().FIELD("状态", Item.Statuses[o.status])._LI();
                h._UL();

                h.TASKUL();
            }, false, 3);
        }
    }


    [UserAuthorize(Org.TYP_SRCCO | Org.TYP_SRC, 1)]
    public class SrclyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            // MakeWork<SrclyGainWork>("purch");

            MakeWork<SrcColyOrgWork>("org");

            MakeWork<SrcColyYieldWork>("prod");

            MakeWork<SrcColyPurchWork>("copurch");

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
                if (!org.IsBizCo)
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
}