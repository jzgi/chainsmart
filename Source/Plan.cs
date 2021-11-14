using System;
using SkyChain;

namespace Revital
{
    /// <summary>
    /// The data model for a particular supply of standard item.
    /// </summary>
    public class Plan : _Doc, IKeyable<int>
    {
        public static readonly Plan Empty = new Plan();

        public const short
            TYP_ROUTINE = 1,
            TYP_FUTURE = 2;


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_ROUTINE, "常规"},
            {TYP_FUTURE, "预先"},
        };

        internal int id;
        internal int ctrid;
        internal short itemid;
        internal short cat;

        internal DateTime started;
        internal DateTime ended;
        internal DateTime filled;

        internal string nunit; // need
        internal short nx;
        internal short nmin;
        internal short nmax;
        internal short nstep;
        internal decimal nprice;
        internal decimal noff;

        internal string dunit; // distrib
        internal short dx;
        internal short dmin;
        internal short dmax;
        internal short dstep;
        internal decimal dprice;
        internal decimal doff;

        internal string sunit; // subscrib
        internal short sx;
        internal short smin;
        internal short smax;
        internal short sstep;
        internal decimal sprice;
        internal decimal soff;

        public override void Read(ISource s, byte proj = 15)
        {
            base.Read(s, proj);
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(cat), ref cat);
            s.Get(nameof(started), ref started);
            s.Get(nameof(ended), ref ended);
            s.Get(nameof(filled), ref filled);

            s.Get(nameof(nunit), ref nunit);
            s.Get(nameof(nx), ref nx);
            s.Get(nameof(nmin), ref nmin);
            s.Get(nameof(nmax), ref nmax);
            s.Get(nameof(nstep), ref nstep);
            s.Get(nameof(nprice), ref nprice);
            s.Get(nameof(noff), ref noff);

            s.Get(nameof(dunit), ref dunit);
            s.Get(nameof(dx), ref dx);
            s.Get(nameof(dmin), ref dmin);
            s.Get(nameof(dmax), ref dmax);
            s.Get(nameof(dstep), ref dstep);
            s.Get(nameof(dprice), ref dprice);
            s.Get(nameof(doff), ref doff);

            s.Get(nameof(sunit), ref sunit);
            s.Get(nameof(sx), ref sx);
            s.Get(nameof(smin), ref smin);
            s.Get(nameof(smax), ref smax);
            s.Get(nameof(sstep), ref sstep);
            s.Get(nameof(sprice), ref sprice);
            s.Get(nameof(soff), ref soff);
        }

        public override void Write(ISink s, byte proj = 15)
        {
            base.Write(s, proj);
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(cat), cat);
            s.Put(nameof(started), started);
            s.Put(nameof(ended), ended);
            s.Put(nameof(filled), filled);

            s.Put(nameof(nunit), nunit);
            s.Put(nameof(nx), nx);
            s.Put(nameof(nmin), nmin);
            s.Put(nameof(nmax), nmax);
            s.Put(nameof(nstep), nstep);
            s.Put(nameof(nprice), nprice);
            s.Put(nameof(noff), noff);

            s.Put(nameof(dunit), dunit);
            s.Put(nameof(dx), dx);
            s.Put(nameof(dmin), dmin);
            s.Put(nameof(dmax), dmax);
            s.Put(nameof(dstep), dstep);
            s.Put(nameof(dprice), dprice);
            s.Put(nameof(doff), doff);

            s.Put(nameof(sunit), sunit);
            s.Put(nameof(sx), sx);
            s.Put(nameof(smin), smin);
            s.Put(nameof(smax), smax);
            s.Put(nameof(sstep), sstep);
            s.Put(nameof(sprice), sprice);
            s.Put(nameof(soff), soff);
        }

        public int Key => id;
    }
}