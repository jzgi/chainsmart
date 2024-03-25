using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChainFX;
using static System.Data.IsolationLevel;

namespace ChainSmart;

public class MainApp : Application
{
    public static string WwwUrl;

    public static string MgtUrl;


    // periodic polling and concluding ended lots 
    // static readonly Thread cycler = new Thread(Cycle);

    /**
     * The entry point of the application.
     */
    public static async Task Main(string[] args)
    {
        // start the concluder thead
        // cycler.Start();

        //
        // db and caches and graphs

        MapCompositeDbType<BuyLn>();
        MapCompositeDbType<Bat>();

        SetupCaches();

        //
        // create web services

        const string STATIC_ROOT = "static";

        WwwUrl = CreateService<WwwService>("www", STATIC_ROOT).PublicUrl;
        MgtUrl = CreateService<MgtService>("mgt", STATIC_ROOT).PublicUrl;


        await StartAsync();
    }


    public static void SetupCaches()
    {
        MakeCache(dc =>
            {
                dc.Sql("SELECT ").collst(Std.Empty).T(" FROM cats WHERE status > 0 ORDER BY typ");
                return dc.Query<short, Cat>();
            },
            60 * 60 * 12
        );

        MakeCache(dc =>
            {
                dc.Sql("SELECT ").collst(Std.Empty).T(" FROM envs WHERE status > 0 ORDER BY typ");
                return dc.Query<short, Env>();
            },
            60 * 60 * 12
        );

        MakeCache(dc =>
            {
                dc.Sql("SELECT ").collst(Std.Empty).T(" FROM tags WHERE status > 0 ORDER BY typ");
                return dc.Query<short, Tag>();
            },
            60 * 60 * 12
        );

        MakeCache(dc =>
            {
                dc.Sql("SELECT ").collst(Std.Empty).T(" FROM syms WHERE status > 0 ORDER BY typ");
                return dc.Query<short, Sym>();
            },
            60 * 60 * 12
        );

        MakeCache(dc =>
            {
                dc.Sql("SELECT ").collst(Std.Empty).T(" FROM cers WHERE status > 0 ORDER BY typ");
                return dc.Query<short, Cer>();
            },
            60 * 60 * 12
        );

        MakeCache(dc =>
            {
                dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs ORDER BY typ, id");
                return dc.Query<short, Reg>();
            },
            60 * 60 * 12
        );

        MakeCache<OrgTwinCache>("org").Period = 360;
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
                    dc.Sql("SELECT first(id), count(id) FROM buys WHERE status = 1 AND adapted < @1 GROUP BY rtlid");
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