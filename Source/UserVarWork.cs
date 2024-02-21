using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Storage;

namespace ChainSmart;

public abstract class UserVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        int uid = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users_vw WHERE id = @1");
        var o = await dc.QueryTopAsync<User>(p => p.Set(uid));

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("用户名", o.name)._LI();
            h.LI_().FIELD("专业", User.Typs[o.typ])._LI();
            h.LI_().FIELD("电话", o.tel)._LI();
            h.LI_().FIELD("平台权限", User.Roles[o.admly])._LI();
            h.LI_().FIELD("机构权限", User.Roles[o.suply])._LI();

            h.LI_().FIELD("状态", o.status, User.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "删除" : "上线", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true);
        });
    }
}

public class AdmlyUserVarWork : UserVarWork
{
    [Ui(tip: "调整用户信息", icon: "pencil"), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        short typ;
        int id = wc[0];
        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            await dc.QueryTopAsync("SELECT typ FROM users WHERE id = @1", p => p.Set(id));
            dc.Let(out typ);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("设置专业类型");
                h.LI_().SELECT("专业类型", nameof(typ), typ, User.Typs, required: true)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(msg))._FORM();
            });
        }
        else
        {
            var f = (await wc.ReadAsync<Form>());
            typ = f[nameof(typ)];

            using var dc = NewDbContext();
            dc.Sql("UPDATE users SET typ = @1 WHERE id = @2");
            await dc.ExecuteAsync(p => p.Set(typ).Set(id));

            wc.GivePane(200);
        }
    }

    [Ui("消息", icon: "mail"), Tool(ButtonShow)]
    public async Task msg(WebContext wc)
    {
        int id = wc[0];
        string text = null;
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("设置专业类型");
                h.LI_().TEXT("消息", nameof(text), text, min: 2, max: 20, required: true)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(msg))._FORM();
            });
        }
        else // POST
        {
            using var dc = NewDbContext();
            await dc.QueryTopAsync("SELECT im FROM users WHERE id = @1", p => p.Set(id));
            dc.Let(out string im);

            var f = await wc.ReadAsync<Form>();
            text = f[nameof(text)];

            await WeChatUtility.PostSendAsync(im, text);

            wc.GivePane(200);
        }
    }
}

public class AdmlyAccessVarWork : UserVarWork
{
    [MgtAuthorize(-1, User.ROL_MGT)]
    [Ui(tip: "删除此人员权限", icon: "trash"), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        int uid = wc[0];

        using var dc = NewDbContext();
        dc.Sql("UPDATE users SET admly = 0 WHERE id = @1");
        await dc.ExecuteAsync(p => p.Set(uid));

        wc.Give(204); // no content
    }
}

public class OrglyMbrVarWork : UserVarWork
{
    bool IsShop => (bool)Parent.State;

    [MgtAuthorize(0, User.ROL_MGT)]
    [Ui(tip: "删除此人员权限", icon: "trash"), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        var org = wc[-2].As<Org>();
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("UPDATE users SET ").T(IsShop ? "rtlid" : "supid").T(" = NULL, ").T(IsShop ? "rtlly" : "suply").T(" = 0 WHERE id = @1 AND ").T(IsShop ? "rtlid" : "supid").T(" = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.Give(204); // no content
    }
}

public class RtllyVipVarWork : UserVarWork
{
    [MgtAuthorize(Org.TYP_RTL_, User.ROL_MGT)]
    [Ui(tip: "删除VIP身份", icon: "trash"), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        var org = wc[-2].As<Org>();
        short id = wc[0];

        using var dc = NewDbContext(); // NOTE array_length() of empty array is NULL
        dc.Sql("UPDATE users SET vip = CASE WHEN array_length(array_remove(vip, @1), 1) IS NULL THEN NULL ELSE array_remove(vip, @1) END WHERE id = @2");
        await dc.ExecuteAsync(p => p.Set(org.id).Set(id));

        wc.Give(204); // no content
    }
}