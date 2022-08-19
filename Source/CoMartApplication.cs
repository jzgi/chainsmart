﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Nodal;
using ChainFx.Web;
using static System.Data.IsolationLevel;

namespace ChainMart
{
    public class CoMartApplication : Application
    {
        // periodic polling and concluding ended lots 
        static readonly Thread cycler = new Thread(Cycle);

        /// <summary>
        /// The entry point of the application.
        /// </summary>
        public static async Task Main(string[] args)
        {
            // start the concluder thead
            // cycler.Start();

            AddComposite<BuyLn>();

            if (args.Contains("proxy"))
            {
                if (args.Contains("pub-p"))
                {
                    CreateService<ProxyService>("pub");
                }

                if (args.Contains("mgt-p"))
                {
                    CreateService<ProxyService>("mgt");
                }
            }
            else
            {
                CacheUp();

                CreateService<WwwService>("www");

                CreateService<MgtService>("mgt");

                CreateService<FedService>("fed");
            }

            await StartAsync();
        }


        public static void CacheUp()
        {
            Cache(dc =>
                {
                    dc.Sql("SELECT ").collst(Cat.Empty).T(" FROM cats WHERE state > 0 ORDER BY idx");
                    return dc.Query<short, Cat>();
                }, 3600 * 24
            );

            Cache(dc =>
                {
                    dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs ORDER BY typ, id");
                    return dc.Query<short, Reg>();
                }, 3600 * 24
            );

            Cache(dc =>
                {
                    dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items ORDER BY typ, id");
                    return dc.Query<short, Item>();
                }, 60 * 15
            );

            CacheObject<int, Org>((dc, id) =>
                {
                    dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE id = @1");
                    return dc.QueryTop<Org>(p => p.Set(id));
                }, 60 * 15
            );

            Cache(dc =>
                {
                    dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ >= ").T(Org.TYP_DST);
                    return dc.Query<int, Org>();
                }, 60 * 15
            );

            CacheMap((DbContext dc, int orgid) =>
                {
                    dc.Sql("SELECT ").collst(Product.Empty).T(" FROM products WHERE orgid = @1 AND state > 0 ORDER BY state DESC");
                    return dc.Query<int, Product>(p => p.Set(orgid));
                }, 60 * 15
            );
        }

        static async void Cycle(object state)
        {
            var lst = new List<int>(64);
            while (true)
            {
                Thread.Sleep(60 * 1000);

                var today = DateTime.Today;
                // WAR("cycle: " + today);

                // to succeed
                lst.Clear();
                try
                {
                    using (var dc = NewDbContext())
                    {
                        // dc.Sql("SELECT id FROM lots WHERE status = ").T(Flow_.STATUS_CREATED).T(" AND ended < @1 AND qtys >= min");
                        await dc.QueryAsync(p => p.Set(today));
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

                    // to abort
                    lst.Clear();
                    using (var dc = NewDbContext())
                    {
                        // dc.Sql("SELECT id FROM lots WHERE status = ").T(Flow_.STATUS_CREATED).T(" AND ended < @1 AND qtys < min");
                        await dc.QueryAsync(p => p.Set(today));
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
            }
        }
    }
}