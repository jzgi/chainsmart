using System.Data;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Zhnt.Supply
{
    public class AdmlyOrgVarWork : WebWork
    {
        [Ui("✎", "✎ 修改", group: 2), Tool(AnchorShow)]
        public async Task upd(WebContext wc)
        {
            var regs = Fetch<Map<short, Reg>>();
            var orgs = Fetch<Map<short, Org>>();
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1");
                var m = dc.QueryTop<Org>(p => p.Set(id));
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("服务方属性");
                    h.LI_().SELECT("服务类型", nameof(m.typ), m.typ, Org.Typs)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXT("标语", nameof(m.tip), m.tip, max: 16)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, _Art.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
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

    public class MrtlyBizVarWork : WebWork
    {
    }
}