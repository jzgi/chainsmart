using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static System.Data.IsolationLevel;
using static SkyChain.Web.Modal;

namespace Revital.Supply
{
    public class AdmlyProdVarWork : WebWork
    {
        public async Task @default(WebContext wc, int page)
        {
        }
    }

    public class CtrlyProdVarWork : WebWork
    {
    }

    public class SrclyProdVarWork : WebWork
    {
        [Ui("修改", group: 1), Tool(ButtonOpen)]
        public async Task upd(WebContext wc)
        {
            var prin = (User_) wc.Principal;
            short orgid = wc[-2];
            var org = Obtain<short, Org_>(orgid);
            int id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Purchase.Empty).T(" FROM lots_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Purchase>(p => p.Set(id));
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                short typ = f[nameof(typ)];
                var m = new Purchase
                {
                };
                m.Read(f);
                using var dc = NewDbContext();
                dc.Sql("UPDATE lots ")._SET_(m, 0).T(" WHERE id = @1");
                await dc.ExecuteAsync(p =>
                {
                    m.Write(p, 0);
                    p.Set(id);
                });

                wc.GivePane(201);
            }
        }

        [Ui("图标", group: 1), Tool(ButtonCrop, Appear.Small)]
        public async Task icon(WebContext wc)
        {
            int id = wc[0];
            if (wc.IsGet)
            {
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ArraySegment<byte> img = f[nameof(img)];
                using var dc = NewDbContext();
                if (await dc.ExecuteAsync("UPDATE lots SET icon = @1 WHERE id = @2", p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else wc.Give(500); // internal server error
            }
        }

        [Ui("图片", group: 1), Tool(ButtonCrop, Appear.Small)]
        public async Task img(WebContext wc)
        {
            int id = wc[0];
            if (wc.IsGet)
            {
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ArraySegment<byte> img = f[nameof(img)];
                using var dc = NewDbContext();
                if (await dc.ExecuteAsync("UPDATE lots SET img = @1 WHERE id = @2", p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else wc.Give(500); // internal server error
            }
        }

        // [Ui("核实"), Tool(Modal.ButtonShow)]
        public async Task apprv(WebContext wc)
        {
            short orgid = wc[-2];
            var org = Obtain<short, Org_>(orgid);
            long job = wc[0];
            bool ok;
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("核实申请");
                    h.LI_().CHECKBOX(null, nameof(ok), true, tip: "我确定此项申请情况属实，同意奖励数字珍珠", required: true)._LI();
                    h._FORM()._FIELDSUL();
                });
            }
            else
            {
                ok = (await wc.ReadAsync<Form>())[nameof(ok)];
                if (ok)
                {
                    using var dc = NewDbContext(ReadCommitted);

                    // var tx = new ChainTransaction(1)
                    // .Row()

                    // await dc.ExecuteAsync("", org.Acct);
                }
                wc.GivePane(200);
            }
        }
    }

    public class SrcColyProdVarWork : WebWork
    {
    }
}