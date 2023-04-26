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
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var id = org?.id ?? wc[0]; // apply to both implicit and explicit cases
        var regs = Grab<short, Reg>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1");
        var o = await dc.QueryTopAsync<Org>(p => p.Set(id));

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("商户名", o.name)._LI();
            h.LI_().FIELD("简介", o.tip)._LI();
            h.LI_().FIELD("工商登记名", o.legal)._LI();
            if (o.IsParent)
            {
                h.LI_().FIELD("延展名", o.ext)._LI();
            }

            h.LI_().FIELD("联系电话", o.tel).FIELD("区域", regs[o.regid])._LI();
            h.LI_().FIELD("联系地址", o.addr)._LI();
            h.LI_().FIELD("经度", o.x).FIELD("纬度", o.y)._LI();
            h.LI_().FIELD("指标参数", o.specs)._LI();
            h.LI_().FIELD("托管", o.trust)._LI();

            h.LI_().FIELD2("创编", o.created, o.creator)._LI();
            if (o.adapter != null) h.LI_().FIELD2("修改", o.adapted, o.adapter)._LI();
            if (o.oker != null) h.LI_().FIELD2("上线", o.oked, o.oker)._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.State);
        }, false, 900);
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

[Ui("信息和参数", "常规")]
public class OrglySetgWork : OrgVarWork
{
    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("设置", icon: "cog"), Tool(ButtonShow, status: 3)]
    public async Task setg(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1");
            var m = await dc.QueryTopAsync<Org>(p => p.Set(org.id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("设置基本信息和参数");
                h.LI_().TEXTAREA("简介", nameof(org.tip), org.tip, max: 40)._LI();
                h.LI_().TEXT("联系电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(setg))._FORM();
            });
        }
        else
        {
            var o = await wc.ReadObjectAsync(instance: org); // use existing object

            using var dc = NewDbContext();
            // update the db record
            await dc.ExecuteAsync("UPDATE orgs SET tip = @1, tel = @2, adapted = @3, adapter = @4, status = 2 WHERE id = @5", p => p.Set(o.tip).Set(o.tel).Set(DateTime.Now).Set(prin.name).Set(org.id));

            wc.GivePane(200);
        }
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("上线", "上线投入使用", icon: "cloud-upload"), Tool(ButtonConfirm, status: 3, state: Org.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE orgs SET status = 4, oked = @1, oker = @2 WHERE id = @3");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(org.id));

        wc.GivePane(200);
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("下线", "下线以便修改", icon: "cloud-download"), Tool(ButtonConfirm, status: 4)]
    public async Task unok(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE orgs SET status = 2, oked = NULL, oker = NULL WHERE id = @1");
        await dc.ExecuteAsync(p => p.Set(org.id));

        wc.GivePane(200);
    }
}

public class AdmlyOrgVarWork : OrgVarWork
{
    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui(tip: "修改机构信息", icon: "pencil"), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        short id = wc[0];
        var prin = (User)wc.Principal;
        var regs = Grab<short, Reg>();
        var orgs = Grab<int, Org>();

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1");
            var m = dc.QueryTop<Org>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(m.IsMarket ? "市场机构" : "供应机构");

                h.LI_().TEXT("商户名", nameof(m.name), m.name, min: 2, max: 12, required: true)._LI();
                h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 40)._LI();
                h.LI_().TEXT("工商登记名", nameof(m.legal), m.legal, max: 20, required: true)._LI();
                h.LI_().TEXT("范围延展名", nameof(m.ext), m.ext, max: 12, required: true)._LI();
                h.LI_().SELECT("地市", nameof(m.regid), m.regid, regs, filter: (_, v) => v.IsCity, required: true)._LI();
                h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 30)._LI();
                h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                if (m.IsMarket)
                {
                    h.LI_().SELECT("关联中库", nameof(m.ctrid), m.ctrid, orgs, filter: (_, v) => v.IsCenter, required: true)._LI();
                }

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;

            var m = await wc.ReadObjectAsync(msk, new Org
            {
                adapted = DateTime.Now,
                adapter = prin.name
            });

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
                            var org = GrabObject<int, Org>(o.supid);
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
    [Ui(icon: "github-alt"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui(tip: "资料", icon: "album"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, "m" + sub, false, 6);
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui("上线", "上线投入使用", icon: "cloud-upload"), Tool(ButtonConfirm, status: 3, state: Org.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE orgs SET status = 4, oked = @1, oker = @2 WHERE id = @3");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id));

        wc.GivePane(200);
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui("下线", "下线以便修改", icon: "cloud-download"), Tool(ButtonConfirm, status: STU_OKED)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("UPDATE orgs SET status = 1 WHERE id = @1");
        await dc.ExecuteAsync(p => p.Set(id));

        wc.GivePane(200);
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui(tip: "确定删除此商户", icon: "trash"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("DELETE FROM orgs WHERE id = @1 AND typ = ").T(Org.TYP_RTL);
        await dc.ExecuteAsync(p => p.Set(id));

        wc.GivePane(200);
    }
}

public class MktlyOrgVarWork : OrgVarWork
{
    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(icon: "pencil"), Tool(ButtonShow, status: 3)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var regs = Grab<short, Reg>();

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1");
            var o = await dc.QueryTopAsync<Org>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();

                if (o.typ == Org.TYP_RTL)
                {
                    h.LI_().TEXT("商户名", nameof(o.name), o.name, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().TEXT("工商登记名", nameof(o.legal), o.legal, max: 20, required: true)._LI();
                    h.LI_().TEXT("联系电话", nameof(o.tel), o.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h.LI_().SELECT("场区", nameof(o.regid), o.regid, regs, filter: (_, v) => v.IsSection)._LI();
                    h.LI_().TEXT("商户编号", nameof(o.addr), o.addr, max: 4)._LI();
                    h.LI_().CHECKBOX("委托办理", nameof(o.trust), true, o.trust)._LI();
                }
                else // brand
                {
                    h.LI_().TEXT("品牌名", nameof(o.name), o.name, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().TEXT("链接地址", nameof(o.addr), o.addr, max: 50)._LI();
                    // h.LI_().SELECT("状态", nameof(m.state), m.state, States, filter: (k, v) => k >= 0)._LI();
                }

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else
        {
            const short msk = MSK_EDIT;
            var m = await wc.ReadObjectAsync<Org>(msk);

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

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "上传图标", icon: "github-alt"), Tool(ButtonCrop, status: 3)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 3);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "上传照片", icon: "image"), Tool(ButtonCrop, status: 3, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 3);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "上传影印", icon: "album"), Tool(ButtonCrop, status: 3, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, "m" + sub, false, 3);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("上线", "上线投入使用", icon: "cloud-upload"), Tool(ButtonConfirm, status: 3, state: Org.STA_OKABLE)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE orgs SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND prtid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("下线", "下线以便修改", icon: "cloud-download"), Tool(ButtonConfirm, status: 4)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE orgs SET status = 1, oked = NULL, oker = NULL WHERE id = @1 AND prtid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "确定删除此商户", icon: "trash"), Tool(ButtonConfirm, status: 3)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("UPDATE orgs SET status = 0 WHERE id = @1 AND typ = ").T(Org.TYP_RTL);
        await dc.ExecuteAsync(p => p.Set(id));

        wc.GivePane(200);
    }
}

public class CtrlyOrgVarWork : OrgVarWork
{
    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改供源信息", icon: "pencil"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var regs = Grab<short, Reg>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1");
            var m = await dc.QueryTopAsync<Org>(p => p.Set(id));

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
            var m = await wc.ReadObjectAsync(msk, instance: new Org
            {
                adapted = DateTime.Now,
                adapter = prin.name
            });

            using var dc = NewDbContext();
            dc.Sql("UPDATE orgs")._SET_(Org.Empty, msk).T(" WHERE id = @1");
            dc.Execute(p =>
            {
                m.Write(p, msk);
                p.Set(id);
            });

            wc.GivePane(200); // close
        }
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(icon: "github-alt"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 3);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("照片", icon: "image"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 3);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("资料", icon: "album"), Tool(ButtonCrop, status: STU_CREATED | STU_ADAPTED, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, nameof(m) + sub, false, 3);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "确定删除此供源", icon: "trash"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("DELETE FROM orgs WHERE id = @1 AND typ = ").T(Org.TYP_SUP);
        await dc.ExecuteAsync(p => p.Set(id));

        wc.Give(204);
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("上线", "上线投入使用", icon: "cloud-upload"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE orgs SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND prtid = @4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.GivePane(200);
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("下线", "下线以便修改", icon: "cloud-download"), Tool(ButtonConfirm, status: STU_OKED)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("UPDATE orgs SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND prtid = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.GivePane(200);
    }
}