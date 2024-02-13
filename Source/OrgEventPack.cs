using System;
using System.Collections.Generic;
using ChainFX;
using ChainFX.Nodal;

namespace ChainSmart;

/// <summary>
/// The output event queue for an org which contains b 
/// </summary>
public class OrgEventPack : IPack<JsonBuilder>
{
    readonly List<string> news = new(16);

    readonly List<Buy> buys = new(16);

    private DateTime since = DateTime.Now;


    public DateTime Since => since;


    public void AddNew(string v)
    {
        lock (this)
        {
            news.Add(v);
        }
    }

    public void AddBuy(Buy v)
    {
        lock (this)
        {
            buys.Add(v);
        }
    }


    public void Dump(JsonBuilder bdr, DateTime now)
    {
        bdr.ARR_();
        lock (this)
        {
            // bix names 
            if (news.Count > 0)
            {
                bdr.OBJ_();
                bdr.Put(nameof(news), news);
                bdr._OBJ();
            }

            // buying orders
            foreach (var o in buys)
            {
                bdr.Put(null, o);
            }

            // clear
            news.Clear();
            buys.Clear();

            since = now;
        }
        bdr._ARR();
    }
}