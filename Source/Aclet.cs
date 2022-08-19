﻿using ChainFx;

namespace ChainMart
{
    public struct Aclet : IData
    {
        internal int id;

        internal string name;

        internal string tel;

        internal short orgly;

        public void Read(ISource s, short msk = 255)
        {
            s.Get(nameof(id), ref id);
            s.Get(nameof(name), ref name);
            s.Get(nameof(tel), ref tel);
            s.Get(nameof(orgly), ref orgly);
        }

        public void Write(ISink s, short msk = 255)
        {
        }
    }
}