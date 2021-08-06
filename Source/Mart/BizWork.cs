using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Zhnt.Mart
{
    public class PublyBizWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PublyBizVarWork>();
        }

        public void @default(WebContext wc)
        {
            short regid = wc.Query[nameof(regid)];
            var regs = Fetch<Map<short, Reg>>();
            var reg = regs[regid];
            var orgs = Fetch<Map<short, Biz>>();
            wc.GivePage(200, h =>
            {
                h.FORM_().FIELDSUL_("健康商家");
                for (int i = 0; i < orgs.Count; i++)
                {
                    var o = orgs.ValueAt(i);
                }
                h._FIELDSUL()._FORM();

                h.FORM_().FIELDSUL_("社工机构");
                for (int i = 0; i < orgs.Count; i++)
                {
                    var o = orgs.ValueAt(i);
                    if (o.IsSocialTo(reg))
                    {
                        h.LI_("uk-flex uk-flex-middle");
                        h.A_GOTO(o.name, "/org/" + o.id + "/", css: "uk-button-link").SP().SUB(o.tip).SP().A_TEL("☏", o.Tel, css: "uk-small")._SPAN();
                        h.SPAN_("uk-margin-auto-left");
                        if (o.x > 0 && o.y > 0)
                        {
                            h.A_POI(o.x, o.y, o.name, o.addr, o.Tel);
                        }
                        else
                        {
                            h.T("<span class=\"uk-icon-link uk-inactive\" uk-icon=\"location\"></span>");
                        }
                        h._SPAN();
                        h._LI();
                    }
                }
                h._FIELDSUL()._FORM();
            });
        }
    }

    [UserAuthorize(admly: User.ADMLY_OP)]
    [Ui("经营方")]
    public class AdmlyBizWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyBizVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            var regs = Fetch<Map<short, Reg>>();

            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Biz.Empty).T(" FROM orgs_vw ORDER BY regid, id, status DESC");
            var arr = await dc.QueryAsync<Biz>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: "服务方");

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.regid != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 4).T(o.regid > 0 ? regs[o.regid].name : "其它")._TD()._TR();
                    }
                    h.TR_();
                    h.TDCHECK(o.id);
                    h.TD_().VARTOOL(o.Key, nameof(AdmlyBizVarWork.upd), caption: o.name).SP().SUB(Biz.Typs[o.typ])._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(_Art.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                    h._TR();
                    last = o.regid;
                }
                h._TABLE();
            });
        }

        [Ui("新建"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            var regs = Fetch<Map<short, Reg>>();
            var orgs = Fetch<Map<short, Biz>>();
            if (wc.IsGet)
            {
                var m = new Biz();
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("服务方属性");
                    h.LI_().SELECT("服务类型", nameof(m.typ), m.typ, Biz.Typs)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXT("标语", nameof(m.tip), m.tip, max: 16)._LI();
                    h.LI_().SELECT("所属区域", nameof(m.regid), m.regid, regs, required: false).CHECKBOX("全网包邮", nameof(m.@extern), true, m.@extern)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, _Art.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Biz>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Biz.Empty, 0)._VALUES_(Biz.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p));
                wc.GivePane(201); // created
            }
        }
    }


    [UserAuthorize(bizly: 1)]
    [Ui("摊位")]
    public class BizlyBizWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyBizVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            short id = wc[-1];
            var regs = Fetch<Map<short, Reg>>();

            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Biz.Empty).T(" FROM bizs_vw WHERE parent = @1 AND typ = 2 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Biz>(p => p.Set(id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: "摊位");

                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.regid != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 4).T(o.regid > 0 ? regs[o.regid].name : "其它")._TD()._TR();
                    }
                    h.TR_();
                    h.TDCHECK(o.id);
                    h.TD_().VARTOOL(o.Key, nameof(AdmlyBizVarWork.upd), caption: o.name).SP().SUB(Biz.Typs[o.typ])._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(_Art.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                    h._TR();
                    last = o.regid;
                }
                h._TABLE();
            });
        }

        [Ui("新建"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            var regs = Fetch<Map<short, Reg>>();
            var orgs = Fetch<Map<short, Biz>>();
            if (wc.IsGet)
            {
                var m = new Biz();
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("服务方属性");
                    h.LI_().SELECT("服务类型", nameof(m.typ), m.typ, Biz.Typs)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXT("标语", nameof(m.tip), m.tip, max: 16)._LI();
                    h.LI_().SELECT("所属区域", nameof(m.regid), m.regid, regs, required: false).CHECKBOX("全网包邮", nameof(m.@extern), true, m.@extern)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, _Art.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Biz>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Biz.Empty, 0)._VALUES_(Biz.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p));
                wc.GivePane(201); // created
            }
        }
    }
}