using ChainFx;

namespace ChainSmart
{
    /// <summary>
    /// An organizational unit.
    /// </summary>
    public class Org : Entity, IKeyable<int>
    {
        public static readonly Org Empty = new Org();


        public const short
            TYP_BRD = 0b00000, // virtual
            TYP_PRT = 0b01000, // parent or leader
            TYP_SHP = 0b00001, // shop
            TYP_SRC = 0b00010, // source
            TYP_DST = 0b00100, // distributor
            TYP_MKT = TYP_PRT | TYP_SHP, // market
            TYP_ZON = TYP_PRT | TYP_SRC, // zone
            TYP_CTR = TYP_PRT | TYP_SRC | TYP_DST; // center

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_SHP, "摊铺"},
            {TYP_SRC, "产源"},
#if ZHNT
            {TYP_MKT, "市场"},
#else
            {TYP_SHP, "驿站"},
#endif
            {TYP_ZON, "供区"},
            {TYP_CTR, "中控"},
        };

        public const short
            STA_VOID = 0, STA_PRE = 1, STA_FINE = 2, STA_TOP = 4;

        public static readonly Map<short, string> States = new Map<short, string>
        {
            {STA_VOID, "停业"},
            {STA_PRE, "放假"},
            {STA_FINE, "正常"},
            {STA_TOP, "满负"},
        };


        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STU_CREATED, "新建"},
            {STU_ADAPTED, "调整"},
            {STU_OKED, "上线"},
        };


        // id
        internal int id;

        // parent id, only if shop or source
        internal int prtid;

        // center id, only if market or shop
        internal int ctrid;

        internal string ext; // extended territory name
        internal string legal; // legal name
        internal short regid;
        internal string addr;
        internal double x;
        internal double y;
        internal string tel;
        internal bool trust;
        internal string link;

        internal bool icon;
        internal JObj specs;
        internal bool pic;
        internal bool m1;
        internal bool m2;
        internal bool m3;
        internal bool m4;

        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(prtid), ref prtid);
                s.Get(nameof(ctrid), ref ctrid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(ext), ref ext);
                s.Get(nameof(legal), ref legal);
                s.Get(nameof(regid), ref regid);
                s.Get(nameof(addr), ref addr);
                s.Get(nameof(x), ref x);
                s.Get(nameof(y), ref y);
                s.Get(nameof(tel), ref tel);
                s.Get(nameof(trust), ref trust);
                s.Get(nameof(link), ref link);
                s.Get(nameof(specs), ref specs);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(icon), ref icon);
                s.Get(nameof(pic), ref pic);
                s.Get(nameof(m1), ref m1);
                s.Get(nameof(m2), ref m2);
                s.Get(nameof(m3), ref m3);
                s.Get(nameof(m4), ref m4);
            }
        }

        public override void Write(ISink s, short msk = 0xff)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                if (prtid > 0) s.Put(nameof(prtid), prtid);
                else s.PutNull(nameof(prtid));

                if (ctrid > 0) s.Put(nameof(ctrid), ctrid);
                else s.PutNull(nameof(ctrid));
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(ext), ext);
                s.Put(nameof(legal), legal);
                if (regid > 0) s.Put(nameof(regid), regid);
                else s.PutNull(nameof(regid));
                s.Put(nameof(addr), addr);
                s.Put(nameof(x), x);
                s.Put(nameof(y), y);
                s.Put(nameof(trust), trust);
                s.Put(nameof(tel), tel);
                s.Put(nameof(link), link);
                s.Put(nameof(specs), specs);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(icon), icon);
                s.Put(nameof(pic), pic);
                s.Put(nameof(m1), m1);
                s.Put(nameof(m2), m2);
                s.Put(nameof(m3), m3);
                s.Put(nameof(m4), m4);
            }
        }


        public int Key => id;

        public string Tel => tel;

        public int MarketId => EqMarket ? id : IsShop ? prtid : 0;

        public int ZoneId => EqZone ? id : IsSource ? prtid : 0;

        public bool IsParent => (typ & TYP_PRT) == TYP_PRT;

        public bool EqBrand => typ == TYP_BRD;

        public bool EqZone => typ == TYP_ZON;

        public bool IsZone => (typ & TYP_ZON) == TYP_ZON;

        public bool EqSource => typ == TYP_SRC;

        public bool IsSource => (typ & TYP_SRC) == TYP_SRC;

        public bool EqShop => typ == TYP_SHP;

        public bool IsShop => (typ & TYP_SHP) == TYP_SHP;

        public bool EqMarket => typ == TYP_MKT;

        public bool EqCenter => typ == TYP_CTR;

        public bool HasXy => EqMarket || EqSource || EqCenter;

        public bool HasCtr => IsShop;

        public bool IsTopOrg => prtid == 0;

        public string Name => name;

        public string Ext => ext;

        public override string ToString() => name;
    }
}