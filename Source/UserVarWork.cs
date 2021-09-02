using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using Zhnt;
using static SkyChain.Web.Modal;

namespace Zhnt.Supply
{
    public class AdmlyUserVarWork : WebWork
    {
        [Ui("✎", "修改"), Tool(ButtonShow)]
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

    public class AdmlyAccessVarWork : WebWork
    {
        [UserAuthorize(admly: User.ADMLY_MGT)]
        [Ui("✕", "删除"), Tool(ButtonShow, Appear.Small)]
        public async Task rm(WebContext wc)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                var admly = (short) await dc.ScalarAsync("SELECT admly FROM users WHERE id = @1", p => p.Set(id));

                wc.GivePane(200, h =>
                {
                    if (admly < 7)
                    {
                        h.ALERT("删除人员权限？");
                        h.FORM_().HIDDEN(string.Empty, true)._FORM();
                    }
                    else
                    {
                        h.ALERT("不能删除主管理员权限");
                    }
                });
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("UPDATE users SET admly = 0 WHERE id = @1");
                await dc.ExecuteAsync(p => p.Set(id));

                wc.GivePane(200);
            }
        }
    }

    public class OrglyAccessVarWork : WebWork
    {
        [UserAuthorize(orgly: 3)]
        [Ui("✕", "删除"), Tool(ButtonShow, Appear.Small)]
        public async Task rm(WebContext wc)
        {
            short orgid = wc[-2];
            short id = wc[0];
            var org = Fetch<Map<short, Org>>()[orgid];
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    if (org.mgrid != id)
                    {
                        h.ALERT("删除人员权限？");
                        h.FORM_().HIDDEN(string.Empty, true)._FORM();
                    }
                    else
                    {
                        h.ALERT("不能删除主管理员权限");
                    }
                });
            }
            else
            {
                if (org.mgrid != id)
                {
                    using var dc = NewDbContext();
                    dc.Sql("UPDATE users SET orgid = NULL, orgly = 0 WHERE id = @1 AND orgid = @2");
                    await dc.ExecuteAsync(p => p.Set(id).Set(orgid));
                }
                wc.GivePane(200);
            }
        }
    }
    
    public class MrtlyUserVarWork : WebWork
    {
        [Ui("✎", "修改"), Tool(ButtonShow)]
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