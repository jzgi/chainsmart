using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital.Supply
{
    [Ui("协作主体管理", "℠")]
    public class AdmlyOrgWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyOrgVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            var regs = ObtainMap<short, Reg>();

            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw ORDER BY regid, id, status DESC");
            var arr = await dc.QueryAsync<Org>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: Label);

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
                    h.TD_().VARTOOL(o.Key, nameof(AdmlyOrgVarWork.upd), caption: o.name).SP().SUB(Org.Typs[o.typ])._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(Org.Statuses[o.status]);
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
            var prin = (User_) wc.Principal;
            var regs = ObtainMap<short, Reg>();

            if (wc.IsGet)
            {
                var m = new Org
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    status = Org.STA_ENABLED
                };
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("主体信息");
                    h.LI_().SELECT("类型", nameof(m.typ), m.typ, Org.Typs, filter: (k, v) => k != Org.TYP_BIZ && k != Org.TYP_SRCCO, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().SELECT("地区", nameof(m.regid), m.regid, regs)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Org.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Org>(0, new Org
                {
                    created = DateTime.Now,
                    creator = prin.name,
                });
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, 0)._VALUES_(Org.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p));
                wc.GivePane(201); // created
            }
        }
    }


    [UserAuthorize(Org.TYP_BIZCO, 1)]
    [Ui("商户团管理", "album")]
    public class BizColyOrgWork : WebWork
    {
        protected override void OnMake()
        {
            State = Org.TYP_BIZ;
            MakeVarWork<BizColyOrgVarWork>(state: Org.TYP_BIZ);
        }

        public async Task @default(WebContext wc)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE coid = @1 ORDER BY id");
            var arr = await dc.QueryAsync<Org>(p => p.Set(orgid));
            var regs = ObtainMap<short, Reg>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.Key);
                    h.TD_().A_HREF_("/bizly/", o.Key, "/", css: "uk-button-link")._ONCLICK_("return dialog(this,8,false,4,'');").T(o.name)._A()._TD();
                    h.TD(regs[o.regid].name);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("添加"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            var prin = (User_) wc.Principal;
            var regs = ObtainMap<short, Reg>();

            if (wc.IsGet)
            {
                var m = new Org
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    status = Org.STA_ENABLED
                };
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("主体信息");
                    h.LI_().SELECT("类型", nameof(m.typ), m.typ, Org.Typs, filter: (k, v) => k != Org.TYP_BIZ && k != Org.TYP_SRCCO, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().SELECT("地区", nameof(m.regid), m.regid, regs)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Org.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Org>(0, new Org
                {
                    created = DateTime.Now,
                    creator = prin.name,
                });
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, 0)._VALUES_(Org.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p));
                wc.GivePane(201); // created
            }
        }
    }

    [UserAuthorize(Org.TYP_SRC, 1)]
    [Ui("产源团管理", "thumbnails")]
    public class SrcColyOrgWork : WebWork
    {
        protected override void OnMake()
        {
            State = Org.TYP_SRCCO;
            MakeVarWork<BizColyOrgVarWork>(state: Org.TYP_SRCCO);
        }

        public async Task @default(WebContext wc)
        {
        }
    }
}