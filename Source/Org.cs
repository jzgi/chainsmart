using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// An organizational unit.
    /// </summary>
    public class Org : Entity, IKeyable<int>
    {
        public static readonly Org Empty = new Org();

        public const short
            TYP_PRT = 0b01000, // parent
            TYP_SHP = 0b00001, // shop
            TYP_SRC = 0b00010, // source
            TYP_DST = 0b00100, // distributor
            TYP_MKT = TYP_PRT | TYP_SHP, // market
            TYP_ZON = TYP_PRT | TYP_SRC, // zone
            TYP_CTR = TYP_PRT | TYP_SRC | TYP_DST; // center

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_SHP, "商户"},
            {TYP_SRC, "产源"},
#if ZHNT
            {TYP_MKT, "市场"},
#else
            {TYP_SHP, "驿站"},
#endif
            {TYP_ZON, "供区"},
            {TYP_CTR, "中控"},
        };

        // id
        internal int id;

        // parent id, only if shop or source
        internal int prtid;

        // center id, only if market or shop
        internal int ctrid;

        internal string license;
        internal short regid;
        internal string addr;
        internal double x;
        internal double y;

        internal string tel;
        internal string link;
        internal bool trust;

        internal int sprid; // supervisor id
        internal string sprname;
        internal string sprtel;
        internal string sprim;
        internal int rvrid; // reviewer id 
        internal bool icon;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((proj & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(prtid), ref prtid);
                s.Get(nameof(ctrid), ref ctrid);
            }
            if ((proj & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(license), ref license);
                s.Get(nameof(regid), ref regid);
                s.Get(nameof(addr), ref addr);
                s.Get(nameof(x), ref x);
                s.Get(nameof(y), ref y);
                s.Get(nameof(tel), ref tel);
                s.Get(nameof(link), ref link);
                s.Get(nameof(trust), ref trust);
            }
            if ((proj & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(sprid), ref sprid);
                s.Get(nameof(sprname), ref sprname);
                s.Get(nameof(sprtel), ref sprtel);
                s.Get(nameof(sprim), ref sprim);
                s.Get(nameof(rvrid), ref rvrid);
                s.Get(nameof(icon), ref icon);
            }
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            if ((proj & MSK_BORN) == MSK_BORN)
            {
                if (prtid > 0) s.Put(nameof(prtid), prtid);
                else s.PutNull(nameof(prtid));

                if (ctrid > 0) s.Put(nameof(ctrid), ctrid);
                else s.PutNull(nameof(ctrid));
            }
            if ((proj & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(license), license);
                if (regid > 0) s.Put(nameof(regid), regid);
                else s.PutNull(nameof(regid));
                s.Put(nameof(addr), addr);
                s.Put(nameof(x), x);
                s.Put(nameof(y), y);
                s.Put(nameof(tel), tel);
                s.Put(nameof(link), link);
                s.Put(nameof(trust), trust);
            }
            if ((proj & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(sprid), sprid);
                s.Put(nameof(sprname), sprname);
                s.Put(nameof(sprtel), sprtel);
                s.Put(nameof(sprim), sprim);
                s.Put(nameof(rvrid), rvrid);
                s.Put(nameof(icon), icon);
            }
        }


        public int Key => id;

        public string Tel => tel;

        public string Im => sprim;

        public bool IsParentCapable => (typ & TYP_PRT) == TYP_PRT;

        public bool IsZone => typ == TYP_ZON;

        public bool IsOfZone => (typ & TYP_ZON) == TYP_ZON;

        public bool IsSource => typ == TYP_SRC;

        public bool IsOfSource => (typ & TYP_SRC) == TYP_SRC;

        public bool IsShop => typ == TYP_SHP;

        public bool IsOfShop => (typ & TYP_SHP) == TYP_SHP;

        public bool IsMarket => typ == TYP_MKT;

        public bool IsCenter => typ == TYP_CTR;

        public bool HasXy => IsMarket || IsSource || IsCenter;

        public bool HasCtr => IsOfShop;

        public string ShopName => IsMarket ? tip : name;

        public override string ToString() => name;
    }
}