using System;
using SkyChain;

namespace Zhnt.Supply
{
    public class Do : IData, IKeyable<int>
    {
        public static readonly Uo Empty = new Uo();

        public const byte ID = 1, LATER = 2;

        public const short
            TYP_PRODUCT = 1,
            TYP_SERVICE = 2,
            TYP_EVENT = 3;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_PRODUCT, "产品拼团"},
            {TYP_SERVICE, "服务拼团"},
            {TYP_EVENT, "社工活动"},
        };


        internal int id;
        internal string tip;
        internal string unit;
        internal string unitip;
        internal decimal price;
        internal short min;
        internal short max;
        internal short least;
        internal short step;
        internal bool @extern;
        internal string addr;
        internal DateTime start;
        internal string author;
        internal bool icon;
        internal bool img;

        internal short qtys;
        internal decimal pays;

        public void Read(ISource s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            s.Get(nameof(tip), ref tip);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitip), ref unitip);
            s.Get(nameof(price), ref price);
            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
            s.Get(nameof(least), ref least);
            s.Get(nameof(step), ref step);
            s.Get(nameof(start), ref start);
            s.Get(nameof(addr), ref addr);
            s.Get(nameof(author), ref author);
            s.Get(nameof(@extern), ref @extern);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(qtys), ref qtys);
                s.Get(nameof(pays), ref pays);
                s.Get(nameof(icon), ref icon);
                s.Get(nameof(img), ref img);
            }
        }

        public void Write(ISink s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            s.Put(nameof(tip), tip);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitip), unitip);
            s.Put(nameof(price), price);
            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(least), least);
            s.Put(nameof(step), step);
            s.Put(nameof(start), start);

            if (string.IsNullOrEmpty(addr)) s.PutNull(nameof(addr));
            else s.Put(nameof(addr), addr);

            s.Put(nameof(author), author);
            s.Put(nameof(@extern), @extern);

            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(qtys), qtys);
                s.Put(nameof(pays), pays);
                s.Put(nameof(icon), icon);
                s.Put(nameof(img), img);
            }
        }

        public int Key => id;

        public bool HasIcon => icon;

        public bool HasImg => img;


        public bool IsOver(DateTime now) => false;
    }
}