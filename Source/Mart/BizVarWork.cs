using System.Data;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Zhnt.Mart
{
    public class PubOrgVarWork : WebWork
    {
        /// <summary>
        /// The home page for the org
        /// </summary>
        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[0];
            var orgs = Fetch<Map<short, Biz>>();
            var org = orgs[orgid];

            using var dc = NewDbContext();
        }

        public async Task icon(WebContext wc)
        {
            short id = wc[0];
            using var dc = NewDbContext();
            if (await dc.QueryTopAsync("SELECT icon FROM orgs WHERE id = @1", p => p.Set(id)))
            {
                dc.Let(out byte[] bytes);
                if (bytes == null) wc.Give(204); // no content 
                else wc.Give(200, new StaticContent(bytes), shared: false, 60);
            }
            else wc.Give(404, shared: true, maxage: 3600 * 24); // not found
        }
    }


    public class BizlyBizVarWork : WebWork
    {
        [Ui("✎", "✎ 修改", group: 2), Tool(AnchorShow)]
        public async Task upd(WebContext wc)
        {
            var regs = Fetch<Map<short, Reg>>();
            var orgs = Fetch<Map<short, Biz>>();
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                var m = await wc.ReadObjectAsync<Biz>(0);
                using var dc = NewDbContext();
                dc.Sql("UPDATE orgs")._SET_(Biz.Empty, 0).T(" WHERE id = @1");
                dc.Execute(p =>
                {
                    m.Write(p, 0);
                    p.Set(id);
                });
                wc.GivePane(200); // close
            }
        }

        [Ui("☹", "☹ 负责人"), Tool(ButtonOpen)]
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
                            h.LI_().FIELD("用户姓名", o.name)._LI();
                            h._FIELDSUL();
                            h.BOTTOMBAR_().BUTTON("确认", nameof(mgr), 2)._BOTTOMBAR();
                        }
                    }
                    h._FORM();
                });
            }
            else // POST
            {
                short orgid = wc[0];
                int id = (await wc.ReadAsync<Form>())[nameof(id)];
                using var dc = NewDbContext(IsolationLevel.ReadCommitted);
                dc.Execute("UPDATE orgs SET mgrid = @1 WHERE id = @2", p => p.Set(id).Set(orgid));
                dc.Execute("UPDATE users SET orgid = @1, orgly = 3 WHERE id = @2", p => p.Set(orgid).Set(id));
                wc.GivePane(200); // ok
            }
        }
    }

    public class AdmlyBizVarWork : WebWork
    {
        [Ui("✎", "✎ 修改", group: 2), Tool(AnchorShow)]
        public async Task upd(WebContext wc)
        {
            var regs = Fetch<Map<short, Reg>>();
            var orgs = Fetch<Map<short, Biz>>();
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                var m = await wc.ReadObjectAsync<Biz>(0);
                using var dc = NewDbContext();
                dc.Sql("UPDATE orgs")._SET_(Biz.Empty, 0).T(" WHERE id = @1");
                dc.Execute(p =>
                {
                    m.Write(p, 0);
                    p.Set(id);
                });
                wc.GivePane(200); // close
            }
        }

        [Ui("☹", "☹ 负责人"), Tool(ButtonOpen)]
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
                            h.LI_().FIELD("用户姓名", o.name)._LI();
                            h._FIELDSUL();
                            h.BOTTOMBAR_().BUTTON("确认", nameof(mgr), 2)._BOTTOMBAR();
                        }
                    }
                    h._FORM();
                });
            }
            else // POST
            {
                short orgid = wc[0];
                int id = (await wc.ReadAsync<Form>())[nameof(id)];
                using var dc = NewDbContext(IsolationLevel.ReadCommitted);
                dc.Execute("UPDATE orgs SET mgrid = @1 WHERE id = @2", p => p.Set(id).Set(orgid));
                dc.Execute("UPDATE users SET orgid = @1, orgly = 3 WHERE id = @2", p => p.Set(orgid).Set(id));
                wc.GivePane(200); // ok
            }
        }
    }
}