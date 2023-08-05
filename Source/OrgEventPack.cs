using System;
using System.Collections.Generic;
using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

/// <summary>
/// The output event queue for an org which contains b 
/// </summary>
public class OrgEventPack : IPack<JsonBuilder>
{
    readonly List<string> news = new(16);

    readonly List<Buy> buys = new(16);

    readonly List<string> notes = new(16);


    private DateTime since = DateTime.Now;


    public DateTime Since => since;


    public void Add(string v)
    {
        lock (this)
        {
            news.Add(v);
        }
    }

    public void Add(Buy v)
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
            bdr.OBJ_();
            bdr.Put(nameof(news), news);
            bdr._OBJ();

            // buying orders
            foreach (var o in buys)
            {
                bdr.Put(null, o);
            }

            // notes 
            bdr.OBJ_();
            bdr.Put(nameof(notes), notes);
            bdr._OBJ();

            // clear
            news.Clear();
            buys.Clear();

            since = now;
        }
        bdr._ARR();
    }
}