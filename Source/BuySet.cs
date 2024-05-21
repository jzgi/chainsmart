using System;
using System.Collections.Generic;
using ChainFX;
using ChainFX.Nodal;

namespace ChainSmart;

/// <summary>
/// The output event queue for an org which contains b 
/// </summary>
public class BuySet
{
    readonly List<Buy> lst = new(16);

    public DateTime Since { get; internal set; } = DateTime.Now;


    public void Add(Buy v)
    {
        lock (this)
        {
            lst.Add(v);
        }
    }

    public void Dump(JsonBuilder bdr, DateTime now)
    {
        bdr.ARR_();

        lock (this)
        {
            // buying orders
            foreach (var o in lst)
            {
                bdr.Put(null, o);
            }

            // clear
            lst.Clear();

            Since = now;
        }

        bdr._ARR();
    }
}