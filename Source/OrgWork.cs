using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class OrgWork : WebWork
    {
    }

    [Ui("平台入驻主体")]
    public class AdmlyOrgWork : OrgWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyOrgVarWork>();
        }

        [Ui("市场", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ = 5 ORDER BY typ, regid, status DESC");
            var arr = await dc.QueryAsync<Org>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: Label, subscript: 5);

                if (arr == null) return;

                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().VARTOOL(o.Key, nameof(AdmlyOrgVarWork.upd), caption: o.name).SP().SUB(Org.Typs[o.typ])._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(_Art.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                });
            });
        }

        [Ui("产源", group: 2), Tool(Anchor)]
        public async Task src(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ = 6 ORDER BY typ, regid, status DESC");
            var arr = await dc.QueryAsync<Org>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: Label, subscript: 6);

                if (arr == null) return;

                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().VARTOOL(o.Key, nameof(AdmlyOrgVarWork.upd), caption: o.name).SP().SUB(Org.Typs[o.typ])._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(_Art.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                });
            });
        }

        [Ui("其他", group: 4), Tool(Anchor)]
        public async Task other(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ >= 7 ORDER BY typ, regid, status DESC");
            var arr = await dc.QueryAsync<Org>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: Label, subscript: 7);

                if (arr == null) return;

                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().VARTOOL(o.Key, nameof(AdmlyOrgVarWork.upd), caption: o.name).SP().SUB(Org.Typs[o.typ])._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(_Art.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                });
            });
        }

        [Ui("✚", "新建入驻主体", group: 7), Tool(ButtonShow)]
        public async Task @new(WebContext wc, int typ)
        {
            var prin = (User) wc.Principal;
            var regs = ObtainMap<short, Reg>();

            if (wc.IsGet)
            {
                var m = new Org
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    status = _Art.STA_ENABLED
                };
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("主体信息");
                    h.LI_().SELECT("机构类型", nameof(m.typ), m.typ, Org.Typs, filter: (k, v) => typ == 7 ? (k >= 7) : k == typ, required: true)._LI();
                    h.LI_().TEXT("主体名称", nameof(m.name), m.name, min: 2, max: 10, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().SELECT("地区", nameof(m.regid), m.regid, regs)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, _Art.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(0, new Org
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


    [UserAuthorize(Org.TYP_MRT, 1)]
#if ZHNT
    [Ui("市场入驻机构")]
#else
    [Ui("驿站入驻机构")]
#endif
    public class MrtlyOrgWork : OrgWork
    {
        protected override void OnMake()
        {
            State = Org.TYP_BIZ;
            MakeVarWork<MrtlyOrgVarWork>(state: Org.TYP_BIZ);
        }

        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE sprid = @1 ORDER BY id");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id));
            var regs = ObtainMap<short, Reg>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.Key);
                    h.TD_().A_HREF_("/mrtly/", o.Key, "/", css: "uk-button-link")._ONCLICK_("return dialog(this,8,false,4,'');").T(o.name)._A()._TD();
                    h.TD(regs[o.regid].name);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("添加"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var regs = ObtainMap<short, Reg>();

            if (wc.IsGet)
            {
                var m = new Org
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    status = _Art.STA_ENABLED
                };
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("主体信息");
                    h.LI_().SELECT("类型", nameof(m.typ), m.typ, Org.Typs, filter: (k, v) => k == Org.TYP_BIZ, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().SELECT("区域", nameof(m.regid), m.regid, regs, filter: (k, v) => v.typ == Reg.TYP_INDOOR)._LI();
                    h.LI_().SELECT("业务分支", nameof(m.fork), m.fork, Item.Typs, required: true)._LI();
                    h.LI_().TEXT("编址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, _Art.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(0, new Org
                {
                    sprid = org.id,
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

    [UserAuthorize(Org.TYP_PRD, 1)]
    [Ui("产源入驻机构", "thumbnails")]
    public class SrclyOrgWork : OrgWork
    {
        protected override void OnMake()
        {
            State = Org.TYP_SRC;
            MakeVarWork<SrclyOrgVarWork>(state: Org.TYP_SRC);
        }

        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE sprid = @1 ORDER BY id");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id));
            var regs = ObtainMap<short, Reg>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.Key);
                    h.TD_().A_HREF_("/mrtly/", o.Key, "/", css: "uk-button-link")._ONCLICK_("return dialog(this,8,false,4,'');").T(o.name)._A()._TD();
                    h.TD(regs[o.regid].name);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("添加"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var regs = ObtainMap<short, Reg>();

            if (wc.IsGet)
            {
                var m = new Org
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    status = _Art.STA_ENABLED
                };
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("主体信息");
                    h.LI_().SELECT("类型", nameof(m.typ), m.typ, Org.Typs, filter: (k, v) => k == Org.TYP_BIZ, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().SELECT("区域", nameof(m.regid), m.regid, regs, filter: (k, v) => v.typ == Reg.TYP_INDOOR)._LI();
                    h.LI_().SELECT("业务分支", nameof(m.fork), m.fork, Item.Typs, required: true)._LI();
                    h.LI_().TEXT("编址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, _Art.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(0, new Org
                {
                    sprid = org.id,
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
}