using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static Revital.Org;
using static SkyChain.Web.Modal;

namespace Revital.Site
{

    [Ui("平台｜入驻机构设置")]
    public class AdmlyOrgWork : OrgWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<AdmlyOrgVarWork>();
        }

        [Ui("市场", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM orgs_vw WHERE typ = ").T(TYP_MRT).T(" ORDER BY regid, status DESC");
            var arr = await dc.QueryAsync<Org>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: TYP_MRT);
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().AVAR(o.Key, o.name)._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(Info.Symbols[o.status]);
                    h.TDFORM(() => h.TOOLGROUPVAR(o.Key));
                });
            });
        }

        [Ui("中枢", group: 2), Tool(Anchor)]
        public async Task ctr(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM orgs_vw WHERE typ = ").T(TYP_CTR).T(" ORDER BY regid, status DESC");
            var arr = await dc.QueryAsync<Org>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: TYP_CTR);
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().AVAR(o.Key, o.name)._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(Info.Symbols[o.status]);
                    h.TDFORM(() => h.TOOLGROUPVAR(o.Key));
                });
            });
        }

        [Ui("供给", group: 4), Tool(Anchor)]
        public async Task prv(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM orgs_vw WHERE typ = ").T(TYP_PRV).T(" ORDER BY status DESC");
            var arr = await dc.QueryAsync<Org>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: TYP_PRV);
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().AVAR(o.Key, o.name)._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(Info.Symbols[o.status]);
                    h.TDFORM(() => h.TOOLGROUPVAR(o.Key));
                });
            });
        }

        [Ui("✚", "新建入驻机构", group: 7), Tool(ButtonShow)]
        public async Task @new(WebContext wc, int typ)
        {
            var prin = (User) wc.Principal;
            var regs = Grab<short, Reg>();
            var orgs = Grab<int, Org>();

            if (wc.IsGet)
            {
                var m = new Org
                {
                    typ = (short) typ,
                    created = DateTime.Now,
                    creator = prin.name,
                    status = Info.STA_ENABLED
                };
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    var typname = Typs[m.typ];
                    h.FORM_().FIELDSUL_(typname + "机构信息");
                    // if (typ == TYP_CHL)
                    // {
                    //     h.LI_().SELECT("业务分属", nameof(m.fork), m.fork, Item.Typs, required: true)._LI();
                    // }
                    h.LI_().TEXT(typname + "名称", nameof(m.name), m.name, min: 2, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    if (m.IsPrv)
                    {
                        h.LI_().SELECT("物流分支", nameof(m.fork), m.fork, Forks, required: true)._LI();
                    }
                    h.LI_().SELECT(m.HasLocality ? "所在地市" : "所在省份", nameof(m.regid), m.regid, regs, filter: (k, v) => m.HasLocality ? v.IsDist : v.IsProv, required: !m.IsPrv)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    if (m.HasXy)
                    {
                        h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    }
                    if (m.IsMrt)
                    {
                        h.LI_().SELECT("关联中枢", nameof(m.sprid), m.sprid, orgs, filter: (k, v) => v.IsCtr, required: true)._LI();
                    }
                    if (m.IsSrc)
                    {
                        h.LI_().SELECT("辐射地市", nameof(m.dists), m.dists, regs, filter: (k, v) => v.IsDist, size: 10)._LI();
                    }
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Info.Statuses, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(Info.BORN, new Org
                {
                    typ = (short) typ,
                    created = DateTime.Now,
                    creator = prin.name,
                });
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Empty, Info.BORN)._VALUES_(Empty, Info.BORN);
                await dc.ExecuteAsync(p => o.Write(p, Info.BORN));
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
        protected override void OnCreate()
        {
            State = TYP_BIZ;
            CreateVarWork<MrtlyOrgVarWork>(state: TYP_BIZ);
        }

        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM orgs_vw WHERE sprid = @1 ORDER BY id");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id));
            var regs = Grab<short, Reg>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.Key);
                    // h.TD_().A_("/mrtly/", o.Key, "/", css: "uk-button-link")._DIALOG_("return dialog(this,8,false,4,'');").T(o.name)._A()._TD();
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
            var regs = Grab<short, Reg>();

            if (wc.IsGet)
            {
                var m = new Org
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    status = Info.STA_ENABLED
                };
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("主体信息");
                    h.LI_().SELECT("类型", nameof(m.typ), m.typ, Typs, filter: (k, v) => k == TYP_BIZ, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().SELECT("区域", nameof(m.regid), m.regid, regs, filter: (k, v) => v.typ == Reg.TYP_SECT)._LI();
                    h.LI_().TEXT("编址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Info.Statuses)._LI();
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
    [Ui("供给｜下属产源管理", "thumbnails")]
    public class PrvlyOrgWork : OrgWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PrvlyOrgVarWork>();
        }

        public async Task @default(WebContext wc, int regid)
        {
            var org = wc[-1].As<Org>();
            var regs = Grab<short, Reg>();
            if (regid == 0)
            {
                wc.Subscript = regid = regs.First(x => x.IsProv)?.id ?? 0;
            }
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Empty).T(" FROM orgs_vw WHERE sprid = @1 AND regid = @2 ORDER BY status DESC, id");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id).Set(regid));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(regid, opts: regs, filter: (k, v) => v.IsProv);
                if (arr == null) return;

                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().AVAR(o.Key, o.name).SP().ADIALOG_("/prvly/", o.id, "/?astack=true", 8, false, Appear.Full).T("代办")._A()._TD();
                    h.TD(Info.Statuses[o.status]);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TDFORM(() => h.TOOLGROUPVAR(o.Key));
                });
            });
        }

        [Ui("✚", "新建产源"), Tool(ButtonShow)]
        public async Task @new(WebContext wc, int regid)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var regs = Grab<short, Reg>();
            var m = new Org
            {
                typ = TYP_SRC,
                sprid = org.id,
                regid = (short) regid,
                created = DateTime.Now,
                creator = prin.name,
                status = Info.STA_ENABLED
            };
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    var reg = regs[m.regid];
                    h.FORM_().FIELDSUL_(reg.name + "产源信息");
                    h.LI_().TEXT("主体名称", nameof(m.name), m.name, max: 10, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().TEXT("电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Info.Statuses).CHECKBOX("委托代办", nameof(m.trust), m.trust)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                const short proj = Info.BORN;
                var o = await wc.ReadObjectAsync(proj, instance: m);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Empty, proj)._VALUES_(Empty, proj);
                await dc.ExecuteAsync(p => o.Write(p, proj));

                wc.GivePane(201); // created
            }
        }
    }
}