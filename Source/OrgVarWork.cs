using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Storage;

namespace ChainSmart;

public abstract class OrgVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var id = org?.id ?? wc[0]; // apply to both implicit and explicit cases
        var regs = Grab<short, Reg>();

        var m = GrabTwin<int, Org>(id);

        using var dc = NewDbContext();

        dc.Sql("SELECT name FROM users WHERE ").T(m.AsWhl ? "supid = @1 AND suply = 63" : "mktid = @1 AND mktly = 63");
        await dc.QueryAsync(p => p.Set(id));
        var mgrs = dc.ToStringArray();

        wc.GivePane(200, h =>
        {
            lock (m)
            {
                h.H4("基本", css: "uk-padding");
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("名称", m.name)._LI();
                h.LI_().FIELD("简介语", m.tip)._LI();
                if (m.AsFrt)
                {
                    h.LI_().FIELD("工商主体", m.legal)._LI();
                }
                if (m.AsEst)
                {
                    h.LI_().FIELD("整体名", m.whole)._LI();
                }
                h.LI_().FIELD("联系电话", m.tel);
                if (m.regid > 0)
                {
                    h.FIELD(m.AsRtl ? "版块" : "区域", regs[m.regid]);
                }
                h._LI();
                h.LI_().FIELD(m.AsRtl ? "排位" : m.IsHomeOrg ? "链接" : "地址", m.addr)._LI();
                if (m.AsEst || m.IsSrc)
                {
                    h.LI_().FIELD("经度", m.x).FIELD("纬度", m.y)._LI();
                }
                if (m.AsFrt)
                {
                    h.LI_().FIELD("收款账名", m.bankacctname)._LI();
                    h.LI_().FIELD("收款账号", m.bankacct)._LI();
                }
                h._UL();

                h.H4("状态", css: "uk-padding");
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("托管", m.trust)._LI();
                h.LI_().FIELD("管理员", mgrs)._LI();
                h.LI_().FIELD("状态", m.status, Statuses).FIELD2("创建", m.creator, m.created, sep: "<br>")._LI();
                h.LI_().FIELD2("调整", m.adapter, m.adapted, sep: "<br>").FIELD2(m.IsVoid ? "删除" : "上线", m.oker, m.oked, sep: "<br>")._LI();

                h._UL();
            }

            h.TOOLBAR(subscript: m.IsRtlEst ? 1 : 0, bottom: true, status: m.Status, state: m.ToState());
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
                lock (m)
                {
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
    const int MAXAGE = 3600 * 12;

    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), true, MAXAGE);
    }

    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), true, MAXAGE);
    }

    public async Task img(WebContext wc)
    {
        await doimg(wc, nameof(img), true, MAXAGE);
    }

    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, nameof(m) + sub, true, MAXAGE);
    }

    public async Task scene(WebContext wc)
    {
        await doimg(wc, nameof(scene), true, MAXAGE);
    }
}

public class AdmlyEstVarWork : OrgVarWork
{
    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改成员机构", icon: "pencil", status: 3), Tool(ButtonShow)]
    public async Task upd(WebContext wc, int cmd)
    {
        int id = wc[0];
        var prin = (User)wc.Principal;
        var regs = Grab<short, Reg>();

        var m = GrabTwin<int, Org>(id);

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                lock (m)
                {
                    h.FORM_().FIELDSUL_(m.IsRtlEst ? "修改市场" : "修改品控云仓");

                    h.LI_().TEXT("名称", nameof(m.name), m.name, min: 2, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介语", nameof(m.tip), m.tip, max: 40)._LI();
                    h.LI_().TEXT("工商登记名", nameof(m.legal), m.legal, max: 20, required: true)._LI();
                    h.LI_().TEXT("总体名", nameof(m.whole), m.whole, max: 12, required: true)._LI();
                    h.LI_().SELECT("地市", nameof(m.regid), m.regid, regs, filter: (_, v) => v.IsCity, required: true)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 30)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    if (m.IsRtlEst)
                    {
                        var hubs = GrabTwinArray<int, Org>(0, x => x.IsHub);
                        h.LI_().SELECT("交付方式", nameof(m.mode), m.mode, Org.Modes, required: true).SELECT("关联云仓", nameof(m.hubid), m.hubid, hubs, required: true)._LI();
                    }
                    h.LI_().TEXT("联系电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true).CHECKBOX("托管", nameof(m.trust), true, m.trust)._LI();
                    h.LI_().TEXT("收款账名", nameof(m.bankacctname), m.bankacctname, tip: "工商银行账户名称", max: 20, required: true)._LI();
                    h.LI_().TEXT("收款账号", nameof(m.bankacct), m.bankacct, pattern: "[0-9]+", min: 19, max: 19, required: true)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(upd))._FORM();
                }
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            await wc.ReadObjectAsync(msk, instance: m);
            lock (m)
            {
                m.adapted = DateTime.Now;
                m.adapter = prin.name;
            }

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

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "场照", icon: "eye", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "全景", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task img(WebContext wc)
    {
        await doimg(wc, nameof(img), false, 6);
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "资料", icon: "album", status: 3), Tool(ButtonCrop, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, "m" + sub, false, 6);
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "设置管理员", icon: "user"), Tool(ButtonShow)]
    public async Task mgr(WebContext wc, int cmd)
    {
        int orgid = wc[0];
        var org = GrabTwin<int, Org>(orgid);

        if (wc.IsGet)
        {
            string tel = wc.Query[nameof(tel)];

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);
                h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(mgr), 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();
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

                        if (org.AsWhl && o.supid > 0)
                        {
                            var sup = GrabTwin<int, Org>(o.supid);
                            h.LI_().FIELD2("现有权限", sup.name, User.Roles[o.suply])._LI();
                        }
                        else if (org.AsRtl && o.mktid > 0)
                        {
                            var mkt = GrabTwin<int, Org>(o.mktid);
                            h.LI_().FIELD2("现有权限", mkt.name, User.Roles[o.mktly])._LI();
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
            int id = (await wc.ReadAsync<Form>())[nameof(id)];

            using var dc = NewDbContext();
            if (org.AsWhl)
            {
                dc.Sql("UPDATE users SET supid = @1, suply = ").T(User.ROL_MGT).T(" WHERE id = @2");
            }
            else
            {
                dc.Sql("UPDATE users SET mktid = @1, mktly = ").T(User.ROL_MGT).T(" WHERE id = @2");
            }
            await dc.ExecuteAsync(p => p.Set(orgid).Set(id));

            wc.GivePane(200); // send a closing pane
        }
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui("上线", "上线投入使用", status: 3), Tool(ButtonConfirm, state: Org.STA_OKABLE)]
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
        await GetTwinCache<OrgCache, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 4, oked = @1, oker = @2 WHERE id = @3");
            return await dc.ExecuteAsync(p => p.Set(now).Set(prin.name).Set(id)) == 1;
        });

        wc.Give(205);
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui("下线", "下线停用或调整", status: 4), Tool(ButtonConfirm)]
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
        await GetTwinCache<OrgCache, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 1, oked = NULL, oker = NULL WHERE id = @1");
            return await dc.ExecuteAsync(p => p.Set(id)) == 1;
        });

        wc.Give(205);
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "确定删除此商户", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];
        var m = GrabTwin<int, Org>(id);

        await GetTwinCache<OrgCache, int, Org>().RemoveAsync(m, async (dc) =>
        {
            dc.Sql("DELETE FROM orgs WHERE id = @1 AND typ = ").T(Org.TYP_RTL_);
            return await dc.ExecuteAsync(p => p.Set(id)) == 1;
        });

        wc.Give(204); // no content
    }
}

public class AdmlySupVarWork : OrgVarWork
{
    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "调整机构信息", icon: "pencil", status: 3), Tool(ButtonShow)]
    public async Task upd(WebContext wc)
    {
        int id = wc[0];
        var prin = (User)wc.Principal;
        var regs = Grab<short, Reg>();
        var m = GrabTwin<int, Org>(id);

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                lock (m)
                {
                    h.FORM_().FIELDSUL_("修改供应源");

                    h.LI_().SELECT("类型", nameof(m.typ), m.typ, Org.Typs, filter: (k, _) => Org.IsNonEstSupTyp(k), required: true)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, min: 2, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介语", nameof(m.tip), m.tip, max: 40)._LI();
                    h.LI_().TEXT("工商名", nameof(m.legal), m.legal, max: 20, required: true)._LI();
                    h.LI_().SELECT("地市", nameof(m.regid), m.regid, regs, filter: (_, v) => v.IsCity, required: true)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 30)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().TEXT("联系电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true).CHECKBOX("托管", nameof(m.trust), true, m.trust)._LI();
                    if (m.IsSup)
                    {
                        h.LI_().TEXT("收款账名", nameof(m.bankacctname), m.bankacctname, tip: "工商银行账户名称", max: 20)._LI();
                        h.LI_().TEXT("收款账号", nameof(m.bankacct), m.bankacct, pattern: "[0-9]+", min: 19, max: 19)._LI();
                    }
                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(upd))._FORM();
                }
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            await wc.ReadObjectAsync(msk, instance: m);
            lock (m)
            {
                m.adapted = DateTime.Now;
                m.adapter = prin.name;
            }

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

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "资料", icon: "album", status: 3), Tool(ButtonCrop, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, "m" + sub, false, 6);
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "设置管理员", icon: "user"), Tool(ButtonShow)]
    public async Task mgr(WebContext wc, int cmd)
    {
        int orgid = wc[0];

        if (wc.IsGet)
        {
            string tel = wc.Query[nameof(tel)];

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(mgr), 1, post: false, onclick: "formRefresh(this,event);", css: "uk-button-secondary")._LI();
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
                            h.LI_().FIELD2("现有权限", org.name, User.Roles[o.suply])._LI();
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
            int id = (await wc.ReadAsync<Form>())[nameof(id)];

            using var dc = NewDbContext();
            dc.Sql("UPDATE users SET supid = @1, suply = ").T(User.ROL_MGT).T(" WHERE id = @2");
            await dc.ExecuteAsync(p => p.Set(orgid).Set(id));

            wc.GivePane(200); // send a closing pane
        }
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui("上线", "上线投入使用", status: 3), Tool(ButtonConfirm, state: Org.STA_OKABLE)]
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
        await GetTwinCache<OrgCache, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 4, oked = @1, oker = @2 WHERE id = @3");
            return await dc.ExecuteAsync(p => p.Set(now).Set(prin.name).Set(id)) == 1;
        });

        wc.Give(205);
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui("下线", "下线停用或调整", status: 4), Tool(ButtonConfirm)]
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
        await GetTwinCache<OrgCache, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 1, oked = NULL, oker = NULL WHERE id = @1");
            return await dc.ExecuteAsync(p => p.Set(id)) == 1;
        });

        wc.Give(205);
    }

    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "确定删除此商户", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        int id = wc[0];
        var m = GrabTwin<int, Org>(id);

        await GetTwinCache<OrgCache, int, Org>().RemoveAsync(m, async (dc) =>
        {
            dc.Sql("DELETE FROM orgs WHERE id = @1 AND typ = ").T(Org.TYP_RTL_);
            return await dc.ExecuteAsync(p => p.Set(id)) == 1;
        });

        wc.Give(204); // no content
    }
}

public class MktlyOrgVarWork : OrgVarWork
{
    [MgtAuthorize(Org.TYP_MKV, User.ROL_OPN)]
    [Ui(icon: "pencil", status: 3), Tool(ButtonShow)]
    public async Task upd(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var regs = Grab<short, Reg>();
        var m = GrabTwin<int, Org>(id);

        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                lock (m)
                {
                    h.FORM_("uk-card uk-card-primary").FIELDSUL_(m.IsShp ? "修改成员商户" : "修改成员门店");

                    h.LI_().SELECT("版块", nameof(m.regid), m.regid, regs, filter: (_, v) => v.IsSector, required: true).TEXT("编址", nameof(m.addr), m.addr, max: 12)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 15, required: true)._LI();
                    h.LI_().TEXTAREA("简介语", nameof(m.tip), m.tip, max: 40)._LI();
                    h.LI_().TEXT("工商名", nameof(m.legal), m.legal, max: 20, required: true)._LI();
                    h.LI_().SELECT("模式", nameof(m.mode), m.mode, Org.Modes, required: true)._LI();
                    h.LI_().TEXT("联系电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true).CHECKBOX("托管", nameof(m.trust), true, m.trust)._LI();
                    h.LI_().TEXT("收款账号", nameof(m.bankacct), m.bankacct, pattern: "[0-9]+", min: 19, max: 19)._LI();
                    h.LI_().TEXT("收款户名", nameof(m.bankacctname), m.bankacctname, max: 20)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(upd))._FORM();
                }
            });
        }
        else
        {
            const short Msk = MSK_EDIT;
            await wc.ReadObjectAsync(Msk, instance: m);

            await GetTwinCache<OrgCache, int, Org>().UpdateAsync(m, async dc =>
            {
                dc.Sql("UPDATE orgs_vw")._SET_(Org.Empty, Msk).T(" WHERE id = @1");
                return await dc.ExecuteAsync(p =>
                {
                    m.Write(p, Msk);
                    p.Set(id);
                }) == 1;
            });

            wc.GivePane(200); // close
        }
    }

    [MgtAuthorize(Org.TYP_MKV, User.ROL_OPN)]
    [Ui(tip: "图标", icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 3);
    }

    [MgtAuthorize(Org.TYP_MKV, User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 3);
    }

    [MgtAuthorize(Org.TYP_MKV, User.ROL_OPN)]
    [Ui(tip: "资料", icon: "album", status: 3), Tool(ButtonCrop, size: 3, subs: 3)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, nameof(m) + sub, false, 3);
    }

    [MgtAuthorize(Org.TYP_MKV, User.ROL_OPN)]
    [Ui("上线", "上线投入使用", status: 3), Tool(ButtonConfirm, state: Org.STA_OKABLE)]
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
        await GetTwinCache<OrgCache, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND parentid = @4");
            return await dc.ExecuteAsync(p => p.Set(now).Set(prin.name).Set(id).Set(org.id)) == 1;
        });

        wc.Give(205);
    }

    [MgtAuthorize(Org.TYP_MKV, User.ROL_OPN)]
    [Ui("下线", "下线停用或调整", status: 4), Tool(ButtonConfirm)]
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
        await GetTwinCache<OrgCache, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 1, oked = NULL, oker = NULL WHERE id = @1 AND parentid = @2");
            return await dc.ExecuteAsync(p => p.Set(id).Set(org.id)) == 1;
        });

        wc.Give(205);
    }

    [MgtAuthorize(Org.TYP_MKV, User.ROL_OPN)]
    [Ui(tip: "确定禁用此商户", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task @void(WebContext wc)
    {
        int id = wc[0];
        var prin = (User)wc.Principal;
        var org = wc[-2].As<Org>();

        var now = DateTime.Now;

        var m = GrabTwin<int, Org>(id);
        lock (m)
        {
            m.status = 0;
            m.oked = now;
            m.oker = prin.name;
        }

        await GetTwinCache<OrgCache, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 0, oked = @1, oker = @2 WHERE id = @3 AND parentid = @4 AND status BETWEEN 1 AND 2");
            return await dc.ExecuteAsync(p => p.Set(now).Set(prin.name).Set(id).Set(org.id)) == 1;
        });

        wc.Give(204); // no content
    }

    [MgtAuthorize(Org.TYP_MKV, User.ROL_OPN)]
    [Ui(tip: "确定恢复此商户", icon: "reply", status: 0), Tool(ButtonConfirm)]
    public async Task unvoid(WebContext wc)
    {
        int id = wc[0];
        var prin = (User)wc.Principal;
        var org = wc[-2].As<Org>();

        var now = DateTime.Now;
        var m = GrabTwin<int, Org>(id);
        await GetTwinCache<OrgCache, int, Org>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE orgs SET status = 2, oked = NULL, oker = NULL, adapted = @1, adapter = @2 WHERE id = @3 AND parentid = @4 AND status BETWEEN 1 AND 2");
            return await dc.ExecuteAsync(p => p.Set(now).Set(prin.name).Set(id).Set(org.id)) == 1;
        });
        lock (m)
        {
            m.status = 2;
            m.oked = default;
            m.oker = null;
            m.adapted = now;
            m.adapter = prin.name;
        }

        wc.Give(204); // no content
    }
}