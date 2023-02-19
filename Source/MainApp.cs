using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChainFX;
using static System.Data.IsolationLevel;

namespace ChainSMart
{
    public class MainApp : Application
    {
        public static string WwwUrl;

        public static string MgtUrl;


        // periodic polling and concluding ended lots 
        // static readonly Thread cycler = new Thread(Cycle);

        /// <summary>
        /// The entry point of the application.
        /// </summary>
        public static async Task Main(string[] args)
        {
            // start the concluder thead
            // cycler.Start();

            MapComposite<BuyLn>();
            MapComposite<StockOp>();

            CacheUp();

            const string STATIC_ROOT = "static";

            WwwUrl = CreateService<WwwService>("www", STATIC_ROOT).VisitUrl;

            MgtUrl = CreateService<MgtService>("mgt", STATIC_ROOT).VisitUrl;

            // CreateService<FedService>("fed", STATIC_ROOT);

            NoticeBot.Start();

            await StartAsync();
        }


        public static void CacheUp()
        {
            Cache(dc =>
                {
                    dc.Sql("SELECT ").collst(Cat.Empty).T(" FROM cats WHERE status > 0 ORDER BY idx");
                    return dc.Query<short, Cat>();
                }, 60 * 60 * 12
            );

            Cache(dc =>
                {
                    dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs ORDER BY typ, id");
                    return dc.Query<short, Reg>();
                }, 60 * 60 * 12
            );

            // upper level orgs
            Cache(dc =>
                {
                    dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ >= ").T(Org.TYP_DST).T(" ORDER BY regid");
                    return dc.Query<int, Org>();
                }, 60 * 15
            );

            // indivisual working items
            CacheObject<int, Item>((dc, id) =>
                {
                    dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items_vw WHERE id = @1");
                    return dc.QueryTop<Item>(p => p.Set(id));
                }, 60 * 60
            );

            // individual working orgs
            CacheObject<int, Org>((dc, id) =>
                {
                    dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1");
                    return dc.QueryTop<Org>(p => p.Set(id));
                }, 60 * 60
            );
        }

        static async void Cycle(object state)
        {
            var lst = new List<int>(64);
            while (true)
            {
                Thread.Sleep(60 * 1000);

                var now = DateTime.Now;
                // WAR("cycle: " + today);

                // to succeed
                lst.Clear();
                try
                {
                    using (var dc = NewDbContext())
                    {
                        dc.Sql("SELECT first(id), count(id) FROM buys WHERE status = 1 AND adapted < @1 GROUP BY shpid");
                        await dc.QueryAsync(p => p.Set(now));
                        while (dc.Next())
                        {
                            dc.Let(out int id);
                            lst.Add(id);
                        }
                    }
                    foreach (var lotid in lst)
                    {
                        using var dc = NewDbContext(ReadCommitted);
                        try
                        {
                        }
                        catch (Exception e)
                        {
                            dc.Rollback();
                            Err(e.Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    Err(nameof(Cycle) + ": " + e.Message);
                }

                // send short messages
            }
        }
    }
}