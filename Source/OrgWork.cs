using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static Revital.Org;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class OrgWork : WebWork
    {
    }

    public class PublySrcWork : OrgWork
    {
        public async Task @default(WebContext wc, int code)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Book.Empty).T(" FROM orgs WHERE codend >= @1 ORDER BY codend LIMIT 1");
            var o = await dc.QueryTopAsync<Book>(p => p.Set(code));
            wc.GivePage(200, h =>
            {
                if (o == null || o.codend - o.codes >= code)
                {
                    h.ALERT("编码没有找到");
                }
                else
                {
                    var plan = Obtain<short, Product>(o.planid);
                    var frm = Obtain<int, Org>(o.toid);
                    var ctr = Obtain<int, Org>(o.fromid);

                    h.FORM_();
                    h.FIELDSUL_("溯源信息");
                    h.LI_().FIELD("生产户", frm.name);
                    h.LI_().FIELD("分拣中心", ctr.name);
                    h._FIELDSUL();
                    h._FORM();
                }
            }, title: "中惠农通溯源系统");
        }
    }


    [Ui("［平台］入驻机构设置")]
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
            dc.Sql("SELECT ").collst(Empty).T(" FROM orgs_vw WHERE typ = ").T(TYP_MRT).T(" ORDER BY regid, status DESC");
            var arr = await dc.QueryAsync<Org>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: Label, subscript: TYP_MRT);

                if (arr == null) return;

                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().VARTOOL(o.Key, nameof(AdmlyOrgVarWork.upd), caption: o.name).SP().SUB(Typs[o.typ])._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(_Info.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                });
            });
        }

        [Ui("分拣", group: 2), Tool(Anchor)]
        public async Task ctr(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM orgs_vw WHERE typ = ").T(TYP_CTR).T(" ORDER BY regid, status DESC");
            var arr = await dc.QueryAsync<Org>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: Label, subscript: TYP_CTR);

                if (arr == null) return;

                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().VARTOOL(o.Key, nameof(AdmlyOrgVarWork.upd), caption: o.name).SP().SUB(Typs[o.typ])._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(_Info.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                });
            });
        }

        [Ui("产供", group: 4), Tool(Anchor)]
        public async Task sec(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM orgs_vw WHERE typ = ").T(TYP_PRV).T(" ORDER BY status DESC");
            var arr = await dc.QueryAsync<Org>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: Label, subscript: TYP_PRV);

                if (arr == null) return;

                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().VARTOOL(o.Key, nameof(AdmlyOrgVarWork.upd), caption: o.name).SP().SUB(Typs[o.typ])._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(_Info.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                });
            });
        }

        [Ui("✚", "新建入驻机构", group: 7), Tool(ButtonShow)]
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
                    status = _Info.STA_ENABLED
                };
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("机构信息");
                    h.LI_().SELECT("类型", nameof(m.typ), m.typ, Typs, filter: (k, v) => typ == k, required: true)._LI();
                    // if (typ == TYP_CHL)
                    // {
                    //     h.LI_().SELECT("业务分属", nameof(m.fork), m.fork, Item.Typs, required: true)._LI();
                    // }
                    h.LI_().TEXT("名称", nameof(m.name), m.name, min: 2, max: 10, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().SELECT("地区", nameof(m.regid), m.regid, regs, filter: (k, v) => k == 1, required: typ != TYP_PRV)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, _Info.Statuses)._LI();
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
                dc.Sql("INSERT INTO orgs ").colset(Empty, 0)._VALUES_(Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p));
                wc.GivePane(201); // created
            }
        }
    }


    [UserAuthorize(TYP_MRT, 1)]
#if ZHNT
    [Ui("［市场］下属商户管理")]
#else
    [Ui("［驿站］下属商户管理")]
#endif
    public class MrtlyOrgWork : OrgWork
    {
        protected override void OnMake()
        {
            State = TYP_BIZ;
            MakeVarWork<MrtlyOrgVarWork>(state: TYP_BIZ);
        }

        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM orgs_vw WHERE sprid = @1 ORDER BY id");
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
                    status = _Info.STA_ENABLED
                };
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("主体信息");
                    h.LI_().SELECT("类型", nameof(m.typ), m.typ, Typs, filter: (k, v) => k == TYP_BIZ, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().SELECT("区域", nameof(m.regid), m.regid, regs, filter: (k, v) => v.typ == Reg.TYP_SECTIONAL)._LI();
                    h.LI_().SELECT("业务分支", nameof(m.fork), m.fork, Item.Typs, required: true)._LI();
                    h.LI_().TEXT("编址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, _Info.Statuses)._LI();
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
                dc.Sql("INSERT INTO orgs ").colset(Empty, 0)._VALUES_(Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p));
                wc.GivePane(201); // created
            }
        }
    }

    [UserAuthorize(TYP_PRV, 1)]
    [Ui("［板块］下属产源管理", "thumbnails")]
    public class PrvlyOrgWork : OrgWork
    {
        protected override void OnMake()
        {
            State = TYP_PRV;
            MakeVarWork<SrclyOrgVarWork>(state: TYP_PRV);
        }

        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM orgs_vw WHERE sprid = @1 ORDER BY id");
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
                    status = _Info.STA_ENABLED
                };
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("主体信息");
                    h.LI_().SELECT("类型", nameof(m.typ), m.typ, Typs, filter: (k, v) => k == TYP_BIZ, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().SELECT("区域", nameof(m.regid), m.regid, regs, filter: (k, v) => v.typ == Reg.TYP_SECTIONAL)._LI();
                    h.LI_().SELECT("业务分支", nameof(m.fork), m.fork, Item.Typs, required: true)._LI();
                    h.LI_().TEXT("编址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, _Info.Statuses)._LI();
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
                dc.Sql("INSERT INTO orgs ").colset(Empty, 0)._VALUES_(Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p));
                wc.GivePane(201); // created
            }
        }
    }
}