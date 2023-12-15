using System;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Nodality;

namespace ChainSmart;

public abstract class SrcVarWork : WebWork
{
    protected async Task doimg(WebContext wc, string col, bool shared, int maxage)
    {
        int id = wc[0];
        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").T(col).T(" FROM srcs WHERE id = @1");
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
            dc.Sql("UPDATE srcs SET ").T(col).T(" = @1 WHERE id = @2");
            if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
            {
                var m = GrabTwin<int, Src>(id);
                lock (m)
                {
                    switch (col)
                    {
                        case nameof(Src.icon):
                            m.icon = true;
                            break;
                        case nameof(Src.pic):
                            m.pic = true;
                            break;
                        case nameof(Src.m1):
                            m.m1 = true;
                            break;
                        case nameof(Src.m2):
                            m.m2 = true;
                            break;
                        case nameof(Src.m3):
                            m.m3 = true;
                            break;
                        case nameof(Src.m4):
                            m.m4 = true;
                            break;
                    }
                }
                wc.Give(200); // ok
            }
            else
            {
                wc.Give(500); // internal server error
            }
        }
    }
}

public class PublySrcVarWork : SrcVarWork
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

    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, nameof(m) + sub, true, MAXAGE);
    }
}

public class SuplySrcVarWork : SrcVarWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Src.Empty).T(" FROM srcs_vw WHERE id = @1");
        var o = await dc.QueryTopAsync<Src>(p => p.Set(id));

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("产源设施名", o.name)._LI();
            h.LI_().FIELD("类别", o.typ, Src.Typs).FIELD("等级", o.rank, Src.Ranks)._LI();
            h.LI_().FIELD("简介语", o.tip)._LI();
            h.LI_().FIELD("说明", o.remark)._LI();
            h.LI_().FIELD("规格参数", o.specs)._LI();
            h.LI_().FIELD("碳积分因子", o.co2ekg)._LI();
            h.LI_().FIELD("经度", o.x).FIELD("纬度", o.y)._LI();

            h.LI_().FIELD("状态", o.status, Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "删除" : "上线", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 4);
    }

    [UserAuthorize(0, User.ROL_OPN, ulevel: 2)]
    [Ui(tip: "调整产源设施信息", icon: "pencil", status: 3), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var sup = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Src.Empty).T(" FROM srcs_vw WHERE id = @1");
            var o = dc.QueryTop<Src>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();

                h.LI_().TEXT("产源设施名", nameof(o.name), o.name, min: 2, max: 12)._LI();
                h.LI_().SELECT("类别", nameof(o.typ), o.typ, Src.Typs, required: true).SELECT("等级", nameof(o.rank), o.rank, Src.Ranks, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().TEXTAREA("说明", nameof(o.remark), o.remark, max: 200)._LI();
                h.LI_().TEXTAREA("规格", nameof(o.specs), o.specs, max: 300)._LI();
                h.LI_().NUMBER("碳积分因子", nameof(o.co2ekg), o.co2ekg, min: 0.00M, max: 99.99M)._LI();
                h.LI_().NUMBER("经度", nameof(o.x), o.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(o.y), o.y, min: -90.000, max: 90.000)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, new Src
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            await GetTwinCache<SrcCache, int, Src>().UpdateAsync(m, async dc =>
            {
                dc.Sql("UPDATE srcs_vw")._SET_(Src.Empty, msk).T(" WHERE id = @1 AND orgid = @2");
                return await dc.ExecuteAsync(p =>
                {
                    m.Write(p, msk);
                    p.Set(id).Set(sup.id);
                }) == 1;
            });

            wc.GivePane(200); // close dialog
        }
    }

    [UserAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "图标", icon: "github-alt", status: 3), Tool(ButtonCrop)]
    public async Task icon(WebContext wc)
    {
        await doimg(wc, nameof(icon), false, 6);
    }

    [UserAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "照片", icon: "image", status: 3), Tool(ButtonCrop, size: 2)]
    public async Task pic(WebContext wc)
    {
        await doimg(wc, nameof(pic), false, 6);
    }

    [UserAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "资料", icon: "album", status: 3), Tool(ButtonCrop, size: 3, subs: 4)]
    public async Task m(WebContext wc, int sub)
    {
        await doimg(wc, "m" + sub, false, 6);
    }

    [UserAuthorize(0, User.ROL_OPN)]
    [Ui("上线", "上线投入使用", status: 3), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        var m = GrabTwin<int, Src>(id);

        var now = DateTime.Now;
        lock (m)
        {
            m.status = 4;
            m.oked = now;
            m.oker = prin.name;
        }
        await GetTwinCache<SrcCache, int, Src>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE srcs SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4");
            return await dc.ExecuteAsync(p => p.Set(now).Set(prin.name).Set(id).Set(org.id)) == 1;
        });

        wc.Give(205);
    }

    [UserAuthorize(0, User.ROL_OPN)]
    [Ui("下线", "下线停用或调整", status: STU_OKED), Tool(ButtonConfirm)]
    public async Task unok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        var m = GrabTwin<int, Src>(id);

        lock (m)
        {
            m.status = 1;
            m.oked = default;
            m.oker = null;
        }
        await GetTwinCache<SrcCache, int, Src>().UpdateAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE srcs SET status = 1, oked = NULL, oker = NULL WHERE id = @1 AND orgid = @2");
            return await dc.ExecuteAsync(p => p.Set(id).Set(org.id)) == 1;
        });

        wc.Give(205);
    }

    [UserAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "删除作废此产品？", icon: "trash", status: 3), Tool(ButtonConfirm)]
    public async Task @void(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        var m = GrabTwin<int, Src>(id);

        await GetTwinCache<SrcCache, int, Src>().RemoveAsync(m, async (dc) =>
        {
            dc.Sql("UPDATE srcs SET status = 0, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4");
            return await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id)) == 1;
        });

        wc.Give(204); // no content
    }
}