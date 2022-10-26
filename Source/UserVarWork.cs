using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class UserVarWork : WebWork
    {
        public async Task @default(WebContext wc)
        {
            int uid = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users_vw WHERE id = @1");
            var m = await dc.QueryTopAsync<User>(p => p.Set(uid));

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("姓名", m.name)._LI();
                h.LI_().FIELD("类型", User.Typs[m.typ])._LI();
                h.LI_().FIELD("电话", m.tel)._LI();
                h.LI_().FIELD("状态", Entity.Statuses[m.status])._LI();
                h.LI_().FIELD("平台权限", User.Admly[m.admly])._LI();
                h.LI_().FIELD("机构权限", User.Orgly[m.orgly])._LI();
                h._UL();

                h.TOOLBAR(bottom: true);
            });
        }
    }

    public class AdmlyUserVarWork : UserVarWork
    {
        [Ui("修改", icon: "pencil"), Tool(ButtonOpen)]
        public async Task upd(WebContext wc)
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
                    h.LI_().SELECT("专业类型", nameof(typ), typ, User.Typs)._LI();
                    h._FIELDSUL()._FORM();
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
    }

    public class AdmlyAccessVarWork : UserVarWork
    {
        [Ui("删除", "确认删除此权限？", icon: "trash"), Tool(ButtonConfirm)]
        public async Task rm(WebContext wc)
        {
            short uid = wc[0];

            using var dc = NewDbContext();
            dc.Sql("UPDATE users SET admly = NULL WHERE id = @1");
            await dc.ExecuteAsync(p => p.Set(uid).Set(uid));

            wc.GivePane(200);
        }
    }

    public class OrglyAccessVarWork : UserVarWork
    {
        [Ui("删除", icon: "trash"), Tool(ButtonConfirm)]
        public async Task rm(WebContext wc)
        {
            short orgid = wc[-2];
            short id = wc[0];
            var org = GrabObject<int, Org>(orgid);
            if (org.sprid != id)
            {
                using var dc = NewDbContext();
                dc.Sql("UPDATE users SET orgid = NULL, orgly = 0 WHERE id = @1 AND orgid = @2");
                await dc.ExecuteAsync(p => p.Set(id).Set(orgid));
            }
            wc.GivePane(200);
        }
    }

    public class MktlyCustVarWork : UserVarWork
    {
        [Ui("✎", "修改"), Tool(ButtonOpen)]
        public async Task upd(WebContext wc)
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
                    h.LI_().SELECT("专业类型", nameof(typ), typ, User.Typs)._LI();
                    h._FIELDSUL()._FORM();
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
    }
}