using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Application;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public abstract class BookVarWork : WebWork
    {
    }

    public class ShplyBookVarWork : BookVarWork
    {
        public async Task @default(WebContext wc)
        {
            int lotid = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1");
            var m = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

            wc.GivePane(200, h =>
            {
                h.PIC_()._PIC();
                h.DIV_("uk-card uk-card-default");

                h._DIV();
            });
        }
    }

    public class SrclyBookVarWork : BookVarWork
    {
        [Ui, Tool(ButtonOpen)]
        public async Task act(WebContext wc, int cmd)
        {
            int lotid = wc[0];
            short orgid = wc[-2];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Book.Empty).T(" FROM lots_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Book>(p => p.Set(lotid));
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                int[] key = f[nameof(key)];
                if (cmd == 1)
                {
                    using var dc = NewDbContext(IsolationLevel.ReadCommitted);
                    try
                    {
                    }
                    catch (Exception e)
                    {
                        Err(e.Message);
                        dc.Rollback();
                    }
                }
                else
                {
                }

                wc.GiveRedirect(nameof(act));
            }
        }

        [Ui("修改", @group: 1), Tool(ButtonOpen)]
        public async Task upd(WebContext wc)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-2];
            int id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Book.Empty).T(" FROM lots_vw WHERE id = @1");
                var m = await dc.QueryTopAsync<Book>(p => p.Set(id));
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                short typ = f[nameof(typ)];
                var m = new Book
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
    }

    public class CtrlyBookVarWork : BookVarWork
    {
    }


    public class MrtlyBookVarWork : BookVarWork
    {
    }
}