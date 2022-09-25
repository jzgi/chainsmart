using ChainFx;

namespace ChainMart
{
    public class ItemMx : Entity
    {
        internal short id;
        internal int itemid;
        internal short idx;

        public override void Read(ISource s, short msk = 255)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(itemid), ref itemid);
            }
            s.Get(nameof(idx), ref idx);
        }

        public override void Write(ISink s, short msk = 255)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Put(nameof(itemid), itemid);
            }
            s.Put(nameof(idx), idx);
        }
    }
}