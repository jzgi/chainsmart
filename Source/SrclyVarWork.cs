using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;
using static Zhnt.Supply.User;

namespace Zhnt.Supply
{
    [UserAuthorize(Org.TYP_SRC | Org.TYP_CO_SRC, 1)]
    public class SrclyVarWork : WebWork, IOrglyVar
    {
        protected override void OnMake()
        {
            MakeWork<OrglyAccessWork>("access");

            MakeWork<SrclyProdWork>("prod");

            MakeWork<SrclyPurchWork>("purch");

            MakeWork<OrglyClearWork>("clear");

            MakeWork<CoSrclyOrgWork>("mbr");

            MakeWork<CoSrclyProdWork>("coprod");

            MakeWork<CoSrclyPurchWork>("copurch");
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
                h.TOOLBAR(caption: prin.name + "（" + Orgly[prin.orgly] + "）");

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

                h.OPLIST();
            }, false, 3);
        }

        [UserAuthorize(orgly: ORGLY_MGR)]
        [Ui("图片"), Tool(ButtonCrop, Appear.Small)]
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

        [UserAuthorize(orgly: ORGLY_MGR)]
        [Ui("设置", group: 1), Tool(ButtonShow)]
        public async Task setg(WebContext wc)
        {
            short orgid = wc[0];
            var obj = Obtain<short, Org>(orgid);
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Empty).T(" FROM users WHERE orgid = @1 AND orgly > 0");
                var map = dc.Query<int, User>(p => p.Set(orgid));
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("修改基本设置");
                    h.LI_().TEXT("标语", nameof(obj.tip), obj.tip, max: 16)._LI();
                    h.LI_().SELECT("状态", nameof(obj.status), obj.status, _Art.Statuses, filter: (k, v) => k > 0)._LI();
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
}