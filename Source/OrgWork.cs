using System;
using System.Threading.Tasks;
using CoChain;
using CoChain.Web;
using static CoChain.Web.Modal;
using static CoChain.Nodal.Store;

namespace Revital
{
    public abstract class OrgWork : WebWork
    {
    }

    [Ui("平台入驻机构设置", icon: "thumbnails")]
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
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ = ").T(Org.TYP_MRT).T(" ORDER BY regid, state DESC");
            var arr = await dc.QueryAsync<Org>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: 1);
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().AVAR(o.Key, o.name)._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(o.state == 1 ? null : Entity.States[o.state]);
                    h.TDFORM(() => h.TOOLGROUPVAR(o.Key));
                });
            });
        }

        [Ui("供应版块", group: 2), Tool(Anchor)]
        public async Task prv(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ IN (").T(Org.TYP_PRV).T(",").T(Org.TYP_CTR).T(") ORDER BY typ, state DESC");
            var arr = await dc.QueryAsync<Org>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: 2);
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().AVAR(o.Key, o.name)._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(o.state == 1 ? null : Entity.States[o.state]);
                    h.TDFORM(() => h.TOOLGROUPVAR(o.Key));
                });
            });
        }

        [Ui("✚", "新建机构", group: 7), Tool(ButtonShow)]
        public async Task @new(WebContext wc, int cmd)
        {
            var prin = (User) wc.Principal;
            var regs = Grab<short, Reg>();
            var orgs = Grab<int, Org>();

            if (wc.IsGet)
            {
                var m = new Org
                {
                    typ = cmd == 1 ? Org.TYP_MRT : Org.TYP_PRV,
                    created = DateTime.Now,
                    creator = prin.name,
                    state = Entity.STA_ENABLED
                };

                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_(cmd == 1 ? "市场属性" : "供应版块属性");
                    if (cmd == 2)
                    {
                        h.LI_().SELECT("类型", nameof(m.typ), m.typ, Org.Typs, filter: (k, v) => k >= 10, required: true)._LI();
                    }
                    h.LI_().TEXT("机构名称", nameof(m.name), m.name, min: 2, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    if (m.HasProvision)
                    {
                        h.LI_().SELECT("物流方式", nameof(m.fork), m.fork, Org.Forks, required: true)._LI();
                    }
                    h.LI_().SELECT(m.IsMarket ? "市场区划" : "省份", nameof(m.regid), m.regid, regs, filter: (k, v) => m.IsMarket ? v.IsSection : v.IsProvince, required: !m.IsProvision)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().SELECT("关联中枢", nameof(m.ctrties), m.ctrties, orgs, filter: (k, v) => v.IsCenter, multiple: m.IsProvision, required: true)._LI();
                    h.LI_().SELECT("状态", nameof(m.state), m.state, Entity.States, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(Entity.BORN, new Org
                {
                    typ = (short) (cmd == 1 ? Org.TYP_MRT : 0),
                    created = DateTime.Now,
                    creator = prin.name,
                });
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, Entity.BORN)._VALUES_(Org.Empty, Entity.BORN);
                await dc.ExecuteAsync(p => o.Write(p, Entity.BORN));
                wc.GivePane(201); // created
            }
        }
    }


    [UserAuthorize(Org.TYP_MRT, 1)]
#if ZHNT
    [Ui("市场下属商户管理", icon: "thumbnails")]
#else
    [Ui("市场下属驿站管理", icon: "thumbnails")]
#endif
    public class MrtlyOrgWork : OrgWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<MartlyOrgVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            var mrt = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE sprid = @1 ORDER BY id");
            var arr = await dc.QueryAsync<Org>(p => p.Set(mrt.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.Key);
                    h.TD_().ADIALOG_("/mrtly/", o.Key, "/", 8, false, Appear.Full).T(o.name)._A()._TD();
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("新建"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var m = new Org
            {
                typ = Org.TYP_SHP,
                created = DateTime.Now,
                creator = prin.name,
                state = Entity.STA_ENABLED,
                sprid = org.id,
            };
            if (wc.IsGet)
            {
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_(
#if ZHNT
                        "商户属性"
#else
                        "驿站属性"
#endif
                    );
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().TEXT("工商登记号", nameof(m.license), m.license, max: 20)._LI();
#if ZHNT
                    h.LI_().TEXT("编码", nameof(m.addr), m.addr, max: 4)._LI();
#else
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
#endif
                    h.LI_().SELECT("状态", nameof(m.state), m.state, Entity.States)._LI();
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

    [UserAuthorize(Org.TYP_PRV, 1)]
    [Ui("版块下属产源管理", icon: "thumbnails")]
    public class PrvnlyOrgWork : OrgWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PrvnlyOrgVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE sprid = @1 ORDER BY state DESC, id LIMIT 30 OFFSET 30 * @2");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null) return;

                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().AVAR(o.Key, o.name).SP().ADIALOG_("/srcly/", o.id, "/", 8, false, Appear.Full).T("代办")._A()._TD();
                    h.TD(Entity.States[o.state]);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TDFORM(() => h.TOOLGROUPVAR(o.Key));
                });
            });
        }

        [Ui("＋", "新建产源"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            var prv = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var regs = Grab<short, Reg>();
            var m = new Org
            {
                typ = Org.TYP_SRC,
                fork = prv.fork,
                sprid = prv.id,
                created = DateTime.Now,
                creator = prin.name,
                state = Entity.STA_ENABLED
            };
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("产源属性");
                    h.LI_().TEXT("主体名称", nameof(m.name), m.name, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().SELECT("省份", nameof(m.regid), m.regid, regs, filter: (k, v) => v.IsProvince, required: true)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.0000, max: 180.0000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().TEXT("电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h.LI_().CHECKBOX("委托代办", nameof(m.trust), m.trust).SELECT("状态", nameof(m.state), m.state, Entity.States, filter: (k, v) => k >= 0, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                const short msk = Entity.BORN;
                var o = await wc.ReadObjectAsync(msk, instance: m);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, msk)._VALUES_(Org.Empty, msk);
                await dc.ExecuteAsync(p => o.Write(p, msk));

                wc.GivePane(201); // created
            }
        }
    }
}