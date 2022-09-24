using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class OrgVarWork : WebWork
    {
        protected async Task doimg(WebContext wc, string col, bool shared, int maxage)
        {
            int id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").T(col).T(" FROM orgs WHERE id = @1");
                if (dc.QueryTop(p => p.Set(id)))
                {
                    dc.Let(out byte[] bytes);
                    if (bytes == null) wc.Give(204); // no content 
                    else wc.Give(200, new StaticContent(bytes), shared, maxage);
                }
                else
                    wc.Give(404, null, shared, maxage); // not found
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ArraySegment<byte> img = f[nameof(img)];
                using var dc = NewDbContext();
                dc.Sql("UPDATE orgs SET ").T(col).T(" = @1 WHERE id = @2");
                if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else
                    wc.Give(500); // internal server error
            }
        }
    }

    public class PublyOrgVarWork : OrgVarWork
    {
        public async Task @default(WebContext wc)
        {
        }

        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon), true, 3600);
        }
    }

    public class AdmlyOrgVarWork : OrgVarWork
    {
        public async Task @default(WebContext wc)
        {
            short id = wc[0];
            var prin = (User) wc.Principal;
            var regs = Grab<short, Reg>();
            var orgs = Grab<int, Org>();
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1");
                var m = dc.QueryTop<Org>(p => p.Set(id));

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_(m.IsMarket ? "市场属性" : "供应版块属性");
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
                var m = await wc.ReadObjectAsync(0, new Org
                {
                    adapted = DateTime.Now,
                    adapter = prin.name
                });
                using var dc = NewDbContext();
                dc.Sql("UPDATE orgs")._SET_(Org.Empty, 0).T(" WHERE id = @1");
                dc.Execute(p =>
                {
                    m.Write(p, 0);
                    p.Set(id);
                });
                wc.GivePane(200); // close
            }
        }

        [Ui("웃", "设置管理员", @group: 7), Tool(ButtonOpen, Appear.Small)]
        public async Task mgr(WebContext wc, int cmd)
        {
            if (wc.IsGet)
            {
                string tel = wc.Query[nameof(tel)];
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("指定用户");
                    h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(mgr), 1, post: false)._LI();
                    h._FIELDSUL();
                    if (cmd == 1) // search user
                    {
                        using var dc = NewDbContext();
                        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                        var o = dc.QueryTop<User>(p => p.Set(tel));
                        if (o != null)
                        {
                            h.FIELDSUL_();
                            h.HIDDEN(nameof(o.id), o.id);
                            h.LI_().FIELD("用户名", o.name)._LI();
                            h._FIELDSUL();
                            h.BOTTOMBAR_().BUTTON("确认", nameof(mgr), 2)._BOTTOMBAR();
                        }
                    }
                    h._FORM();
                });
            }
            else // POST
            {
                int orgid = wc[0];
                int id = (await wc.ReadAsync<Form>())[nameof(id)];
                using var dc = NewDbContext(IsolationLevel.ReadCommitted);
                dc.Execute("UPDATE orgs SET mgrid = @1 WHERE id = @2", p => p.Set(id).Set(orgid));
                dc.Execute("UPDATE users SET orgid = @1, orgly = 15 WHERE id = @2", p => p.Set(orgid).Set(id));
                wc.GivePane(200); // ok
            }
        }

        [Ui("图标", icon: "github"), Tool(ButtonCrop, Appear.Small)]
        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon), false, 3);
        }
    }


    public class MktlyOrgVarWork : OrgVarWork
    {
        public async Task @default(WebContext wc)
        {
            int id = wc[0];
            var topOrgs = Grab<int, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1");
            var o = await dc.QueryTopAsync<Org>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.TITLEBAR(o.name);

                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("商户名称", o.name)._LI();
                h.LI_().FIELD("简述", o.tip)._LI();
                h.LI_().FIELD("主管机构", topOrgs[o.prtid]?.name).FIELD("保存周期", topOrgs[o.ctrid]?.name)._LI();
                h.LI_().FIELD("信用编号", o.license).FIELD("单位提示", o.regid)._LI();
                h.LI_().FIELD("只供代理", o.trust).FIELD("状态", o.status, Entity.Statuses)._LI();
                h._UL();

                h.TOOLBAR(top: false);
            });
        }

        [Ui("修改", "修改商户资料", icon: "pencil"), Tool(ButtonOpen, Appear.Large)]
        public async Task edit(WebContext wc)
        {
            int id = wc[0];
            var regs = Grab<short, Reg>();

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Org>(p => p.Set(id));

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
            else
            {
                var m = await wc.ReadObjectAsync<Org>(0);
                using var dc = NewDbContext();
                dc.Sql("UPDATE orgs")._SET_(Org.Empty, 0).T(" WHERE id = @1");
                dc.Execute(p =>
                {
                    m.Write(p, 0);
                    p.Set(id);
                });
                wc.GivePane(200); // close
            }
        }

        [Ui("图标", icon: "github"), Tool(ButtonCrop, Appear.Small)]
        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon), false, 3);
        }
    }

    public class ZonlyOrgVarWork : OrgVarWork
    {
        public async Task @default(WebContext wc)
        {
            int id = wc[0];
            var regs = Grab<short, Reg>();

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Org>(p => p.Set(id));
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
            else
            {
                var m = await wc.ReadObjectAsync<Org>(0);
                using var dc = NewDbContext();
                dc.Sql("UPDATE orgs")._SET_(Org.Empty, 0).T(" WHERE id = @1");
                dc.Execute(p =>
                {
                    m.Write(p, 0);
                    p.Set(id);
                });
                wc.GivePane(200); // close
            }
        }

        [Ui("✕", "删除"), Tool(ButtonOpen, Appear.Small)]
        public async Task rm(WebContext wc)
        {
            int id = wc[0];
            if (wc.IsGet)
            {
                const bool ok = true;
                wc.GivePane(200, h =>
                {
                    h.ALERT("确定删除此项？");
                    h.FORM_().HIDDEN(nameof(ok), ok)._FORM();
                });
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("DELETE FROM orgs WHERE id = @1 AND typ = ").T(Org.TYP_SRC);
                await dc.ExecuteAsync(p => p.Set(id));

                wc.GivePane(200);
            }
        }

        [Ui("图标", icon: "github"), Tool(ButtonCrop, Appear.Small)]
        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon), false, 3);
        }
    }
}