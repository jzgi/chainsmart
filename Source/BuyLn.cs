﻿using ChainFx;

namespace ChainMart
{
    public struct BuyLn : IData
    {
        public int supplyid;
        public string name;
        public short itemid;
        public decimal price;
        public short qty;
        public short qtyre; // qty reducted

        public void Read(ISource s, short proj = 0xff)
        {
            s.Get(nameof(supplyid), ref supplyid);
            s.Get(nameof(name), ref name);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(price), ref price);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(qtyre), ref qtyre);
        }

        public void Write(ISink s, short proj = 0xff)
        {
            s.Put(nameof(supplyid), supplyid);
            s.Put(nameof(name), name);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(price), price);
            s.Put(nameof(qty), qty);
            s.Put(nameof(qtyre), qtyre);
        }
    }
}