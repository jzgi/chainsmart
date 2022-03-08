using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;

namespace Revital
{
    public abstract class DailyWork : WebWork
    {
    }

    [UserAuthorize(admly: User.ADMLY_)]
    [Ui("平台业务日报")]
    public class AdmlyDailyWork : DailyWork
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [UserAuthorize(Org.TYP_MRT, User.ORGLY_)]
    [Ui("市场业务日报")]
    public class MrtlyDailyWork : DailyWork
    {
        protected override void OnCreate()
        {
        }

        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM dailys WHERE orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = dc.Query<Daily>(p => p.Set(org.id).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD(Daily.Typs[o.typ]);
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD_().T(o.count).SP().T('笔')._TD();
                    h.TD(o.amt, currency: true);
                });
                h.PAGINATION(arr?.Length == 30);
            }, false, 3);
        }

        [UserAuthorize(Org.TYP_MRT, User.ORGLY_FIN)]
        [Ui("生成"), Tool(Modal.ButtonShow)]
        public async Task gen(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-1];
            DateTime date;
            short typ = 0;
            decimal amt = 0;
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("指定统计区间");
                    h.LI_().DATE("从日期", nameof(date), DateTime.Today, required: true)._LI();
                    h.LI_().DATE("到日期", nameof(date), DateTime.Today, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                date = f[nameof(date)];
                date = f[nameof(date)];
                wc.GivePane(200); // close dialog
            }
        }
    }

    [UserAuthorize(Org.TYP_SRC, User.ORGLY_)]
    [Ui("产源业务日报")]
    public class SrclyDailyWork : DailyWork
    {
        protected override void OnCreate()
        {
        }

        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM dailys WHERE orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = dc.Query<Daily>(p => p.Set(org.id).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD(Daily.Typs[o.typ]);
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD_().T(o.count).SP().T('笔')._TD();
                    h.TD(o.amt, currency: true);
                });
                h.PAGINATION(arr?.Length == 30);
            }, false, 3);
        }

        [UserAuthorize(Org.TYP_SRC, User.ORGLY_FIN)]
        [Ui("生成"), Tool(Modal.ButtonShow)]
        public async Task gen(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-1];
            DateTime date;
            short typ = 0;
            decimal amt = 0;
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("指定统计区间");
                    h.LI_().DATE("从日期", nameof(date), DateTime.Today, required: true)._LI();
                    h.LI_().DATE("到日期", nameof(date), DateTime.Today, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                date = f[nameof(date)];
                date = f[nameof(date)];
                wc.GivePane(200); // close dialog
            }
        }
    }
}