using System;
using System.Data;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class OrgVarWork : WebWork
    {
        protected async Task doimg(WebContext wc, string col)
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
                    else wc.Give(200, new StaticContent(bytes), shared: false, 60);
                }
                else
                    wc.Give(404, shared: true, maxage: 3600 * 24); // not found
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


    public class AdmlyOrgVarWork : OrgVarWork
    {
        public async Task @default(WebContext wc, int typ)
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
                    var typname = Org.Typs[m.typ];
                    h.FORM_().FIELDSUL_(typname + "机构信息");
                    // if (typ == TYP_CHL)
                    // {
                    //     h.LI_().SELECT("业务分属", nameof(m.fork), m.fork, Item.Typs, required: true)._LI();
                    // }
                    h.LI_().TEXT(typname + "名称", nameof(m.name), m.name, min: 2, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    if (m.IsPrv) h.LI_().SELECT("物流分支", nameof(m.fork), m.fork, Org.Forks, required: true)._LI();
                    h.LI_().SELECT(m.HasLocality ? "所在地市" : "所在省份", nameof(m.regid), m.regid, regs, filter: (k, v) => m.HasLocality ? v.IsDist : v.IsProv, required: !m.IsPrv)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    if (m.HasXy) h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    if (m.IsMrt) h.LI_().SELECT("关联中枢", nameof(m.sprid), m.sprid, orgs, filter: (k, v) => v.IsCtr, required: true)._LI();
                    if (m.IsSrc) h.LI_().SELECT("辐射地市", nameof(m.dists), m.dists, regs, filter: (k, v) => v.IsDist, size: 10)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, _Info.Statuses, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var m = await wc.ReadObjectAsync(_Info.DUAL, new Org
                {
                    typ = (short) typ,
                    adapted = DateTime.Now,
                    adapter = prin.name
                });
                using var dc = NewDbContext();
                dc.Sql("UPDATE orgs")._SET_(Org.Empty, _Info.DUAL).T(" WHERE id = @1");
                dc.Execute(p =>
                {
                    m.Write(p, _Info.DUAL);
                    p.Set(id);
                });
                wc.GivePane(200); // close
            }
        }

        [Ui("웃", "设置负责人", group: 7), Tool(ButtonOpen, Appear.Small)]
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

        [Ui("◑", "机构图标", group: 7), Tool(ButtonCrop, Appear.Small)]
        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon));
        }

        [Ui("▤", "营业执照", group: 7), Tool(ButtonCrop, Appear.Large)]
        public async Task cert(WebContext wc)
        {
            await doimg(wc, nameof(cert));
        }
    }


    public class MrtlyOrgVarWork : OrgVarWork
    {
    }

    public class PrvlyOrgVarWork : OrgVarWork
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
                    h.FORM_().FIELDSUL_("产源信息");
                    h.LI_().TEXT("主体名称", nameof(m.name), m.name, max: 10, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().SELECT("物流分支", nameof(m.fork), m.fork, Org.Forks, required: true)._LI();
                    h.LI_().SELECT("所在省份", nameof(m.regid), m.regid, regs, filter: (k, v) => v.IsProv, required: true)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().TEXT("电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h.LI_().SELECT("状态", nameof(m.status), m.status, _Info.Statuses).CHECKBOX("委托代办", nameof(m.trust), m.trust)._LI();
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

        [Ui("✕", "删除"), Tool(ButtonShow, Appear.Small)]
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
    }
}