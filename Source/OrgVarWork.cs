using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public abstract class OrgVarWork : WebWork
{
    public void @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var id = org?.id ?? wc[0]; // apply to both implicit and explicit cases
        var regs = Grab<short, Reg>();

        var m = GrabTwin<int, Org>(id);

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("商户名", m.name)._LI();
            h.LI_().FIELD("简介", m.tip)._LI();
            h.LI_().FIELD("工商登记名", m.legal)._LI();
            if (m.IsParent)
            {
                h.LI_().FIELD("延展名", m.ext)._LI();
            }

            h.LI_().FIELD("联系电话", m.tel).FIELD("区域", regs[m.regid])._LI();
            h.LI_().FIELD("联系地址", m.addr)._LI();
            h.LI_().FIELD("经度", m.x).FIELD("纬度", m.y)._LI();
            h.LI_().FIELD("指标参数", m.specs)._LI();
            h.LI_().FIELD("托管", m.trust)._LI();

            h.LI_().FIELD2("创编", m.created, m.creator)._LI();
            if (m.adapter != null) h.LI_().FIELD2("修改", m.adapted, m.adapter)._LI();
            if (m.oker != null) h.LI_().FIELD2("上线", m.oked, m.oker)._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: m.Status, state: m.State);
        }, false, 6);
    }


    protected async Task doimg(WebContext wc, string col, bool shared, int maxage)
    {
        int id = wc[0];

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").T(col).T(" FROM orgs WHERE id = @1");
            if (await dc.QueryTopAsync(p => p.Set(id)))
            {
                dc.Let(out byte[] bytes);
                if (bytes == null) wc.Give(204); // no content 
                else wc.Give(200, new WebStaticContent(bytes), shared, maxage);
            }
            else
            {
                wc.Give(404, null, shared, maxage); // not found
            }
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            ArraySegment<byte> img = f[nameof(img)];

            using var dc = NewDbContext();
            dc.Sql("UPDATE orgs SET ").T(col).T(" = @1 WHERE id = @2");
            if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
            {
                var m = GrabTwin<int, Org>(id);
                switch (col)
                {
                    case nameof(Org.icon):
                        m.icon = true;
                        break;
                    case nameof(Org.pic):
                        m.pic = true;
                        break;
                    case nameof(Org.m1):
                        m.m1 = true;
                        break;
                    case nameof(Org.m2):
                        m.m2 = true;
                        break;
                    case nameof(Org.m3):
                        m.m3 = true;
                        break;
                    case nameof(Org.m4):
                        m.m4 = true;
                        break;
                }

                wc.Give(200); // ok
            }
            else
                wc.Give(500); // internal server error
        }
    }
}

public class PublyOrgVarWork : OrgVarWork
{
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), true, 7200);
    }

    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), true, 7200);
    }

    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, nameof(m) + sub, true, 7200);
    }
}

public class AdmlyOrgVarWork : OrgVarWork
{
    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui(tip: "修改机构信息", icon: "pencil", status: 3), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var prin = (User)wc.Principal;
        var regs = Grab<short, Reg>();

        var topOrgs = GrabTwinSet<int, Org>(0);

        var m = GrabTwin<int, Org>(id);

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(m.EqMarket ? "市场机构" : "供应机构");

                h.LI_().TEXT("商户名", nameof(m.name), m.name, min: 2, max: 12, required: true)._LI();
                h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 40)._LI();
                h.LI_().TEXT("工商登记名", nameof(m.legal), m.legal, max: 20, required: true)._LI();
                h.LI_().TEXT("范围延展名", nameof(m.ext), m.ext, max: 12, required: true)._LI();
                h.LI_().SELECT("地市", nameof(m.regid), m.regid, regs, filter: (_, v) => v.IsCity, required: true)._LI();
                h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 30)._LI();
                h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                if (m.EqMarket)
                {
                    h.LI_().SELECT("关联中库", nameof(m.ctrid), m.ctrid, topOrgs, filter: v => v.EqCenter, required: true)._LI();
                }

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;

            await wc.ReadObjectAsync(msk, instance: m);
            m.adapted = DateTime.Now;
            m.adapter = prin.name;

            using var dc = NewDbContext();
            dc.Sql("UPDATE orgs")._SET_(Org.Empty, msk).T(" WHERE id = @1");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(id);
            });

            wc.GivePane(200); // close
        }
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui("管理", "设置管理人员", icon: "happy"), Tool(ButtonShow)]
    public async Task mgr(WebContext wc, int cmd)
    {
        if (wc.IsGet)
        {
            string tel = wc.Query[nameof(tel)];
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("指定用户");
                h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(mgr), 1, post: false, css: "uk-button-secondary")._LI();
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
                        if (o.supid > 0)
                        {
                            var org = GrabTwin<int, Org>(o.supid);
                            h.LI_().FIELD2("现有权限", org.name, User.Orgly[o.suply])._LI();
                        }
                        else
                        {
                            h.LI_().FIELD("现有权限", "无")._LI();
                        }

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

            using var dc = NewDbContext();
            dc.Sql("UPDATE users SET orgid = @1, orgly = ").T(User.ROL_MGT).T(" WHERE id = @2");
            await dc.ExecuteAsync(p => p.Set(orgid).Set(id));

            wc.GivePane(200); // ok
        }
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui(icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui(tip: "资料", icon: "album", status: 3), Tool(ButtonCrop, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, "m" + sub, false, 6);
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui("上线", "上线投入使用", icon: "cloud-upload", status: 3), Tool(ButtonConfirm, state: Org.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var prin = (User)wc.Principal;

        var m = GrabTwin<int, Org>(id);

        var now = DateTime.Now;
        lock (m)
        {
            m.status = 4;
            m.oked = now;
            m.oker = prin.name;
        }
        await GetGraph<OrgGraph, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 4, oked = @1, oker = @2 WHERE id = @3");
            return await dc.ExecuteAsync(p => p.Set(now).Set(prin.name).Set(id)) == 1;
        });

        wc.Give(205);
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui("下线", "下线以便修改", icon: "cloud-download", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];

        var m = GrabTwin<int, Org>(id);

        lock (m)
        {
            m.status = 1;
            m.oked = default;
            m.oker = null;
        }
        await GetGraph<OrgGraph, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 1, oked = NULL, oker = NULL WHERE id = @1");
            return await dc.ExecuteAsync(p => p.Set(id)) == 1;
        });

        wc.Give(205);
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui(tip: "确定删除此商户", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];

        var m = GrabTwin<int, Org>(id);

        await GetGraph<OrgGraph, int, Org>().RemoveAsync(m, async (dc) =>
        {
            dc.Sql("DELETE FROM orgs WHERE id = @1 AND typ = ").T(Org.TYP_RTL);
            return await dc.ExecuteAsync(p => p.Set(id)) == 1;
        });

        wc.Give(204); // no content
    }
}

public class MktlyOrgVarWork : OrgVarWork
{
    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(icon: "pencil", status: 3), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var regs = Grab<short, Reg>();

        var m = GrabTwin<int, Org>(id);

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(m.EqBrand ? "品牌信息" : "商户信息");

                if (m.typ == Org.TYP_RTL)
                {
                    h.LI_().TEXT("商户名", nameof(m.name), m.name, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 40)._LI();
                    h.LI_().TEXT("工商登记名", nameof(m.legal), m.legal, max: 20, required: true)._LI();
                    h.LI_().TEXT("联系电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h.LI_().SELECT("场区", nameof(m.regid), m.regid, regs, filter: (_, v) => v.IsSection)._LI();
                    h.LI_().TEXT("商户编号", nameof(m.addr), m.addr, max: 4)._LI();
                    h.LI_().CHECKBOX("委托办理", nameof(m.trust), true, m.trust)._LI();
                }
                else // brand
                {
                    h.LI_().TEXT("品牌名", nameof(m.name), m.name, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 40)._LI();
                    h.LI_().TEXT("链接地址", nameof(m.addr), m.addr, max: 50)._LI();
                }

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else
        {
            const short msk = MSK_EDIT;
            m = await wc.ReadObjectAsync(msk, instance: m);

            await GetGraph<OrgGraph, int, Org>().UpdateAsync(m, async dc =>
            {
                dc.Sql("UPDATE orgs_vw")._SET_(Org.Empty, msk).T(" WHERE id = @1");
                return await dc.ExecuteAsync(p =>
                {
                    m.Write(p, msk);
                    p.Set(id);
                }) == 1;
            });

            wc.GivePane(200); // close
        }
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "上传图标", icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 3);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "上传照片", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 3);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "上传影印", icon: "album", status: 3), Tool(ButtonCrop, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, "m" + sub, false, 3);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("上线", "上线投入使用", icon: "cloud-upload", status: 3), Tool(ButtonConfirm, state: Org.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        var m = GrabTwin<int, Org>(id);

        var now = DateTime.Now;
        lock (m)
        {
            m.status = 4;
            m.oked = now;
            m.oker = prin.name;
        }
        await GetGraph<OrgGraph, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND prtid = @4");
            return await dc.ExecuteAsync(p => p.Set(now).Set(prin.name).Set(id).Set(org.id)) == 1;
        });

        wc.Give(205);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("下线", "下线以便修改", icon: "cloud-download", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        var m = GrabTwin<int, Org>(id);

        lock (m)
        {
            m.status = 1;
            m.oked = default;
            m.oker = null;
        }
        await GetGraph<OrgGraph, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 1, oked = NULL, oker = NULL WHERE id = @1 AND prtid = @2");
            return await dc.ExecuteAsync(p => p.Set(id).Set(org.id)) == 1;
        });

        wc.Give(205);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "确定删除此商户", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        var m = GrabTwin<int, Org>(id);

        await GetGraph<OrgGraph, int, Org>().RemoveAsync(m, async (dc) =>
        {
            dc.Sql("DELETE FROM orgs WHERE id = @1 AND prtid = @2 AND status BETWEEN 1 AND 2");
            return await dc.ExecuteAsync(p => p.Set(id).Set(org.id)) == 1;
        });

        wc.Give(204); // no content
    }
}

public class CtrlyOrgVarWork : OrgVarWork
{
    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改供源信息", icon: "pencil", status: 3), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var regs = Grab<short, Reg>();
        var prin = (User)wc.Principal;

        var m = GrabTwin<int, Org>(id);

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();

                h.LI_().TEXT("商户名", nameof(m.name), m.name, max: 12, required: true)._LI();
                h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 40)._LI();
                h.LI_().TEXT("工商登记名", nameof(m.legal), m.legal, max: 20, required: true)._LI();
                h.LI_().SELECT("省份", nameof(m.regid), m.regid, regs, filter: (_, v) => v.IsProvince, required: true)._LI();
                h.LI_().TEXT("联系地址", nameof(m.addr), m.addr, max: 30)._LI();
                h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.0000, max: 180.0000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                h.LI_().TEXT("联系电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                h.LI_().CHECKBOX("委托代办", nameof(m.trust), true, m.trust)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else
        {
            const short msk = MSK_EDIT;
            await wc.ReadObjectAsync(msk, instance: m);
            m.adapted = DateTime.Now;
            m.adapter = prin.name;

            await GetGraph<OrgGraph, int, Org>().UpdateAsync(m, async dc =>
            {
                dc.Sql("UPDATE orgs_vw")._SET_(Org.Empty, msk).T(" WHERE id = @1");
                return await dc.ExecuteAsync(p =>
                {
                    m.Write(p, msk);
                    p.Set(id);
                }) == 1;
            });

            wc.GivePane(200); // close
        }
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 3);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("照片", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 3);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("资料", icon: "album", status: 3), Tool(ButtonCrop, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, nameof(m) + sub, false, 3);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("上线", "上线投入使用", icon: "cloud-upload", status: 3), Tool(ButtonConfirm, state: Org.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        var m = GrabTwin<int, Org>(id);

        var now = DateTime.Now;
        lock (m)
        {
            m.status = 4;
            m.oked = now;
            m.oker = prin.name;
        }
        await GetGraph<OrgGraph, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND prtid = @4");
            return await dc.ExecuteAsync(p => p.Set(now).Set(prin.name).Set(id).Set(org.id)) == 1;
        });

        wc.Give(205);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("下线", "下线以便修改", icon: "cloud-download", status: 4), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        var m = GrabTwin<int, Org>(id);

        lock (m)
        {
            m.status = 1;
            m.oked = default;
            m.oker = null;
        }
        await GetGraph<OrgGraph, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 1, oked = NULL, oker = NULL WHERE id = @1 AND prtid = @2");
            return await dc.ExecuteAsync(p => p.Set(id).Set(org.id)) == 1;
        });

        wc.Give(205);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "确定删除此商户", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        var m = GrabTwin<int, Org>(id);

        await GetGraph<OrgGraph, int, Org>().RemoveAsync(m, async (dc) =>
        {
            dc.Sql("DELETE FROM orgs WHERE id = @1 AND prtid = @2 AND status BETWEEN 1 AND 2");
            return await dc.ExecuteAsync(p => p.Set(id).Set(org.id)) == 1;
        });

        wc.Give(204); // no content
    }
}