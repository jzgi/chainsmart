using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;
using static Zhnt.User;

namespace Zhnt
{
    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("账号")]
    public class SrclyVarWork : WebWork
    {
        protected override void OnMake()
        {
            MakeWork<OrglyAccessWork>("acc");

            MakeWork<SrclyUOrdWork>("uord");

            // src group

            MakeWork<SrcGrplyMbrWork>("mbr");

            MakeWork<SrcGrplyKpiWork>("kpi");
        }

        public async Task @default(WebContext wc)
        {
            bool inner = wc.Query[nameof(inner)];
            short id = wc[0];
            var orgs = Fetch<Map<short, Org>>();
            var o = orgs[id];
            if (!inner)
            {
                wc.GiveFrame(200, false, 60, title: "供应操作", group: (byte) o.typ);
            }
            else
            {
                using var dc = NewDbContext();

                wc.GivePage(200, h =>
                {
                    h.TOOLBAR(caption: "本方账号信息");

                    h.FORM_("uk-card uk-card-default");
                    h.HEADER_("uk-card-header").T("基本信息").SPAN_("uk-badge").T("状态：").T(_Art.Statuses[o.status])._SPAN()._HEADER();
                    h.UL_("uk-card-body");
                    h.LI_().FIELD("本方名称", o.Name)._LI();
                    h.LI_().FIELD("标语", o.tip)._LI();
                    h.LI_().FIELD("类型", Org.Typs[o.typ])._LI();
                    h.LI_().FIELD("地址", o.addr)._LI();
                    if (o.IsPt)
                    {
                        // var shop = o.grpid > 0 ? orgs[o.grpid]?.name : null;
                        // h.LI_().FIELD("关联厨房", shop)._LI();
                    }
                    if (o.IsMerchant)
                    {
                        // h.LI_().FIELD("派递模式", o.refid ? "全网包邮" : "同城服务站")._LI();
                    }
                    h.LI_().FIELD2("负责人", o.mgrname, o.mgrtel)._LI();
                    h._UL();
                    h.FOOTER_("uk-card-footer uk-flex-center").TOOL(nameof(setg), css: "uk-button-secondary")._FOOTER();
                    h._FORM();

                    if (o.IsProvider)
                    {
                        string url = ServerEnviron.extcfg[nameof(url)];
                        h.SECTION_("uk-section uk-flex-middle uk-col");
                        h.P("本方主页");
                        h.QRCODE(url + "/org/" + o.id + "/", css: "uk-width-medium");
                        h._SECTION();
                    }
                }, false, 3);
            }
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
            var obj = Fetch<Map<short, Org>>()[orgid];
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