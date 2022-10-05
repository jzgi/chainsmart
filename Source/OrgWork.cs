using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class OrgWork : WebWork
    {
    }

    public class PublyOrgWork : OrgWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyOrgVarWork>();
        }
    }

    [Ui("设置入驻机构", "业务")]
    public class AdmlyOrgWork : OrgWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<AdmlyOrgVarWork>();
        }

#if ZHNT
        [Ui("市场", group: 1), Tool(Anchor)]
#else
        [Ui("驿站", group: 1), Tool(Anchor)]
#endif
        public async Task @default(WebContext wc)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ = ").T(Org.TYP_MRT).T(" ORDER BY regid, status DESC");
            var map = await dc.QueryAsync<int, Org>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: 1);
                h.GRIDA(map, o =>
                    {
                        h.DIV_("uk-card-body");
                        h.T(o.name);
                        h._DIV();
                    }
                );
            });
        }

        [Ui("供区", group: 2), Tool(Anchor)]
        public async Task src(WebContext wc)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ IN (").T(Org.TYP_ZON).T(",").T(Org.TYP_CTR).T(") ORDER BY typ, status DESC");
            var map = await dc.QueryAsync<int, Org>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: 2);
                h.GRIDA(map, o =>
                    {
                        h.DIV_("uk-card-body");
                        h.T(o.name);
                        h._DIV();
                    }
                );
            });
        }

        [Ui("新建", "新建机构", icon: "plus", group: 7), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int cmd)
        {
            var prin = (User) wc.Principal;
            var regs = Grab<short, Reg>();
            var orgs = Grab<int, Org>();

            if (wc.IsGet)
            {
                var m = new Org
                {
                    typ = cmd == 1 ? Org.TYP_MRT : Org.TYP_ZON,
                    created = DateTime.Now,
                    creator = prin.name,
                    status = Entity.STU_NORMAL
                };

                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_(cmd == 1 ? "市场资料" : "供区资料");
                    if (cmd == 2)
                    {
                        h.LI_().SELECT("类型", nameof(m.typ), m.typ, Org.Typs, filter: (k, v) => k >= 10, required: true)._LI();
                    }
                    h.LI_().TEXT("机构名称", nameof(m.name), m.name, min: 2, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().SELECT(m.IsMarket ? "市场区划" : "省份", nameof(m.regid), m.regid, regs, filter: (k, v) => m.IsMarket ? v.IsSection : v.IsProvince, required: !m.IsZone)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().SELECT("关联中控", nameof(m.ctrid), m.ctrid, orgs, filter: (k, v) => v.IsCenter, required: true)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Entity.Statuses, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(Entity.MSK_BORN, new Org
                {
                    typ = (short) (cmd == 1 ? Org.TYP_MRT : 0),
                    created = DateTime.Now,
                    creator = prin.name,
                });
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, Entity.MSK_BORN)._VALUES_(Org.Empty, Entity.MSK_BORN);
                await dc.ExecuteAsync(p => o.Write(p, Entity.MSK_BORN));
                wc.GivePane(201); // created
            }
        }
    }

    [UserAuthorize(Org.TYP_ZON, 1)]
    [Ui("管理下属产源", "供区")]
    public class ZonlyOrgWork : OrgWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<ZonlyOrgVarWork>();
        }

        [Ui("有效产源", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 ORDER BY status DESC, name");
            var map = await dc.QueryAsync<int, Org>(p => p.Set(org.id));

            wc.GivePane(200, h =>
            {
                h.TOOLBAR();

                if (map == null) return;

                h.GRIDA(map, o =>
                {
                    h.HEADER_("uk-card-header").PIC(o.id, "/icon").SP().H5(o.name)._HEADER();
                    h.SECTION_("uk-card-body");
                    h.P(o.tip);
                    h._SECTION();
                    h.FOOTER_()._FOOTER();
                });
            });
        }

        [Ui(icon: "ban", group: 2), Tool(Anchor)]
        public async Task ban(WebContext wc)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 ORDER BY status DESC, name");
            var map = await dc.QueryAsync<int, Org>(p => p.Set(org.id));

            wc.GivePane(200, h =>
            {
                h.TOOLBAR();

                if (map == null) return;

                h.GRIDA(map, o =>
                {
                    h.HEADER_("uk-card-header").PIC(o.id, "/icon").SP().H5(o.name)._HEADER();
                    h.SECTION_("uk-card-body");
                    h.P(o.tip);
                    h._SECTION();
                    h.FOOTER_()._FOOTER();
                });
            });
        }

        [Ui("新建", icon: "plus"), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var prv = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var regs = Grab<short, Reg>();
            var m = new Org
            {
                typ = Org.TYP_SRC,
                prtid = prv.id,
                created = DateTime.Now,
                creator = prin.name,
                status = Entity.STU_NORMAL
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
                    h.LI_().CHECKBOX("委托代办", nameof(m.trust), m.trust).SELECT("状态", nameof(m.status), m.status, Entity.Statuses, filter: (k, v) => k >= 0, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                const short msk = Entity.MSK_BORN;
                var o = await wc.ReadObjectAsync(msk, instance: m);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, msk)._VALUES_(Org.Empty, msk);
                await dc.ExecuteAsync(p => o.Write(p, msk));

                wc.GivePane(201); // created
            }
        }
    }

    [UserAuthorize(Org.TYP_MRT, 1)]
#if ZHNT
    [Ui("管理下属商户", "市场")]
#else
    [Ui("管理下属商户", "驿站")]
#endif
    public class MktlyOrgWork : OrgWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<MktlyOrgVarWork>();
        }

        [Ui("全部商户", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var mrt = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 ORDER BY id");
            var arr = await dc.QueryAsync<int, Org>(p => p.Set(mrt.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.GRIDA(arr, o =>
                {
                    h.DIV_();
                    h.T(o.name);
                    h._DIV();
                });
            });
        }

        [Ui(icon: "search"), Tool(AnchorPrompt)]
        public async Task search(WebContext wc)
        {
            var regs = Grab<short, Reg>();

            bool inner = wc.Query[nameof(inner)];
            short regid = 0;
            if (inner)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_();
                    h.RADIOSET<short, Reg>(nameof(regid), regid, regs, filter: v => v.IsSection);
                    h._FORM();
                });
            }
            else // OUTER
            {
                regid = wc.Query[nameof(regid)];

                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE regid = @1");
                var map = await dc.QueryAsync<int, Org>(p => p.Set(regid));

                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    h.GRIDA(map, o =>
                    {
                        h.DIV_();
                        h.T(o.name);
                        h._DIV();
                    });
                }, false, 3);
            }
        }

        [Ui("新建", icon: "plus", group: 2), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var mrt = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var m = new Org
            {
                typ = Org.TYP_SHP,
                created = DateTime.Now,
                creator = prin.name,
                status = Entity.STU_NORMAL,
                prtid = mrt.id,
                ctrid = mrt.ctrid,
            };

            if (wc.IsGet)
            {
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本资料");

                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().TEXT("工商登记号", nameof(m.license), m.license, max: 20)._LI();
                    h.LI_().CHECKBOX("委托办理", nameof(m.trust), m.trust)._LI();
#if ZHNT
                    h.LI_().TEXT("挡位号", nameof(m.addr), m.addr, max: 4)._LI();
#else
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
#endif
                    h.LI_().SELECT("状态", nameof(m.status), m.status, Entity.Statuses, filter: (k, v) => k >= 0)._LI();

                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                await wc.ReadObjectAsync(0, instance: m);

                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, 0)._VALUES_(Org.Empty, 0);
                await dc.ExecuteAsync(p => { m.Write(p); });

                wc.GivePane(201); // created
            }
        }
    }
}