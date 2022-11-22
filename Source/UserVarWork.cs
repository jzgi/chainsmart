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
            var o = await dc.QueryTopAsync<User>(p => p.Set(uid));

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("姓名", o.name)._LI();
                h.LI_().FIELD("专业", User.Typs[o.typ])._LI();
                h.LI_().FIELD("电话", o.tel)._LI();
                h.LI_().FIELD("状态", Entity.Statuses[o.status])._LI();
                h.LI_().FIELD("平台权限", User.Admly[o.admly])._LI();
                h.LI_().FIELD("机构权限", User.Orgly[o.orgly])._LI();
                h._UL();

                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD2("创建", o.creator, o.created)._LI();
                h.LI_().FIELD2("更改", o.adapter, o.adapted)._LI();
                h._UL();

                h.TOOLBAR(bottom: true);
            });
        }
    }


    [Ui("信息及身份", "账号")]
    public class MyInfoVarWork : WebWork
    {
        public void @default(WebContext wc)
        {
            int uid = wc[-1];
            var o = (User) wc.Principal;

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("姓名", o.name)._LI();
                h.LI_().FIELD("专业", User.Typs[o.typ])._LI();
                h.LI_().FIELD("电话", o.tel)._LI();
                h.LI_().FIELD("状态", Entity.Statuses[o.status])._LI();
                h.LI_().FIELD("平台权限", User.Admly[o.admly])._LI();
                h.LI_().FIELD("机构权限", User.Orgly[o.orgly])._LI();
                h._UL();

                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD2("创建", o.creator, o.created)._LI();
                h.LI_().FIELD2("更改", o.adapter, o.adapted)._LI();
                h._UL();

                h.TOOLBAR(bottom: true);

                // spr and rvr
            }, false, 7);
        }

        [Ui("信息", icon: "pencil"), Tool(ButtonShow)]
        public async Task setg(WebContext wc)
        {
            const string PASSMASK = "t#0^0z4R4pX7";
            string name;
            string tel;
            string password;
            var prin = (User) wc.Principal;
            if (wc.IsGet)
            {
                name = prin.name;
                tel = prin.tel;
                password = string.IsNullOrEmpty(prin.credential) ? null : PASSMASK;
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");
                    h.LI_().TEXT("姓名", nameof(name), name, max: 8, min: 2, required: true)._LI();
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h._FIELDSUL().FIELDSUL_("操作密码（可选）");
                    h.LI_().PASSWORD("密码", nameof(password), password, max: 12, min: 3)._LI();
                    h._FIELDSUL()._FORM();

                    h.BOTTOM_BUTTON("确定", nameof(setg));
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                name = f[nameof(name)];
                tel = f[nameof(tel)];
                password = f[nameof(password)];
                string credential = string.IsNullOrEmpty(password) ? null :
                    password == PASSMASK ? prin.credential : MainUtility.ComputeCredential(tel, password);

                using var dc = NewDbContext();
                dc.Sql("UPDATE users SET name = CASE WHEN @1 IS NULL THEN name ELSE @1 END , tel = @2, credential = @3 WHERE id = @4 RETURNING ").collst(User.Empty);
                prin = await dc.QueryTopAsync<User>(p => p.Set(name).Set(tel).Set(credential).Set(prin.id));
                // refresh cookie
                wc.SetUserCookie(prin);

                wc.GivePane(200); // close
            }
        }
    }

    [Ui("身份和权限", "账号")]
    public class MyAccessVarWork : WebWork
    {
        public void @default(WebContext wc)
        {
            int uid = wc[-1];
            var o = (User) wc.Principal;

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("已知的身份权限");

                if (o.vip > 0)
                {
                    var org = GrabObject<int, Org>(o.vip);
                    h.LI_().FIELD("大客户", org.name)._LI();
                }

                if (o.admly > 0)
                {
                    h.LI_().FIELD(User.Admly[o.admly], "平台")._LI();
                }

                if (o.IsOrgly)
                {
                    var org = GrabObject<int, Org>(o.orgid);
                    h.LI_().FIELD(User.Orgly[o.orgly], org.name)._LI();
                }

                h._FIELDSUL()._FORM();

                h.TOOLBAR(bottom: true);

                // spr and rvr
            }, false, 6);
        }

        [Ui("刷新", "刷新身份权限", icon: "refresh"), Tool(ButtonConfirm)]
        public async Task refresh(WebContext wc)
        {
            int uid = wc[-1];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE id = @1");
            var o = await dc.QueryTopAsync<User>(p => p.Set(uid));

            // resend token cookie
            wc.SetUserCookie(o);

            wc.Give(200, shared: false, maxage: 12);
        }

        [Ui("委派", "上层机构的委派", icon: "info"), Tool(ButtonShow)]
        public async Task access(WebContext wc)
        {
            int uid = wc[-1];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE sprid = @1 OR rvrid = @1");
            var o = await dc.QueryTopAsync<Org>(p => p.Set(uid));

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD2("创建", o.creator, o.created)._LI();
                h.LI_().FIELD2("更改", o.adapter, o.adapted)._LI();
                h._UL();
                //
            });
        }
    }

    public class AdmlyUserVarWork : UserVarWork
    {
        [Ui("修改", icon: "pencil"), Tool(ButtonShow)]
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

            wc.Give(204); // no content
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

            wc.Give(204); // no content
        }
    }

    public class ShplyVipVarWork : UserVarWork
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