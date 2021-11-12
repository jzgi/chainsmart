using System.Threading.Tasks;
using SkyChain.Web;
using static Revital.User;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class RegVarWork : WebWork
    {
    }

    public class AdmlyRegVarWork : RegVarWork
    {
        [UserAuthorize(admly: ADMLY_MGT)]
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
                dc.Sql("DELETE FROM regs WHERE id = @1");
                await dc.ExecuteAsync(p => p.Set(id));

                wc.GivePane(200);
            }
        }
    }
}