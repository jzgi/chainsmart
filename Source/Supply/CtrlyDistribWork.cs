using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;

namespace Revital.Supply
{
    [UserAuthorize(orgly: User.ORGLY_OP)]
    public class CtrlyDistribWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyDistribVarWork>();
        }

        [Ui("已确认", kind: 1), Tool(Modal.Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }

        [Ui("已发货", kind: 2), Tool(Modal.Anchor)]
        public async Task shipped(WebContext wc, int page)
        {
            short orgid = wc[-1];
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }

        [Ui("发货", kind: 1), Tool(Modal.ButtonOpen)]
        public async Task @new(WebContext wc, int typ)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-1];
        }

        [Ui("复制", kind: 2), Tool(Modal.ButtonPickOpen)]
        public async Task copy(WebContext wc)
        {
            short orgid = wc[-1];
            var prin = (User) wc.Principal;
            var ended = DateTime.Today.AddDays(3);
            int[] key;
            if (wc.IsGet)
            {
                key = wc.Query[nameof(key)];
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("目标截止日期");
                    h.LI_().DATE("截止", nameof(ended), ended)._LI();
                    h._FIELDSUL();
                    h.HIDDENS(nameof(key), key);
                    h.BOTTOM_BUTTON("确认", nameof(copy));
                    h._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ended = f[nameof(ended)];
                key = f[nameof(key)];
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO lots (typ, status, orgid, issued, ended, span, name, tag, tip, unit, unitip, price, min, max, least, step, extern, addr, start, author, icon, img) SELECT typ, 0, orgid, issued, @1, span, name, tag, tip, unit, unitip, price, min, max, least, step, extern, addr, start, @2, icon, img FROM lots WHERE orgid = @3 AND id")._IN_(key);
                await dc.ExecuteAsync(p => p.Set(ended).Set(prin.name).Set(orgid).SetForIn(key));

                wc.GivePane(201);
            }
        }
    }

    [Ui("销售分拣管理", "sign-out")]
    public class CtrlyAgriDistribWork : CtrlyDistribWork
    {
    }

    [Ui("销售分拣管理", "sign-out")]
    public class CtrlyDietaryDistribWork : CtrlyDistribWork
    {
    }

    public class CtrlyHomeDistribWork : CtrlyDistribWork
    {
    }

    public class CtrlyCareDistribWork : CtrlyDistribWork
    {
    }

    public class CtrlyAdDistribWork : CtrlyDistribWork
    {
    }

    public class CtrlyCharityDistribWork : CtrlyDistribWork
    {
    }
}