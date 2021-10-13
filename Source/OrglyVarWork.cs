using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Zhnt.Supply
{
    public abstract class OrglyVarWork : WebWork
    {
        [UserAuthorize(orgly: 1)]
        [Ui("形象图标"), Tool(ButtonCrop, Appear.Small)]
        public async Task icon(WebContext wc)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                if (dc.QueryTop("SELECT icon FROM orgs WHERE id = @1", p => p.Set(id)))
                {
                    dc.Let(out byte[] bytes);
                    if (bytes == null) wc.Give(204); // no content 
                    else wc.Give(200, new StaticContent(bytes), shared: false, 60);
                }
                else wc.Give(404, shared: true, maxage: 3600 * 24); // not found
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ArraySegment<byte> img = f[nameof(img)];
                using var dc = NewDbContext();
                if (await dc.ExecuteAsync("UPDATE orgs SET icon = @1 WHERE id = @2", p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else wc.Give(500); // internal server error
            }
        }

        [UserAuthorize(orgly: 1)]
        [Ui("运行设置"), Tool(ButtonShow)]
        public async Task setg(WebContext wc)
        {
            short orgid = wc[0];
            var obj = Obtain<short, Org>(orgid);
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE orgid = @1 AND orgly > 0");
                var map = dc.Query<int, User>(p => p.Set(orgid));
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("修改基本设置");
                    h.LI_().TEXT("标语", nameof(obj.tip), obj.tip, max: 16)._LI();
                    h.LI_().SELECT("状态", nameof(obj.status), obj.status, Art_.Statuses, filter: (k, v) => k > 0)._LI();
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

    [UserAuthorize(Org.TYP_CTR, 1)]
    [Ui("分拣中心操作")]
    public class CtrlyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            MakeWork<OrglyAccessWork>("access", User.Ctrly);

            MakeWork<CtrlyBuyWork>("buy"); // downstream order

            MakeWork<CtrlyPurchWork>("purch"); // upstream order

            MakeWork<OrglyClearWork>("clear"); // upstream order
        }

        public void @default(WebContext wc)
        {
            short orgid = wc[0];
            var o = Obtain<short, Org>(orgid);
            var regs = ObtainMap<short, Reg>();

            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: prin.name + "（" + User.Orgly[prin.orgly] + "）");

                h.UL_("uk-card uk-card-primary uk-card-body ul-list uk-list-divider");
                h.LI_().FIELD("经营主体", o.Name)._LI();
                h.LI_().FIELD2("地址", regs[o.regid]?.name, o.addr)._LI();
                h.LI_().FIELD2("负责人", o.mgrname, o.mgrtel)._LI();
                h.LI_().FIELD("状态", Art_.Statuses[o.status])._LI();
                h._UL();

                h.TASKUL();
            }, false, 3);
        }
    }

    [UserAuthorize(Org.TYP_BIZ | Org.TYP_CO_BIZ, 1)]
    [Ui("商户端")]
    public class BizlyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            MakeWork<BizlyBuyWork>("buy");

            MakeWork<OrglyClearWork>("clear");

            MakeWork<BizColyOrgWork>("org");

            MakeWork<OrglyAccessWork>("access", User.Orgly);
        }

        public void @default(WebContext wc)
        {
            short orgid = wc[0];
            var org = Obtain<short, Org>(orgid);
            var co = Obtain<short, Org>(org.coid);
            var ctr = Obtain<short, Org>(org.ctrid);

            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: prin.name + "（" + User.Orgly[prin.orgly] + "）");

                h.UL_("uk-card uk-card-primary uk-card-body uk-list uk-list-divider");
                h.LI_().FIELD("主体名称", org.Name)._LI();
                h.LI_().FIELD("协作类型", Org.Typs[org.typ])._LI();
                h.LI_().FIELD("地址", org.addr)._LI();
                if (!org.IsBizCo)
                {
                    h.LI_().FIELD("商户社", co.name)._LI();
                }
                h.LI_().FIELD("分拣中心", ctr.name)._LI();
                h.LI_().FIELD2("负责人", org.mgrname, org.mgrtel)._LI();
                h.LI_().FIELD("授权代办", org.grant)._LI();
                h._UL();

                h.TASKUL();
            }, false, 3);
        }
    }

    [UserAuthorize(Org.TYP_SRC | Org.TYP_SRCCO, 1)]
    public class SrclyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            MakeWork<SrclyPurchWork>("purch");

            MakeWork<SrcColyOrgWork>("org");

            MakeWork<SrcColyProdWork>("prod");

            MakeWork<SrcColyPurchWork>("copurch");

            MakeWork<OrglyClearWork>("clear");

            MakeWork<OrglyAccessWork>("access");
        }

        public void @default(WebContext wc)
        {
            short orgid = wc[0];
            var org = Obtain<short, Org>(orgid);
            var co = Obtain<short, Org>(org.coid);

            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: prin.name + "（" + User.Orgly[prin.orgly] + "）");

                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body");
                h.LI_().FIELD("主体", org.Name)._LI();
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