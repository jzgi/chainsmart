using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;

namespace Revital
{
    public abstract class OrglyVarWork : WebWork
    {
        [UserAuthorize(orgly: 15)]
        [Ui("操作权限"), Tool(Modal.ButtonOpen)]
        public async Task acl(WebContext wc, int cmd)
        {
            var org = wc[0].As<Org>();
            short orgly = 0;
            int id = 0;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE orgid = @1 AND orgly > 0");
            var arr = dc.Query<User>(p => p.Set(org.id));

            if (wc.IsGet)
            {
                string tel = wc.Query[nameof(tel)];
                wc.GivePage(200, h =>
                {
                    h.TABLE(arr, o =>
                    {
                        h.TD(o.name);
                        h.TD(o.tel);
                        h.TD(User.Orgly[o.orgly]);
                        h.TDFORM(() =>
                        {
                            h.HIDDEN(nameof(id), o.id);
                            h.TOOL(nameof(acl), caption: "✕", subscript: 2, tool: ToolAttribute.BUTTON_CONFIRM, css: "uk-button-secondary");
                        });
                    }, caption: "现有操作权限");

                    h.FORM_().FIELDSUL_("授权目标用户");
                    if (cmd == 0)
                    {
                        h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(acl), 1, post: false, css: "uk-button-secondary")._LI();
                    }
                    else if (cmd == 1) // search user
                    {
                        // get user by tel
                        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                        var o = dc.QueryTop<User>(p => p.Set(tel));
                        if (o != null)
                        {
                            h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(acl), 1, post: false, css: "uk-button-secondary")._LI();
                            h.LI_().FIELD("用户姓名", o.name)._LI();
                            h.LI_().SELECT("权限", nameof(orgly), orgly, User.Orgly, filter: (k, v) => k > 0, required: true)._LI();
                            h.LI_("uk-flex uk-flex-center").BUTTON("确认", nameof(acl), 2)._LI();
                            h.HIDDEN(nameof(o.id), o.id);
                        }
                    }
                    h._FIELDSUL()._FORM();
                }, false, 3);
            }
            else
            {
                var f = await wc.ReadAsync<Form>();
                id = f[nameof(id)];
                orgly = f[nameof(orgly)];
                dc.Execute("UPDATE users SET orgid = @1, orgly = @2 WHERE id = @3", p => p.Set(org.id).Set(orgly).Set(id));
                wc.GiveRedirect(nameof(acl)); // ok
            }
        }

        [Ui("运行设置"), Tool(Modal.ButtonShow)]
        public async Task setg(WebContext wc)
        {
            var org = wc[0].As<Org>();
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("修改基本设置");
                    h.LI_().TEXT("标语", nameof(org.tip), org.tip, max: 16)._LI();
                    h.LI_().TEXT("地址", nameof(org.addr), org.addr, max: 16)._LI();
                    h.LI_().SELECT("状态", nameof(org.status), org.status, Info.Statuses, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else
            {
                var o = await wc.ReadObjectAsync(instance: org); // use existing object
                using var dc = NewDbContext();
                // update the db record
                await dc.ExecuteAsync("UPDATE orgs SET tip = @1, cttid = CASE WHEN @2 = 0 THEN NULL ELSE @2 END, status = @3 WHERE id = @4",
                    p => p.Set(o.tip).Set(o.status).Set(org.id));

                wc.GivePane(200);
            }
        }
    }
}