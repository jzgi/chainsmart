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
    readonly List<string> newlst = new(16);

    readonly List<Buy> buylst = new(16);


    public DateTime Since { get; internal set; } = DateTime.Now;


    public void AddNew(string v)
    {
        lock (this)
        {
            newlst.Add(v);
        }
    }

    public void AddBuy(Buy v)
    {
        lock (this)
        {
            buylst.Add(v);
        }
    }


    public void Dump(JsonBuilder bdr, DateTime now)
    {
        bdr.ARR_();

        lock (this)
        {
            // bix names 
            if (newlst.Count > 0)
            {
                bdr.OBJ_();
                bdr.Put(nameof(newlst), newlst);
                bdr._OBJ();
            }

            // buying orders
            foreach (var o in buylst)
            {
                bdr.Put(null, o);
            }

            // clear
            newlst.Clear();
            buylst.Clear();

            Since = now;
        }

        bdr._ARR();
    }
}