using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static System.Data.IsolationLevel;
using static SkyChain.Web.Modal;

namespace Supply
{
    public class AdmlyBuyVarWork : WebWork
    {
        [Ui(group: 1), Tool(ButtonOpen)]
        public async Task act(WebContext wc, int cmd)
        {
            int lotid = wc[0];
            var prin = (User) wc.Principal;

            if (wc.IsGet)
            {
                using var dc = NewDbContext();

                dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM lots_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Buy>(p => p.Set(lotid));
            }
            else // POST
            {
                if (cmd == 1)
                {
                    using var dc = NewDbContext(ReadCommitted);
                    try
                    {
                    }
                    catch
                    {
                        dc.Rollback();
                    }
                }
                wc.GivePane(200);
            }
        }
    }


    public class BizlyBuyVarWork : WebWork
    {
        [Ui, Tool(ButtonOpen)]
        public async Task act(WebContext wc, int cmd)
        {
            int lotid = wc[0];
            short orgid = wc[-2];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM lots_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Buy>(p => p.Set(lotid));
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                int[] key = f[nameof(key)];
                if (cmd == 1)
                {
                    using var dc = NewDbContext(ReadCommitted);
                    try
                    {
                    }
                    catch (Exception e)
                    {
                        ERR(e.Message);
                        dc.Rollback();
                    }
                }
                else
                {
                }

                wc.GiveRedirect(nameof(act));
            }
        }

        [Ui("修改", group: 1), Tool(ButtonOpen)]
        public async Task upd(WebContext wc)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-2];
            int id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM lots_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Buy>(p => p.Set(id));
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                short typ = f[nameof(typ)];
                var m = new Buy
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
    }

    public class CtrlyBuyVarWork : WebWork
    {
    }
}